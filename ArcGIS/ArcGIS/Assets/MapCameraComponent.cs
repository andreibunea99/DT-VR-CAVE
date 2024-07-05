using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCameraComponent : MonoBehaviour
{
    public Camera arcGISCamera;

    void Start()
    {
        if (arcGISCamera != null)
        {
            // Set the starting position of the camera
            Vector3 startPosition = arcGISCamera.transform.position;
            startPosition.y = 10;
            arcGISCamera.transform.position = startPosition;

            // Modify the x rotation of the camera by 10 degrees
            Vector3 startRotation = arcGISCamera.transform.rotation.eulerAngles;
            startRotation.x = 10;
            arcGISCamera.transform.rotation = Quaternion.Euler(startRotation);

            Debug.Log("ArcGIS Camera start position set to: " + arcGISCamera.transform.position);
            Debug.Log("ArcGIS Camera start rotation set to: " + arcGISCamera.transform.rotation.eulerAngles);
        }
        else
        {
            Debug.LogError("ArcGIS Camera is not assigned.");
        }
    }

    void Update()
    {
        // Any other update logic can go here
    }
}
