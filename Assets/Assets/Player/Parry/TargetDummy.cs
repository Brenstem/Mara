using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class TargetDummy : Entity
{
    public override void TakeDamage(Hitbox hitbox)
    {
        GetComponent<EnemyHealth>().Damage(hitbox.damageValue);
    }

    public override void TakeDamage(float damage)
    {
        GetComponent<EnemyHealth>().Damage(damage);
    }
}
