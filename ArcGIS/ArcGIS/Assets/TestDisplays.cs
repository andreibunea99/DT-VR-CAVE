using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDisplays : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Number of Displays: " + Display.displays.Length);
        for (int i = 0; i < Display.displays.Length; i++)
        {
            Debug.Log("Display " + i + ": " + Display.displays[i].renderingWidth + "x" + Display.displays[i].renderingHeight);
        }
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
