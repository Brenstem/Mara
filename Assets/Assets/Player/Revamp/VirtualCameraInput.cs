using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class VirtualCameraInput : MonoBehaviour
{
    private PlayerInput _defaultcontrols;
    private CinemachineVirtualCamera _camera;
    private Vector2 _lookDelta;

    private void Awake() => _defaultcontrols = new PlayerInput();
    private void OnEnable() => _defaultcontrols.Enable();
    private void OnDisable() => _defaultcontrols.Disable();

    private void Start()
    {
        _camera = GetComponent<CinemachineVirtualCamera>();
        //_defaultcontrols.PlayerControls.Look.performed += ctx => _lookDelta = ctx.ReadValue<Vector2>();
    }

    private void Update()
    {
        _lookDelta = _defaultcontrols.PlayerControls.Look.ReadValue<Vector2>();
        if (_lookDelta.magnitude > 1)
            _lookDelta.Normalize();
        print(_lookDelta);
        _camera.GetCinemachineComponent<CinemachineComposer>().m_TrackedObjectOffset = new Vector3(_lookDelta.x, 0, _lookDelta.y);
    }
}
