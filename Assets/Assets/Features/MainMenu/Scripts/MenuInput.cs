using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MenuInput : MonoBehaviour
{
    [SerializeField] private Text _label;

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


                int index = 1;
                if (ctx.control.device.deviceId == 1 || ctx.control.device.deviceId == 2)
                    index = 0;

                switch (inputType)
                {
                    case InputType.AttackLight:
                        MenuInputResource.playerInput.PlayerControls.AttackLight.ApplyBindingOverride(index, new InputBinding { overridePath = ctx.control.path });
                        break;
                    case InputType.AttackHeavy:
                        MenuInputResource.playerInput.PlayerControls.AttackHeavy.ApplyBindingOverride(index, new InputBinding { overridePath = ctx.control.path });
                        break;
                    case InputType.Dash:
                        MenuInputResource.playerInput.PlayerControls.Dash.ApplyBindingOverride(index, new InputBinding { overridePath = ctx.control.path });
                        break;
                    case InputType.Jump:
                        MenuInputResource.playerInput.PlayerControls.Jump.ApplyBindingOverride(index, new InputBinding { overridePath = ctx.control.path });
                        break;
                    case InputType.Lockon:
                        MenuInputResource.playerInput.PlayerControls.Lockon.ApplyBindingOverride(index, new InputBinding { overridePath = ctx.control.path });
                        break;
                    case InputType.Parry:
                        MenuInputResource.playerInput.PlayerControls.Parry.ApplyBindingOverride(index, new InputBinding { overridePath = ctx.control.path });
                        break;
                    default:
                        break;
                }

                _inputAction.Disable();
            }
        }
    }
}
