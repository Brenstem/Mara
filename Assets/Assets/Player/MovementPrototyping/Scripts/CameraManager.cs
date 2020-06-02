using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{ // https://forum.unity.com/threads/free-look-with-new-input-system.676873/
    private CinemachineCore.AxisInputDelegate _baseInputType;
    private PlayerInput _defaultcontrols;
    private Vector2 _lookDelta;

    private void Awake() => _defaultcontrols = new PlayerInput();

    private void OnEnable() => _defaultcontrols.Enable();
    private void OnDisable() => _defaultcontrols.Disable();

    private void Start()
    {
        _baseInputType = CinemachineCore.GetInputAxis;
        //_defaultcontrols.PlayerControls.Look.performed += ctx => _lookDelta = ctx.ReadValue<Vector2>();
    }

    private void Update()
    {
        _lookDelta = _defaultcontrols.PlayerControls.Look.ReadValue<Vector2>();
        if (_defaultcontrols.PlayerControls.Look.activeControl != null)
        {
            string controls = _defaultcontrols.PlayerControls.Look.activeControl.device.name;
            if (controls != "Mouse")
            { // Checks if the input device is a mouse
                CinemachineCore.GetInputAxis = GetAxisCustom;
            }
            else
            {
                CinemachineCore.GetInputAxis = _baseInputType;
            }
        }
    }

    public float GetAxisCustom(string axisName)
    {
        _lookDelta = _defaultcontrols.PlayerControls.Look.ReadValue<Vector2>();

        if (axisName == "Mouse X")
        {
            return _lookDelta.x;
        }
        else if (axisName == "Mouse Y")
        {
            return _lookDelta.y;
        }
        return 0;
    }
}