using UnityEngine;

public class AlignCamerasComponent : MonoBehaviour
{
    public Camera orthoCamera;
    public Transform plane;

    void Start()
    {
        // Ensure the camera is in orthographic mode
        orthoCamera.orthographic = true;

        // Calculate the orthographic size
        float planeHeight = plane.localScale.y;
        orthoCamera.orthographicSize = planeHeight / 2.0f;

        // Calculate the aspect ratio based on plane's scale
        float planeWidth = plane.localScale.x;
        float aspectRatio = planeWidth / planeHeight;
        orthoCamera.aspect = aspectRatio;

        // Position the camera to look at the center of the plane
        Vector3 planeCenter = plane.position;

        // Determine a suitable distance from the plane based on its rotation
        Vector3 offset = new Vector3(0, 0, -10);

        // Adjust camera position based on plane rotation
        Vector3 cameraPosition = planeCenter + plane.rotation * offset;

        orthoCamera.transform.position = cameraPosition;

        // Make the camera look at the center of the plane
        orthoCamera.transform.LookAt(planeCenter);

        // Ensure the camera's up direction matches the plane's up direction
        orthoCamera.transform.up = plane.up;
    }
}
