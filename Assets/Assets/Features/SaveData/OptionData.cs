using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OptionData : Data
{
    public byte[][] controlKeyArray;
    public string[] valueArray;
    public float mouseSensitivity;

    public OptionData(Dictionary<System.Guid, string> controls)
    {

        path = "controls";

        int i = 0;
        foreach (var key in controls.Keys)
        {
            controlKeyArray[i] = key.ToByteArray();
            valueArray[i] = controls[key];
            i++;
        }

    }

}
