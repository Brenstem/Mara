﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class EntityHealth : MonoBehaviour
{
    [SerializeField] private float _maxHealth;

    [SerializeField] protected HealthBar _healthBar;

    public virtual HealthBar HealthBar {
        get { return _healthBar; }
        set { _healthBar = value; } 
    }

    private Entity _entity;

    protected float _currentHealth;
    public virtual float CurrentHealth
    {
        get { return _currentHealth; }
        set
        {
            if (value <= 0)
            {
                _currentHealth = 0;
                KillThis();
            }
            else if (value > MaxHealth)
            {
                _currentHealth = MaxHealth;
            }
            else
            {
                _currentHealth = value;
            }

            if (HealthBar != null)
            {
                HealthBar.SetValue(value);
            }   
        }
    }

    public float MaxHealth
    {
        get { return _maxHealth; }
        set
        {
            _maxHealth = value;
            if (HealthBar != null)
            {
                HealthBar.SetMaxValue(value);
            }
        }
    }

    public abstract void Damage(HitboxValues hitbox);

    public virtual void Damage(float damage)
    {
        CurrentHealth -= damage;
    }

    public void KillThis()
    {
        _entity.KillThis();
    }

    protected virtual void Awake()
    {
        _entity = GetComponent<Entity>();
    }

    protected virtual void Start()
    {
        if (HealthBar != null)
        {
            HealthBar.SetMaxValue(MaxHealth);
        }

        CurrentHealth = MaxHealth;
    }
}
