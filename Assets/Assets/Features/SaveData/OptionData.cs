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
    public int refreshRate;
    public int width;
    public int height;
    public int currentLanguage;

    public OptionData(Dictionary<System.Guid, string> controls)
    {
        path = "controls";
        width = Screen.currentResolution.width;
        height = Screen.currentResolution.height;
        refreshRate = Screen.currentResolution.refreshRate;
        isFullscreen = Screen.fullScreen;
        qualityLevel = QualitySettings.GetQualityLevel();

        currentLanguage = (int)GlobalState.state.language;
        if (controls == null)
        {
            OptionData d = (OptionData)SaveData.Load_Data("controls");
            if (d.controlKeyArray.Length > 0)
            {
                controlKeyArray = d.controlKeyArray;
                valueArray = d.valueArray;
            }
            else
            {
                controlKeyArray = new byte[0][];
                valueArray = new string[0];
            }
        }
        else
        {
            controlKeyArray = new byte[controls.Keys.Count][];
            valueArray = new string[controls.Values.Count];
            int i = 0;
            foreach (var key in controls.Keys)
            {
                controlKeyArray[i] = key.ToByteArray();
                valueArray[i] = controls[key];
                i++;
            }
        }
    }

    public override string ToString()
    {
        return "Width: " + width + ", height: " + height + ", refresh rate: " + refreshRate + ", fullscreen: " + isFullscreen + ", quality level: " + qualityLevel + ", language: " + currentLanguage + ",  control key length: " + controlKeyArray.Length;
    }
}
