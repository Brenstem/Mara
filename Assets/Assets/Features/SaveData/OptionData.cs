using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OptionData : Data
{
    public byte[][] controlKeyArray;
    public string[] valueArray;
    public float mouseSensitivity;
    public int qualityLevel;
    public bool isFullscreen;
    public int width;
    public int height;

    public OptionData(Dictionary<System.Guid, string> controls)
    {
        controlKeyArray = new byte[controls.Keys.Count][];
        valueArray = new string[controls.Values.Count];
        path = "controls";

        width = Screen.width;
        height = Screen.height;
        isFullscreen = Screen.fullScreen;
        qualityLevel = QualitySettings.GetQualityLevel();

        int i = 0;
        foreach (var key in controls.Keys)
        {
            controlKeyArray[i] = key.ToByteArray();
            valueArray[i] = controls[key];
            i++;
        }
    }
}
