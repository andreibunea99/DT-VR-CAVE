using UnityEngine;

public class AssignCamerasToDisplays : MonoBehaviour
{
    public Camera frontCamera;
    public Camera leftCamera;
    public Camera rightCamera;

    void Start()
    {
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
            frontCamera.targetDisplay = 1;
            Debug.Log("Front camera assigned to display 1");
        }
        else
        {
            Debug.LogWarning("Display 1 not available");
        }

        if (Display.displays.Length > 2)
        {
            Display.displays[2].Activate();
            leftCamera.targetDisplay = 2;
            Debug.Log("Left camera assigned to display 2");
        }
        else
        {
            Debug.LogWarning("Display 2 not available");
        }

        if (Display.displays.Length > 3)
        {
            Display.displays[3].Activate();
            rightCamera.targetDisplay = 3;
            Debug.Log("Right camera assigned to display 3");
        }
        else
        {
            Debug.LogWarning("Display 3 not available");
        }
    }
}
