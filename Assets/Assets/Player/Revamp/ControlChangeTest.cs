using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ControlChangeTest : MonoBehaviour
{
    private PlayerInput _playerInput;
    InputAction myAction;

    private void Awake()
    {
        _playerInput = new PlayerInput();

        myAction = new InputAction(binding: "/*/<button>");
       // myAction.performed += (action, control) => Debug.Log($"Button {control.name} pressed!");
        myAction.performed += ListenForInput;
        myAction.Enable();
    }

    private void OnEnable() { _playerInput.PlayerControls.Enable(); }
    private void OnDisable() { _playerInput.PlayerControls.Disable(); }

    public bool print;
    public bool listenForInput;

    private void ListenForInput(InputAction.CallbackContext ctx)
    {
        if (listenForInput)
        {
            Debug.Log($"Button {ctx.control.name} pressed!");
            Debug.Log($"Button {ctx.control.path} pressed!");
        }
    }

    private void OnValidate()
    {
        if (print)
        {
            foreach (var binding in _playerInput.PlayerControls.AttackHeavy.bindings)
            {
                print(binding.path);
            }
            print = false;
        }
    }
}
