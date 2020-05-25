using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuInputSensitivity : MonoBehaviour
{
    private enum Type
    {
        Camera,
        LockonCamera,
    }
    [SerializeField] private Type type;

    [SerializeField] private Slider _slider;
    [SerializeField] private InputField _inputField;

    private void Awake()
    {
        _inputField.text = _slider.value.ToString();
    }

    public void OnValueChange(float value)
    {
        _inputField.text = value.ToString();
        switch (type)
        {
            case Type.Camera:
                MenuInputResource.cameraSensitivity = value;
                break;
            case Type.LockonCamera:
                MenuInputResource.lockonCameraSensitivity = value;
                break;
            default:
                break;
        }
    }

    public void OnValueChange(string value)
    {
        float val = _slider.value;
        float.TryParse(value, out val);
        _slider.value = val;
        switch (type)
        {
            case Type.Camera:
                MenuInputResource.cameraSensitivity = val;
                break;
            case Type.LockonCamera:
                MenuInputResource.lockonCameraSensitivity = val;
                break;
            default:
                break;
        }
    }
}
