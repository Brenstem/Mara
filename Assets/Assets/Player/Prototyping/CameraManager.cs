using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour { // https://forum.unity.com/threads/free-look-with-new-input-system.676873/
    private PlayerInput _defaultcontrols;
    private Vector2 _lookDelta;

    private void Awake() => _defaultcontrols = new PlayerInput();

    private void OnEnable() => _defaultcontrols.Enable();
    private void OnDisable() => _defaultcontrols.Disable();

    private void Update() {
        CinemachineCore.GetInputAxis = GetAxisCustom;
    }

    public float GetAxisCustom(string axisName) {
        _lookDelta = _defaultcontrols.PlayerControls.Look.ReadValue<Vector2>();
        //_lookDelta.Normalize();

        if (axisName == "Mouse X") {
            return _lookDelta.x;
        }
        else if (axisName == "Mouse Y") {
            return _lookDelta.y;
        }
        return 0;
    }
}