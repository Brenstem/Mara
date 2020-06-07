using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OptionData : Data
{
    public byte[][] controlKeyArray;
    public string[] valueArray;
    public float horizontalSensitivity;
    public float verticalSensitivity;
    public float gamepadMultiplier;
    public int qualityLevel;
    public bool isFullscreen;
    public int refreshRate;
    public int width;
    public int height;
    public int currentLanguage;

    public OptionData(Dictionary<System.Guid, string> controls, float horizontalSensitivity = 0, float verticalSensitivity = 0, float gamepadMultiplier = 0)
    {
        path = "controls";
        width = Screen.currentResolution.width;
        height = Screen.currentResolution.height;
        refreshRate = Screen.currentResolution.refreshRate;
        isFullscreen = Screen.fullScreen;
        qualityLevel = QualitySettings.GetQualityLevel();

        if (horizontalSensitivity > 0)
            this.horizontalSensitivity = horizontalSensitivity;
        if (verticalSensitivity > 0)
            this.verticalSensitivity = verticalSensitivity;
        if (gamepadMultiplier > 0)
            this.gamepadMultiplier = gamepadMultiplier;

        currentLanguage = (int)GlobalState.state.language;
        if (controls == null)
        {
            OptionData d = (OptionData)SaveData.Load("controls");
            bool load = true;
            if (d != null && d.controlKeyArray != null)
            {
                if (d.controlKeyArray.Length > 0)
                {
                    controlKeyArray = d.controlKeyArray;
                    valueArray = d.valueArray;
                    load = false;
                }
            }
            if (load)
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
