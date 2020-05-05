using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : EntityHealth
{
    private new void Start()
    {
        base.Start();
    }
    
    public override void Damage(HitboxValues hitbox)
    {
        CurrentHealth -= hitbox.damageValue;
    }
}
