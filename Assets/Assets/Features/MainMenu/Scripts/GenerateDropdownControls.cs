using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
public class GenerateDropdownControls : MonoBehaviour
{
    private bool _listenForKey;
    private PlayerInput _playerInput;

    private void Awake()
    {
        _playerInput = new PlayerInput();
    }

    private void OnEnable() { _playerInput.PlayerControls.Enable(); }
    private void OnDisable() { _playerInput.PlayerControls.Disable(); }


    public void EnableKeyListen()
    {
        _listenForKey = true;
    }

    private void Update()
    {
        if (_listenForKey)
        {
            //_playerInput.PlayerControls.any
            print(_playerInput.PlayerControls.AttackHeavy.bindings[0].path);
        }
    }

}
