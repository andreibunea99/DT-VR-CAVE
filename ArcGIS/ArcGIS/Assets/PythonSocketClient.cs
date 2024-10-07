using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

public class PythonSocketClient : MonoBehaviour
{

    private const string Host = "127.0.0.1";
    private const int Port = 65432;


    [System.Serializable]
    public class TransformationRequest
    {
        public float[][] real_world_coords;
        public float[][] image_coords;
        public float[][] keypoints;
    }


    [System.Serializable]
    public class Keypoint
    {
        public float x;
        public float y;
    }

    [System.Serializable]
    public class TransformationResponse
    {
        public Keypoint[] transformed_keypoints;
    }



    // Start is called before the first frame update
    void Start()
    {
        // Define the request data
        TransformationRequest request = new TransformationRequest
        {
            real_world_coords = new float[][]
            {
                new float[] {0, 0},
                new float[] {0, 1},
                new float[] {1, 1},
                new float[] {1, 0}
            },
            image_coords = new float[][]
            {
                new float[] {10, 20},
                new float[] {30, 50},
                new float[] {60, 80},
                new float[] {90, 100}
            },
            keypoints = new float[][]
            {
                new float[] {0.5f, 0.5f},
                new float[] {0.3f, 0.7f}
            }
        };

        // Convert the request to JSON
        string requestData = JsonConvert.SerializeObject(request);

        // Connect to the Python server
        using (TcpClient client = new TcpClient(Host, Port))
        {
            NetworkStream stream = client.GetStream();

            // Send data
            byte[] data = Encoding.UTF8.GetBytes(requestData);
            stream.Write(data, 0, data.Length);

            // Receive response
            byte[] responseBuffer = new byte[4096];
            int bytesRead = stream.Read(responseBuffer, 0, responseBuffer.Length);
            string responseData = Encoding.UTF8.GetString(responseBuffer, 0, bytesRead);

            // Deserialize the response
            TransformationResponse response = JsonConvert.DeserializeObject<TransformationResponse>(responseData);

            Debug.Log(response);

            foreach (Keypoint point in response.transformed_keypoints)
            {
                Debug.Log($"Transformed Keypoint: ({point.x}, {point.y})");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
