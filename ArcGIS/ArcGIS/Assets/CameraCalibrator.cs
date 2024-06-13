using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCalibrator : MonoBehaviour
{
    public Camera[] displayCameras;
    public Transform[] wallTransforms;
    public Vector2[] displaySizes;  // in meters (width, height)
    public float viewerDistance;  // in meters, distance from the viewer to the center of the CAVE

    // Start is called before the first frame update
    void Start()
    {
        CalibrateCameras();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void CalibrateCameras()
    {
        for (int i = 0; i < displayCameras.Length; i++)
        {
            // Position the camera in front of the wall
            Vector3 wallPosition = wallTransforms[i].position;
            Vector3 directionToWall = wallPosition - Vector3.zero;
            directionToWall.Normalize();
            displayCameras[i].transform.position = wallPosition - directionToWall * viewerDistance;
            displayCameras[i].transform.LookAt(wallPosition);

            // Adjust the camera's field of view and aspect ratio
            float wallWidth = displaySizes[i].x;
            float wallHeight = displaySizes[i].y;
            displayCameras[i].aspect = wallWidth / wallHeight;
            float fov = 2.0f * Mathf.Atan(wallHeight / (2.0f * viewerDistance)) * Mathf.Rad2Deg;
            displayCameras[i].fieldOfView = fov;
        }
    }
}
