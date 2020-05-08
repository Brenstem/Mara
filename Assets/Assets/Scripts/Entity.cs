using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public struct Modifier
{
    // TODO add tolerance to multiplier isModifier check

    public float multiplier;
    public bool isModified;

    public Modifier(float multiplier)
    {
        this.multiplier = multiplier;

        if (this.multiplier != 1.0f)
        {
            isModified = false;
        }
        else
        {
            isModified = true;
        }
    }

    public void Reset()
    {
        multiplier = 1.0f;
        isModified = false;
    }

    public static float operator *(float value, Modifier modifier)
    {
        value *= modifier.multiplier;
        return value;
    }

    public static Modifier operator *(Modifier modifier, float value)
    {
        modifier.multiplier = value;
        return modifier;
    }
}

public class EntityModifier
{
    private bool _modified;

    private Modifier _movementSpeedMultiplier = new Modifier(1.0f);
    public Modifier MovementSpeedMultiplier
    {
        get { return _movementSpeedMultiplier; }
        set { _movementSpeedMultiplier.multiplier = value.multiplier; }
    }

    private Modifier _hitstunMultiplier = new Modifier(1.0f);
    public Modifier HitstunMultiplier
    {
        get { return _hitstunMultiplier; }
        set { _hitstunMultiplier.multiplier = value.multiplier; }
    }

    private Modifier _damageMultiplier = new Modifier(1.0f);
    public Modifier DamageMultiplier
    {
        get { return _damageMultiplier; }
        set { _damageMultiplier.multiplier = value.multiplier; }
    }

    private Modifier _attackSpeedMultiplier = new Modifier(1.0f);
    public Modifier AttackSpeedMultiplier
    {
        get { return _attackSpeedMultiplier; }
        set { _attackSpeedMultiplier.multiplier = value.multiplier; }
    }

    public bool IsModified
    {
        get { return _modified; }
    }

    public void Reset()
    {
        _modified = false;
        _movementSpeedMultiplier.multiplier = 1.0f;
        _hitstunMultiplier.multiplier = 1.0f;
        _damageMultiplier.multiplier = 1.0f;
        _attackSpeedMultiplier.multiplier = 1.0f;
    }

    public EntityModifier() { }

    public EntityModifier(float movementSpeedMultiplier, float hitstunMultiplier, float damageMultiplier, float attackSpeedMultiplier)
    {
        _movementSpeedMultiplier.multiplier = movementSpeedMultiplier;
        _hitstunMultiplier.multiplier = hitstunMultiplier;
        _damageMultiplier.multiplier = damageMultiplier;
        _attackSpeedMultiplier.multiplier = attackSpeedMultiplier;
    }
}

public abstract class Entity : MonoBehaviour
{
    public EntityHealth health;
    public bool invulerable;
    public EntityModifier modifier;

    public abstract void TakeDamage(HitboxValues hitbox, Entity attacker = null);
    public virtual void TakeDamage(float damageAmount)
    {
        health.Damage(damageAmount);
    }

    public abstract void KillThis();

    protected virtual void Awake()
    {
        modifier = new EntityModifier();
        health = GetComponent<EntityHealth>();
    }
}
