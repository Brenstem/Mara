using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepEventHandler : MonoBehaviour
{
    [SerializeField] Rigidbody _rb;

    public void PlayFootStep(float targetWalkSpeed)
    {
        GlobalState.state.AudioManager.PlayerFootStepsAudio(this.transform, "gravel", _rb);
    }
}
