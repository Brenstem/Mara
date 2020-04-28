using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class TargetDummy : Entity
{
    public override void TakeDamage(HitboxValues hitbox, Entity attacker)
    {
        GetComponent<EnemyHealth>().Damage(hitbox.damageValue);
    }
}