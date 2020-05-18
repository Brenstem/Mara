using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemyEventHandler : MonoBehaviour
{
    [SerializeField] private BasicMeleeAI parentAI;


    public void AttackSoundEvent()
    {
        GlobalState.state.AudioManager.BasicEnemyAttack(this.transform.position);
    }

    public void DestroyThis()
    {
        parentAI.stateMachine.ChangeState(new BaseIdleState()); // Do this to call exitstate on death state before destroying object
        Destroy(this.parentAI.gameObject);
    }

    public void EndAnim()
    {
        parentAI._animationOver = true;
    }
}
