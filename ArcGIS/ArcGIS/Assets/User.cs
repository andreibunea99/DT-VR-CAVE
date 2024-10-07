using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class User : MonoBehaviour
{
    // Start is called before the first frame update
    private PoseSkeleton body;
    private Utils.Keypoint[] pose;
    private Transform[] eyes = new Transform[2];
    private List<float> diffEyesX = new List<float>();
    private List<float> diffEeyesY = new List<float>();
    private float meanDiffX = 0f;
    private float meanDiffY = 0f;
    private int occurences = 0;
    private const int maxOccurences = 100;

    public User() { }
    
    public void setSkeleton(PoseSkeleton skeleton, Vector3 videoScreenOffset)
    {
        this.body = skeleton;

        bool leftEye = false;
        bool rightEye = false;
        
        for (int i = 0; i < skeleton.keypoints.Length; i++)
        {
            if (!skeleton.keypoints[i].GetComponent<MeshRenderer>().enabled)
            {
                continue;
            }
            if (skeleton.keypoints[i].gameObject.name == "leftEye")
            {
                eyes[0] = skeleton.keypoints[i];
                leftEye = true;
            } else if (skeleton.keypoints[i].gameObject.name == "rightEye")
            {
                eyes[1] = skeleton.keypoints[i];
                rightEye = true;
            }

            if (leftEye && rightEye)
            {
                break;
            }
        }

        if (eyes[0] == null || eyes[1] == null)
        {
            return;
        }

        if (leftEye && rightEye)
        {
            // Calculate the difference in position between the eyes
            float diffX = Mathf.Abs(eyes[0].position.x - eyes[1].position.x);
            float diffY = Mathf.Abs(eyes[0].position.y - eyes[1].position.y);

            if (occurences < maxOccurences)
            {
                diffEyesX.Add(diffX);
                diffEeyesY.Add(diffY);
                occurences++;
            }

            if (occurences == maxOccurences)
            {
                meanDiffX = CalculateMean(diffEyesX);
                meanDiffY = CalculateMean(diffEeyesY);
            }
        }
        else if (occurences >= maxOccurences)
        {
            // If one is not enabled, approximate the position using the mean difference between eyes
            if (!leftEye && rightEye)
            {
                eyes[0].position = new Vector3(eyes[1].position.x - meanDiffX, eyes[1].position.y - meanDiffY, eyes[1].position.z);
                //Debug.Log("Left eye approximated using mean difference.");
            }
            else if (leftEye && !rightEye)
            {
                eyes[1].position = new Vector3(eyes[0].position.x + meanDiffX, eyes[0].position.y + meanDiffY, eyes[0].position.z);
                //Debug.Log("Left right approximated using mean difference.");
            }
        }

        eyes[0].position -= videoScreenOffset;
        eyes[1].position -= videoScreenOffset;

        //Debug.Log($"Eye position updated! Left eye: {this.eyes[0].position}, Right eye: {this.eyes[1].position}");
    }

    private float CalculateMean(List<float> differences)
    {
        float sum = 0f;
        foreach (float diff in differences)
        {
            sum += diff;
        }

        return sum / differences.Count;
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }
}
