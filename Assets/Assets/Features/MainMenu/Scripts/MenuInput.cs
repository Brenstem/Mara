using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuInput : MonoBehaviour
{
    [SerializeField] private Text _label;
    [SerializeField] private bool _gamepadControls;

    private enum InputType
    {
        AttackLight,
        AttackHeavy,
        Dash,
        Jump,
        Lockon,
        Parry,
    }

    [SerializeField] private InputType inputType;

    private bool _listenForInput;
    private InputAction _inputAction;

    private void Awake()
    {
        _inputAction = new InputAction(binding: "/*/<button>");
        _inputAction.performed += AnyKeyEvent;

        if (_label == null)
        {
            Debug.LogWarning("Label component missing! Searching in children...", this);
            _label = GetComponentInChildren<Text>();
            if (_label == null)
            {
                Debug.LogError("Label component not found!", this);
            }
            else
            {
                Debug.LogWarning("Label component was found. Please add a reference in the future.", this);
            }
        }

        int c = _gamepadControls ? 1 : 0;
        switch (inputType)
        {
            case InputType.AttackLight:
                _label.text = MenuInputResource.PlayerInput.PlayerControls.AttackLight.controls[c].displayName;
                break;
            case InputType.AttackHeavy:
                _label.text = MenuInputResource.PlayerInput.PlayerControls.AttackHeavy.controls[c].displayName;
                break;
            case InputType.Dash:
                _label.text = MenuInputResource.PlayerInput.PlayerControls.Dash.controls[c].displayName;
                break;
            case InputType.Jump:
                _label.text = MenuInputResource.PlayerInput.PlayerControls.Jump.controls[c].displayName;
                break;
            case InputType.Lockon:
                _label.text = MenuInputResource.PlayerInput.PlayerControls.Lockon.controls[c].displayName;
                break;
            case InputType.Parry:
                _label.text = MenuInputResource.PlayerInput.PlayerControls.Parry.controls[c].displayName;
                break;
            default:
                break;
        }
    }

    public void ListenForInput()
    {
        _listenForInput = true;
        _label.text = "Press any button...";
        _inputAction.Enable();
    }

    private void AnyKeyEvent(InputAction.CallbackContext ctx)
    {
        if (_listenForInput)
        {
            if (ctx.control.name != "anyKey")
            {
                Debug.Log($"Button {ctx.control.path} pressed!");

                int index = 1;
                if (ctx.control.device.deviceId == 1 || ctx.control.device.deviceId == 2)
                    index = 0;

                if (index == (_gamepadControls ? 1 : 0))
                {
                    switch (inputType)
                    {
                        case InputType.AttackLight:
                            MenuInputResource.PlayerInput.PlayerControls.AttackLight.ApplyBindingOverride(index, new InputBinding { overridePath = ctx.control.path });
                            break;
                        case InputType.AttackHeavy:
                            MenuInputResource.PlayerInput.PlayerControls.AttackHeavy.ApplyBindingOverride(index, new InputBinding { overridePath = ctx.control.path });
                            break;
                        case InputType.Dash:
                            MenuInputResource.PlayerInput.PlayerControls.Dash.ApplyBindingOverride(index, new InputBinding { overridePath = ctx.control.path });
                            break;
                        case InputType.Jump:
                            MenuInputResource.PlayerInput.PlayerControls.Jump.ApplyBindingOverride(index, new InputBinding { overridePath = ctx.control.path });
                            break;
                        case InputType.Lockon:
                            MenuInputResource.PlayerInput.PlayerControls.Lockon.ApplyBindingOverride(index, new InputBinding { overridePath = ctx.control.path });
                            break;
                        case InputType.Parry:
                            MenuInputResource.PlayerInput.PlayerControls.Parry.ApplyBindingOverride(index, new InputBinding { overridePath = ctx.control.path });
                            break;
                        default:
                            break;
                    }

                    _listenForInput = false;
                    string txt = string.Empty;
                    switch (ctx.control.displayName)
                    {
                        case "Press":
                            txt = "Left Button";
                            break;
                        default:
                            txt = ctx.control.displayName;
                            break;
                    }
                    _label.text = txt;
                }

                _inputAction.Disable();
            }
        }
    }
}
