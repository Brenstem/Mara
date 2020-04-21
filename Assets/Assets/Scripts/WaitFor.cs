using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//https://answers.unity.com/questions/309320/yield-return-to-wait-for-some-frame-problem.html
public static class WaitFor
{
    public static IEnumerator Frames(int frameCount)
    {
        while (frameCount > 0)
        {
            frameCount--;
            yield return null;
        }
    }
}