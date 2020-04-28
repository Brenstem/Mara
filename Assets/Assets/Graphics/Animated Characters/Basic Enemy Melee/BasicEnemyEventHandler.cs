using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemyEventHandler : MonoBehaviour
{
    public void AttackSoundEvent()
    {
        print("meme");
        GlobalState.state.AudioManager.BasicEnemyAttack(this.transform.position);
    }
}
