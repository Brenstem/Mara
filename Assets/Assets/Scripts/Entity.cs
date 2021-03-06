﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Modifier
{
    public delegate void Modified();
    public event Modified onModified;

    private float _multiplier;
    private bool _isModified;

    public float Multiplier 
    { 
        get { return _multiplier; } 
        set 
        { 
            _multiplier = value;

            if (_multiplier == 1)
            {
                _isModified = false;
            }
            else 
            {
                _isModified = true;

                if (onModified != null)
                    onModified();
            }
        }
    }
    public bool IsModified { get { return _isModified; } }

    private const float TOLERANCE = 0.0000000001f;

    public Modifier(float multiplier)
    {
        _multiplier = multiplier;

        if (NearlyEquals(multiplier, 1.0f, TOLERANCE))
        {
            _isModified = false;
        }
        else
        {
            _isModified = true;

            if (onModified != null)
                onModified();
        }
    }

    public void Reset()
    {
        Multiplier = 1.0f;
        _isModified = false;
    }

    public static float operator *(float value, Modifier modifier)
    {
        value *= modifier.Multiplier;
        return value;
    }

    public static Modifier operator *(Modifier modifier, float value)
    {
        modifier.Multiplier = value;
        return modifier;
    }

    public static bool NearlyEquals(float a, float b, float tolerance)
    {
        if (a > b + tolerance || a < b - tolerance)
        {
            return false;
        }
        else { return true; }
    }

    public static bool NearlyEquals(float a, float b)
    {
        float tolerance = 0.0000000001f;

        if (a > b + tolerance || a < b - tolerance)
        {
            return false;
        }
        else { return true; }
    }
}

public class EntityModifier
{
    private bool _modified;

    private Modifier _movementSpeedMultiplier;
    public Modifier MovementSpeedMultiplier
    {
        get { return _movementSpeedMultiplier; }
        set { _movementSpeedMultiplier.Multiplier = value.Multiplier; }
    }

    private Modifier _hitstunMultiplier;
    public Modifier HitstunMultiplier
    {
        get { return _hitstunMultiplier; }
        set { _hitstunMultiplier.Multiplier = value.Multiplier; }
    }

    private Modifier _damageMultiplier;
    public Modifier DamageMultiplier
    {
        get { return _damageMultiplier; }
        set { _damageMultiplier.Multiplier = value.Multiplier; }
    }

    private Modifier _attackSpeedMultiplier;
    public Modifier AttackSpeedMultiplier
    {
        get { return _attackSpeedMultiplier; }
        set { _attackSpeedMultiplier.Multiplier = value.Multiplier; }
    }

    public bool IsModified
    {
        get { return _modified; }
    }

    public void Reset()
    {
        _modified = false;
        _movementSpeedMultiplier.Multiplier = 1.0f;
        _hitstunMultiplier.Multiplier = 1.0f;
        _damageMultiplier.Multiplier = 1.0f;
        _attackSpeedMultiplier.Multiplier = 1.0f;
    }

    public EntityModifier() 
    {
        _movementSpeedMultiplier = new Modifier(1.0f);
        _hitstunMultiplier = new Modifier(1.0f);
        _damageMultiplier = new Modifier(1.0f);
        _attackSpeedMultiplier = new Modifier(1.0f);
    }

    public EntityModifier(float movementSpeedMultiplier, float hitstunMultiplier, float damageMultiplier, float attackSpeedMultiplier)
    {
        _movementSpeedMultiplier = new Modifier(movementSpeedMultiplier);
        _hitstunMultiplier = new Modifier(hitstunMultiplier);
        _damageMultiplier = new Modifier(damageMultiplier);
        _attackSpeedMultiplier = new Modifier(attackSpeedMultiplier);
    }
}

public abstract class Entity : MonoBehaviour
{
    public bool invulerable;
    public EntityHealth health;
    public EntityModifier modifier;

    public abstract void TakeDamage(HitboxValues hitbox, Entity attacker = null);
    public virtual void TakeDamage(float damageAmount)
    {
        health.Damage(damageAmount);
    }
    public abstract void Parried();
    public abstract void KillThis();

    protected virtual void Awake()
    {
        modifier = new EntityModifier();
        health = GetComponent<EntityHealth>();
    }
}
