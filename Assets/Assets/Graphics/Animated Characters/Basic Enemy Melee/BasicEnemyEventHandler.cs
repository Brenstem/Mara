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
        Destroy(this.parentAI.gameObject);
    }

    public void EndAnim()
    {
        parentAI._animationOver = true;
    }
}
