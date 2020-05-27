using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemyAnimEventHandler : MonoBehaviour
{
    [SerializeField] private RangedEnemyAI _parentAI;

    public void FireEvent()
    {
        _parentAI.Fire();
    }

    public void TurnInterruptEvent()
    {
        _parentAI._canTurn = false;
    }

    public void EndAnim()
    {
        _parentAI._animationOver = true;
    }

    public void DestroyThis()
    {
        _parentAI.stateMachine.ChangeState(new BaseIdleState()); // Do this to call exitstate on death state before destroying object
        Destroy(this._parentAI.gameObject);
    }
}
