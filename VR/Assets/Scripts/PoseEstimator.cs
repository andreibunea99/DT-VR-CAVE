using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using Unity.Barracuda;

public class PoseEstimator : MonoBehaviour
{

    [Tooltip("The requested webcam dimmensions")]
    public Vector2Int webcamDims = new Vector2Int(1280, 720);

    [Tooltip("The requested webcam frame rate")]
    public int webcamFPS = 60;

    [Tooltip("Use webcam feed as input")]
    public bool useWebcam = false;

    [Tooltip("The screen for viewing preprocessed images")]
    public Transform videoScreen;

    private WebCamTexture webcamTexture;

    private Vector2Int videoDims;

    private RenderTexture videoTexture;

    public enum ModelType
    {
        MobileNet,
        ResNet50
    }

    [Tooltip("The ComputeShader that will perform the model-specific preprocessing")]
    public ComputeShader posenetShader;

    [Tooltip("The model architecture used")]
    public ModelType modelType = ModelType.ResNet50;

    [Tooltip("Use GPU for preprocessing")]
    public bool useGPU = true;

    [Tooltip("The dimensions of the image being fed to the model")]
    public Vector2Int imageDims = new Vector2Int(256, 256);

    // Target dimensions for model input
    private Vector2Int targetDims;

    // Used to scale the input image dimensions while maintaining aspect ratio
    private float aspectRatioScale;

    // The texture used to create input tensor
    private RenderTexture rTex;

    // The preprocessing function for the current model type
    private System.Action<float[]> preProcessFunction;

    // Stores the input data for the model
    private Tensor input;

    [Tooltip("The MobileNet model asset file to use when performing inference")]
    public NNModel mobileNetModelAsset;

    [Tooltip("The ResNet50 model asset file to use when performing inference")]
    public NNModel resnetModelAsset;

    [Tooltip("The backend to use when performing inference")]
    public WorkerFactory.Type workerType = WorkerFactory.Type.Auto;

    private struct Engine
    {
        public WorkerFactory.Type workerType;
        public IWorker worker;
        public ModelType modelType;

        public Engine(WorkerFactory.Type workerType, Model model, ModelType modelType)
        {
            this.workerType = workerType;
            worker = WorkerFactory.CreateWorker(workerType, model);
            this.modelType = modelType;
        }
    }


    // The interface used to execute the neural network
    private Engine engine;

    // The name for the heatmap layer in the model asset
    private string heatmapLayer;

    // The name for the offsets layers in the model asset
    private string offsetsLayer;

    private string displacementFWDLayer;
    private string displacementBWDLayer;

    // The name for the Sigmoid layer that returns the heatmap predictions
    private string predictionLayer = "heatmap_predictions";


    public enum EstimationType
    {
        MultiPose,
        SinglePose
    }


    [Tooltip("The type of pose estimation to be performed")]
    public EstimationType estimationType = EstimationType.SinglePose;

    private Utils.Keypoint[][] poses; 


    private void InitializeBarracuda()
    {
        // The compiled model used for inference
        Model m_RunTimeModel;

        if (modelType == ModelType.MobileNet)
        {
            preProcessFunction = Utils.PreprocessMobileNet;

            m_RunTimeModel = ModelLoader.Load(mobileNetModelAsset);
            displacementFWDLayer = m_RunTimeModel.outputs[2];
            displacementBWDLayer = m_RunTimeModel.outputs[3];
        }
        else
        {
            preProcessFunction = Utils.PreprocessResNet;

            m_RunTimeModel = ModelLoader.Load(resnetModelAsset);
            displacementFWDLayer = m_RunTimeModel.outputs[3];
            displacementBWDLayer = m_RunTimeModel.outputs[2];
        }

        heatmapLayer = m_RunTimeModel.outputs[0];
        offsetsLayer = m_RunTimeModel.outputs[1];

        // Create a model builder to modify the m_RunTimeModel
        ModelBuilder modelBuilder = new ModelBuilder(m_RunTimeModel);

        // Add a mew Sigmoid layer
        modelBuilder.Sigmoid(predictionLayer, heatmapLayer);

        // Validate if backend is supported, otherwise use fallback type.
        workerType = WorkerFactory.ValidateType(workerType);

        // Create a worker that will execute the model with the selected backend
        engine = new Engine(workerType, modelBuilder.model, modelType);

    }



    private void InitializeVideoScreen(int width, int height, bool mirrorScreen)
    {
        // Set the render mode for the video player
        videoScreen.GetComponent<VideoPlayer>().renderMode = VideoRenderMode.RenderTexture;

        // Use new videoTexture for Video Player
        videoScreen.GetComponent<VideoPlayer>().targetTexture = videoTexture;

        if (mirrorScreen)
        {
            // Flip the VideoScreen around the Y-Axis
            videoScreen.rotation = Quaternion.Euler(0, 180, 0);

            // Invert the scale value for the Z-Axis
            videoScreen.localScale = new Vector3(videoScreen.localScale.x, videoScreen.localScale.y, -1f);
        }

        // Apply the new videoTexture to the VideoScreen GameObject
        videoScreen.gameObject.GetComponent<MeshRenderer>().material.shader = Shader.Find("Unlit/Texture");
        videoScreen.gameObject.GetComponent<MeshRenderer>().material.SetTexture("_MainTex", videoTexture);

        // Adjust the VideoScreen dimensions for the new videoTexture
        videoScreen.localScale = new Vector3(width, height, videoScreen.localScale.z);

        // Adjust the VideoScreen position for the new videoTexture
        videoScreen.position = new Vector3(width / 2, height / 2, 1);
    }


    private void InitializeCamera()
    {
        // Get a reference to the Main Camera GameObject
        GameObject mainCamera = GameObject.Find("Main Camera");

        // Adjust the camera position to account for updates to the VideoScreen
        mainCamera.transform.position = new Vector3(videoDims.x / 2, videoDims.y / 2, -10f);

        // Render objects with no perspective
        mainCamera.GetComponent<Camera>().orthographic = true;

        // Adjust the camera size to account for updates to the VideoScreen
        mainCamera.GetComponent<Camera>().orthographicSize = videoDims.y / 2;
    }


    private void ProcessImageGPU(RenderTexture image, string functionName)
    {
        int numthreads = 8;

        // Get the index for the specified function in the Compute Shader
        int kernelHandle = posenetShader.FindKernel(functionName);

        // Define a temporary HDR RenderTexture
        RenderTexture result = RenderTexture.GetTemporary(image.width, image.height, 24, RenderTextureFormat.ARGBHalf);

        result.enableRandomWrite = true;
        result.Create();

        posenetShader.SetTexture(kernelHandle, "Result", result);
        posenetShader.SetTexture(kernelHandle, "InputImage", image);

        posenetShader.Dispatch(kernelHandle, result.width / numthreads, result.height / numthreads, 1);

        Graphics.Blit(result, image);

        RenderTexture.ReleaseTemporary(result);
    }


    private void ProcessImage(RenderTexture image)
    {
        if (useGPU)
        {
            ProcessImageGPU(image, preProcessFunction.Method.Name);

            // Create a Tensor of shape [1, image.height, image.width, 3]
            input = new Tensor(image, channels: 3);
        }
        else
        {
            // Create a Tensor of shape [1, image.height, image.width, 3]
            input = new Tensor(image, channels: 3);

            // Download the tensor data to an array
            float[] tensor_array = input.data.Download(input.shape);

            // Apply preprocessing steps
            preProcessFunction(tensor_array);

            // Update input tensor with new color data
            input = new Tensor(input.shape.batch,
                                input.shape.height,
                                input.shape.width,
                                input.shape.channels,
                                tensor_array);
        }
    }


    private void ProcessOutput(IWorker engine)
    {
        // Get the model output
        Tensor heatmaps = engine.PeekOutput(predictionLayer);
        Tensor offsets = engine.PeekOutput(offsetsLayer);
        Tensor displacementFWD = engine.PeekOutput(displacementFWDLayer);
        Tensor displacementBWD = engine.PeekOutput(displacementBWDLayer);

        // Calculate the stride used to scale down the iput image
        int stride = (imageDims.y - 1) / (heatmaps.shape.height - 1);
        stride -= (stride % 8);

        if (estimationType == EstimationType.SinglePose)
        {
            // Initialize the array of Keypoint arrays
            poses = new Utils.Keypoint[1][];
            poses[0] = Utils.DecodeSinglePose(heatmaps, offsets, stride);
        }
        else
        {

        }

        // Release the resources allocated for the output Tensors
        heatmaps.Dispose();
        offsets.Dispose();
        displacementFWD.Dispose();
        displacementBWD.Dispose();
    }


    // Start is called before the first frame update
    void Start()
    {
        if (useWebcam)
        {
            // Limit application framerate to the target webcam framerate
            Application.targetFrameRate = webcamFPS;

            // Create a new WebCamTexture
            webcamTexture = new WebCamTexture(webcamDims.x, webcamDims.y, webcamFPS);

            // Start the camera
            webcamTexture.Play();

            // Deactivate the Video Player
            videoScreen.GetComponent<VideoPlayer>().enabled = false;

            // Update the VideoDims
            videoDims.y = webcamTexture.height;
            videoDims.x = webcamTexture.width;
        }
        else
        {
            videoDims.y = (int)videoScreen.GetComponent<VideoPlayer>().height;
            videoDims.x = (int)videoScreen.GetComponent<VideoPlayer>().width;
        }

        // Create a new videoTexture using the current video dimensions
        videoTexture = RenderTexture.GetTemporary(videoDims.x, videoDims.y, 24, RenderTextureFormat.ARGBHalf);

        // Initialize the videoScreen
        InitializeVideoScreen(videoDims.x, videoDims.y, useWebcam);

        // Adjust the camera based on the source video dimensions
        InitializeCamera();

        // Adjust the input dimensions to maintain the source aspect ration
        aspectRatioScale = (float)videoTexture.width / videoTexture.height;
        targetDims.x = (int)(imageDims.y * aspectRatioScale);
        imageDims.x = targetDims.x;

        rTex = RenderTexture.GetTemporary(imageDims.x, imageDims.y, 24, RenderTextureFormat.ARGBHalf);

        InitializeBarracuda();

    }

    // Update is called once per frame
    void Update()
    {
        // Copy webcamTexture to videoTexture if using webcam
        if (useWebcam)
        {
            Graphics.Blit(webcamTexture, videoTexture);
        }

        // PRevent the input dimensions from going too low for the model
        imageDims.x = Mathf.Max(imageDims.x, 130);
        imageDims.y = Mathf.Max(imageDims.y, 130);

        // Update the input dimensions while maintaining the source aspect ratio
        if (imageDims.x != targetDims.x)
        {
            aspectRatioScale = (float)videoTexture.height / videoTexture.width;
            targetDims.y = (int)(imageDims.x * aspectRatioScale);
            imageDims.y = targetDims.y;
            targetDims.x = imageDims.x;
        }

        if (imageDims.y != targetDims.y)
        {
            aspectRatioScale = (float)videoTexture.width / videoTexture.height;
            targetDims.x = (int)(imageDims.y * aspectRatioScale);
            imageDims.x = targetDims.x;
            targetDims.y = imageDims.y;
        }

        // Update the rTex dimensions to the new input dimensions
        if (imageDims.x != rTex.width || imageDims.y != rTex.height)
        {
            RenderTexture.ReleaseTemporary(rTex);
            rTex = RenderTexture.GetTemporary(imageDims.x, imageDims.y, 24, rTex.format);
        }

        Graphics.Blit(videoTexture, rTex);

        // Prepare the input image to be fed to the selected model
        ProcessImage(rTex);

        if (engine.modelType != modelType || engine.workerType != workerType)
        {
            engine.worker.Dispose();
            InitializeBarracuda();
        }

        // Execute the neural network with the provided input
        engine.worker.Execute(input);

        // Release GPU resources allocated for the Tensor
        input.Dispose();

        // Decode the keypoint coordinates from the model output
        ProcessOutput(engine.worker);
    }


    // OnDisable is called when the MonoBehaviour becomes diabled or inactive
    private void OnDisable()
    {
        // Release the resources allocated for the inference
        engine.worker.Dispose();
    }
}
