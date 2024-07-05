using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualHeadComponent : MonoBehaviour
{
    public Transform virtualHead;
    public Camera arcGISCamera;
    public float moveSpeed = 20.0f;
    public float rotationSpeed = 100.0f;

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

                // Set virtual head rotation to match arcGISCamera rotation except for x-axis
                Quaternion cameraRotation = arcGISCamera.transform.rotation;
                Vector3 euler = cameraRotation.eulerAngles;
                euler.x = virtualHead.rotation.eulerAngles.x;  // Keep the x-axis rotation of the virtual head
                virtualHead.rotation = Quaternion.Euler(euler);
            }

            HandleMovement();
        }
    }

    void HandleMovement()
    {
        Vector3 direction = Vector3.zero;

        // Capture input for movement
        if (Input.GetKey(KeyCode.Z))  // Move up in world space
        {
            direction += Vector3.up;
        }

        if (Input.GetKey(KeyCode.X))  // Move down in world space
        {
            direction += Vector3.down;
        }

        if (Input.GetKey(KeyCode.LeftArrow))  // Move left in world space
        {
            direction += Vector3.left;
        }

        if (Input.GetKey(KeyCode.RightArrow))  // Move right in world space
        {
            direction += Vector3.right;
        }

        if (Input.GetKey(KeyCode.UpArrow))  // Move forward in world space
        {
            direction += Vector3.forward;
        }

        if (Input.GetKey(KeyCode.DownArrow))  // Move backward in world space
        {
            direction += Vector3.back;
        }

        // Normalize direction to ensure consistent movement speed
        direction.Normalize();

        // Log direction only if it's non-zero
        if (direction != Vector3.zero)
        {
            Debug.Log("Movement Direction: " + direction);
        }

        // Move both the ArcGIS main camera and the VirtualHead
        arcGISCamera.transform.position += direction * moveSpeed * Time.deltaTime;
        virtualHead.position = arcGISCamera.transform.position;

        // Capture input for rotation
        float rotation = 0.0f;
        if (Input.GetKey(KeyCode.C))  // Rotate left
        {
            rotation = -rotationSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.V))  // Rotate right
        {
            rotation = rotationSpeed * Time.deltaTime;
        }

        // Apply rotation to both the ArcGIS main camera and the VirtualHead
        if (rotation != 0.0f)
        {
            arcGISCamera.transform.Rotate(0, rotation, 0, Space.World);
            Vector3 euler = virtualHead.rotation.eulerAngles;
        }
    }
}
