using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualHeadComponent : MonoBehaviour
{

    public Transform virtualHead;
    public Camera arcGISCamera;
    public float moveSpeed = .5f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (virtualHead != null && arcGISCamera != null)
        {
            if (virtualHead.position != arcGISCamera.transform.position || virtualHead.rotation != arcGISCamera.transform.rotation)
            {
                virtualHead.position = arcGISCamera.transform.position;
                virtualHead.rotation = arcGISCamera.transform.rotation;
            }
 
            HandleMovement();
        }
    }

    
    void HandleMovement()
    {
        Vector3 direction = Vector3.zero;

        // Capture input
        if (Input.GetKey(KeyCode.W))  // Move forward
        {
            direction += arcGISCamera.transform.forward;
        }

        if (Input.GetKey(KeyCode.S))  // Move backward
        {
            direction -= arcGISCamera.transform.forward;
        }

        if (Input.GetKey(KeyCode.A))  // Move left
        {
            direction -= arcGISCamera.transform.right;
        }

        if (Input.GetKey(KeyCode.D))  // Move right
        {
            direction += arcGISCamera.transform.right;
        }

        if (Input.GetKey(KeyCode.Q))  // Move up
        {
            direction += arcGISCamera.transform.up;
        }

        if (Input.GetKey(KeyCode.E))  // Move right
        {
            direction -= arcGISCamera.transform.up;
        }


        // Normalize direction to ensure consistent movement speed
        direction.Normalize();

        // Move both the ArcGIS main camera and the VirtualHead
        arcGISCamera.transform.position += direction * moveSpeed * Time.deltaTime;
        virtualHead.position += direction * moveSpeed * Time.deltaTime;
    }
}
