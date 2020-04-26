﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth;

    private TrackingHealthBar _healthBar;
    private float _currentHealth;

    public Animator _anim;


    private void Awake()
    {
        _healthBar = GetComponentInChildren<TrackingHealthBar>();
        _anim = GetComponentInChildren<Animator>();
        _healthBar.SetMaxValue(maxHealth);
        _currentHealth = maxHealth;
        _healthBar.SetValue(_currentHealth);
    }

    private void Start() 
    {
        if (!_healthBar)
        {
            throw new System.Exception("Please add a trackingHealthbar prefab as a child to this component!");
        }
    }

    public float GetHealth() {
        return _currentHealth;
    }
    
    public float GetMaxHealth() {
        return maxHealth;
    }

    public void SetMaxHealth(float amount) {
        maxHealth = amount;
        _healthBar.SetMaxValue(maxHealth);
    }

    public void IncrementMaxHealth(float amount) {
        maxHealth += amount;
        _healthBar.SetMaxValue(maxHealth);
    }

    // Sets Health based on parameters
    public void SetHealth(float amount)
    {
        // Insanity cannot be above max or below 0
        if (amount > maxHealth)
        {
            _currentHealth = maxHealth;
        }
        else if (amount < 0) {
            _currentHealth = 0;
        }
        else {
            _currentHealth = amount;
        }

        _healthBar.SetValue(_currentHealth);
    }

    // Increments Health based on parameters
    public void Damage(float amount)
    {
        // Insanity cannot be above max or below 0
        if (_currentHealth - amount > maxHealth)
        {
            _currentHealth = maxHealth;
        }
        else if (_currentHealth - amount <= 0)
        {
            _currentHealth = 0;
        }
        else
        {
            _currentHealth -= amount;
        }
        _healthBar.SetValue(_currentHealth);
    }
}
