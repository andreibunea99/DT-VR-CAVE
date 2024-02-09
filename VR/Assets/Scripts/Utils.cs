using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static void PreprocessMobileNet(float[] tensor)
    {
        // Normalize the values to the range [-1, 1]
        System.Threading.Tasks.Parallel.For(0, tensor.Length, (int i) =>
        {
            tensor[i] = (float)(2.0f * tensor[i] / 1.0f) - 1.0f;
        });
    }

    public static void PreprocessResNet(float[] tensor)
    {
        System.Threading.Tasks.Parallel.For(0, tensor.Length / 3, (int i) =>
        {
            tensor[i * 3 + 0] = (float)tensor[i * 3 + 0] * 255f - 123.15f;
            tensor[i * 3 + 1] = (float)tensor[i * 3 + 1] * 255f - 115.90f;
            tensor[i * 3 + 2] = (float)tensor[i * 3 + 2] * 255f - 103.06f;
        });
    }
}
