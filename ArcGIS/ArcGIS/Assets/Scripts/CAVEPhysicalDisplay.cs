using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CAVEPhysicalDisplay : MonoBehaviour
{
    public Camera displayCamera;
    public RenderTexture renderTexture;
    // Start is called before the first frame update
    void Start()
    {
        if (displayCamera != null && renderTexture != null)
        {
            displayCamera.targetTexture = renderTexture;
            //Debug.LogWarning("Textures generated successfully");
        }   
        else
        {
            //Debug.LogError("Camera or RenderTexture not assigned as PhysicalDisplay script on " + gameObject.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
