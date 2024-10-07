using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScript : MonoBehaviour
{
    public VirtualHeadComponent virtualHeadComponent;  // Reference to the VirtualHeadComponent
    public PoseEstimator poseEstimator;  // Reference to your keypoint detection component
    public float updateInterval = 0.5f;  // Interval to update keypoints

    private float timer;

    void Start()
    {
        if (virtualHeadComponent == null || poseEstimator == null)
        {
            Debug.LogError("Missing component references in MainScript.");
            return;
        }

        timer = updateInterval;
    }

    void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            UpdateKeypoints();
            timer = updateInterval;
        }
    }

    void UpdateKeypoints()
    {
        // Fetch the latest keypoints from your keypoint detection system
        Utils.Keypoint[] keypoints = poseEstimator.GetMainPose();
        Debug.Log(keypoints[0].position);

        // Update the virtual head component with the new keypoints
        //virtualHeadComponent.UpdateHeadPosition(keypoints);
    }
}
