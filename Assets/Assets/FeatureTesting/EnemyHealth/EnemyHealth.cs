using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : EntityHealth
{
    private void Awake()
    {
        base.Awake();
    }
    
    public override void TakeDamage(HitboxValues hitbox)
    {
        CurrentHealth -= hitbox.damageValue;
    }
}
