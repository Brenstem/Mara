using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxModifier
{
    private bool _modified;

    private float _movementSpeedMultiplier = 1.0f;
    public float MovementSpeedMultiplier
    {
        get { return _movementSpeedMultiplier; }
        set
        {
            _modified = true;
            _movementSpeedMultiplier = value;
        }
    }

    private float _hitstunMultiplier = 1.0f; // knockback multiplier???
    public float HitstunMultiplier
    {
        get { return _hitstunMultiplier; }
        set
        {
            _modified = true;
            _hitstunMultiplier = value;
        }
    }

    private float _damageMultiplier = 1.0f;
    public float DamageMultiplier
    {
        get { return _damageMultiplier; }
        set
        {
            _modified = true;
            _damageMultiplier = value;
        }
    }

    private float _animSpeedMultiplier = 1.0f;
    public float AnimSpeedMultiplier
    {
        get { return _animSpeedMultiplier; }
        set
        {
            _modified = true;
            _animSpeedMultiplier = value;
        }
    }

    public bool IsModified
    {
        get { return _modified; }
    }

    public void Reset()
    {
        _modified = false;
        _movementSpeedMultiplier = 1.0f;
        _hitstunMultiplier = 1.0f;
        _damageMultiplier = 1.0f;
        _animSpeedMultiplier = 1.0f;
    }

    public HitboxModifier() { }

    public HitboxModifier(float movementSpeedMultiplier, float hitstunMultiplier, float damageMultiplier, float animSpeedMultiplier)
    {
        MovementSpeedMultiplier = movementSpeedMultiplier;
        HitstunMultiplier = hitstunMultiplier;
        DamageMultiplier = damageMultiplier;
        AnimSpeedMultiplier = animSpeedMultiplier;
    }
}

public abstract class Entity : MonoBehaviour
{
    public EntityHealth health;
    public bool invulerable;
    public HitboxModifier modifier;

    public abstract void TtakeDamage(HitboxValues hitbox, Entity attacker = null);

    protected virtual void Awake()
    {
        modifier = new HitboxModifier();
    }
}
