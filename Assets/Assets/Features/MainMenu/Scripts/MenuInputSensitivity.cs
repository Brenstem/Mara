using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuInputSensitivity : MonoBehaviour
{
    private enum Type
    {
        Horizontal,
        Vertical,
        GamepadMultiplier,
    }
    [SerializeField] private Type type;

    [SerializeField] private Slider _slider;
    [SerializeField] private InputField _inputField;

    private void Start()
    {
        switch (type)
        {
            case Type.Horizontal:
                _slider.value = SceneData.horizontalSensitivity;
                break;
            case Type.Vertical:
                _slider.value = SceneData.verticalSensitivity;
                break;
            case Type.GamepadMultiplier:
                _slider.value = SceneData.gamepadMultiplier;
                break;
            default:
                break;
        }
        _inputField.text = _slider.value.ToString();
    }

    public void OnValueChange(float value)
    {
        _inputField.text = value.ToString();
        switch (type)
        {
            case Type.Horizontal:
                MenuInputResource.horizontalSensitivity = value;
                break;
            case Type.Vertical:
                MenuInputResource.verticalSensitivity = value;
                break;
            case Type.GamepadMultiplier:
                MenuInputResource.gamepadMultiplier = value;
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
            case Type.Horizontal:
                MenuInputResource.horizontalSensitivity = val;
                break;
            case Type.Vertical:
                MenuInputResource.verticalSensitivity = val;
                break;
            case Type.GamepadMultiplier:
                MenuInputResource.gamepadMultiplier = val;
                break;
            default:
                break;
        }
    }
}
