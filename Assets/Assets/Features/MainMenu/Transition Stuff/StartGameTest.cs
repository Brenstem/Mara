using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGameTest : MonoBehaviour
{
    private const int START_CAMERA_BLEND_INDEX = 2;

    private PlayerRevamp _playerRevamp;
    private CinemachineStateDrivenCamera _cinemachineStateDrivenCamera;

    private void Awake()
    {
        _playerRevamp = GlobalState.state.Player;
        _cinemachineStateDrivenCamera = GlobalState.state.Camera.GetComponentInChildren<CinemachineStateDrivenCamera>();
    }

    //kör denna när spelet ska startas
    public void StartGame()
    {
        Debug.Log("Game Started By " + this);

        _playerRevamp.cameraAnimator.SetTrigger("GameStarted");

        GlobalState.state.LockCursor = true;

        StartCoroutine(StartGameTransition(_cinemachineStateDrivenCamera.m_CustomBlends.m_CustomBlends[START_CAMERA_BLEND_INDEX].m_Blend.m_Time));
    }

    private IEnumerator StartGameTransition(float transitionTime)
    {
        yield return new WaitForSeconds(transitionTime);

        _playerRevamp.EnabledControls = true;
    }
}
