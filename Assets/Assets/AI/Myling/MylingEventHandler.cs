using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MylingEventHandler : MonoBehaviour
{
    [SerializeField] private MylingAI _parentAI;

    public void EndAnim()
    {
        _parentAI._animationOver = true;
    }

    public void DestroyThis()
    {
        _parentAI.stateMachine.ChangeState(new BaseIdleState());
        Destroy(this._parentAI.gameObject);
    }

    public void FootStep()
    {
        GlobalState.state.AudioManager.MylingFootstepAudio(this.transform.position);
    }
}
