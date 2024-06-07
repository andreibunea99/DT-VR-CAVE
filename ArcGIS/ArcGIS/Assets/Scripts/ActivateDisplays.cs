using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateDisplays : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Number of displays connected: " + Display.displays.Length);

        // Activate all connected displays
        for (int i = 0; i < Display.displays.Length; i++)
        {
            if (i < Display.displays.Length)
            {
                Display.displays[i].Activate();
                Debug.Log("Display " + i + " activated");
            }
            else
            {
                Debug.LogWarning("Display " + i + " not connected");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
