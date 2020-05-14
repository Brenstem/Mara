using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class TargetDummy : Entity
{
    public override void Parried()
    {
        Debug.LogWarning("Parried implementation missing", this);
    }

    public override void KillThis()
    {
        Destroy(this.gameObject);
    }

    public override void TakeDamage(HitboxValues hitbox, Entity attacker)
    {
        health.Damage(hitbox);
    }
}