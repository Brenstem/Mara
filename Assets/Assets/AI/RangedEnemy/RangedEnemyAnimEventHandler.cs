using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemyAnimEventHandler : MonoBehaviour
{
    [SerializeField] private RangedEnemyAI parentAI;

    public void FireEvent()
    {
        parentAI.Fire();
    }

    public void TurnInterruptEvent()
    {
        parentAI._canTurn = false;
    }

    public void EndAnim()
    {
        parentAI._animationOver = true;
    }

    public void DestroyThis()
    {
        parentAI.stateMachine.ChangeState(new BaseIdleState()); // Do this to call exitstate on death state before destroying object
        Destroy(this.parentAI.gameObject);
    }
}
