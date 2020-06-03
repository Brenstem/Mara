using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGameTest : MonoBehaviour
{
    private const int START_CAMERA_BLEND_INDEX = 2;

    private PlayerRevamp _playerRevamp;
    private CinemachineStateDrivenCamera _cinemachineStateDrivenCamera;
    [SerializeField, Range(0.0f, 1.0f)] private float _activeControlsFaction;
    [SerializeField] private GameObject _canvas;

    private void Awake()
    {
        _playerRevamp = GlobalState.state.Player;
        _cinemachineStateDrivenCamera = GlobalState.state.StateDrivenCamera;
    }

    //kör denna när spelet ska startas
    public void StartGame()
    {
        Debug.Log("Game Started By " + this);

        _playerRevamp.cameraAnimator.SetTrigger("GameStarted");

        GlobalState.state.LockCursor = true;
        StartCoroutine(StartGameTransition(_cinemachineStateDrivenCamera.m_CustomBlends.m_CustomBlends[START_CAMERA_BLEND_INDEX].m_Blend.m_Time * _activeControlsFaction));
    }

    private IEnumerator StartGameTransition(float transitionTime)
    {
        yield return new WaitForSecondsRealtime(transitionTime);
        if (_canvas == null)
            _canvas = GetComponentInParent<Canvas>().gameObject;
        _canvas.SetActive(false);
        _playerRevamp.EnabledControls = true;
    }
}
