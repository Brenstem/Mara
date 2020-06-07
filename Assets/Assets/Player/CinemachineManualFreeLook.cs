using Cinemachine;
using UnityEngine;
 
public class CinemachineManualFreeLook : MonoBehaviour // Modified version of: https://forum.unity.com/threads/free-look-camera-and-mouse-responsiveness.642886/
{
    private CinemachineFreeLook freeLook;
    private PlayerInput _defaultcontrols;
    private Vector2 _lookDelta;

    [Tooltip("This depends on your Free Look rigs setup, use to correct Y sensitivity,"
        + " about 1.5 - 2 results in good Y-X square responsiveness")]
    public float yCorrection = 2f;

    private float xAxisValue;
    private float yAxisValue;

    private void Awake()
    {
        freeLook = GetComponent<CinemachineFreeLook>();
        _defaultcontrols = new PlayerInput();
    }

    private void OnEnable() => _defaultcontrols.Enable();
    private void OnDisable() => _defaultcontrols.Disable();

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * SceneData.horizontalSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * SceneData.verticalSensitivity * Time.deltaTime;

        if (_defaultcontrols.PlayerControls.Look.activeControl != null)
        {
            if (_defaultcontrols.PlayerControls.Look.activeControl.device.name != "Mouse")
            {
                _lookDelta = _defaultcontrols.PlayerControls.Look.ReadValue<Vector2>();
                mouseX = _lookDelta.x * SceneData.horizontalSensitivity * SceneData.gamepadMultiplier * Time.deltaTime;
                mouseY = _lookDelta.y * SceneData.verticalSensitivity * SceneData.gamepadMultiplier * Time.deltaTime;
            }
        }
        
        // Correction for Y
        mouseY /= 360f;
        mouseY *= yCorrection;

        xAxisValue += mouseX;
        yAxisValue = Mathf.Clamp01(yAxisValue - mouseY);

        freeLook.m_XAxis.Value = xAxisValue;
        freeLook.m_YAxis.Value = yAxisValue;
    }
}