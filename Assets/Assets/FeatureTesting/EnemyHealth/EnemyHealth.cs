using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField]
    private GameObject healthBarPrefab;

    [SerializeField]
    private float maxHealth;

    private TrackingHealthBar _healthBar;
    private float _currentHealth;

    private void Awake()
    {
        Instantiate(healthBarPrefab, this.transform);
    }

    private void Start()
    {
        _healthBar = healthBarPrefab.GetComponentInChildren<TrackingHealthBar>();
        _currentHealth = maxHealth;
        _healthBar.SetMaxValue(maxHealth);
        _healthBar.SetValue(_currentHealth);
    }

    public float GetHealth()
    {
        return _currentHealth;
    }

    public void SetMaxHealth(float amount)
    {
        maxHealth = amount;
        _healthBar.SetMaxValue(maxHealth);
    }

    public void IncrementMaxHealth(float amount)
    {
        maxHealth += amount;
        _healthBar.SetMaxValue(maxHealth);

    }

    // Sets Health based on parameters
    public void SetHealth(float amount)
    {
        if (amount > maxHealth)
        {
            _currentHealth = maxHealth;
        }
        else if (amount < 0)
        {
            _currentHealth = 0;
        }
        else
        {
            _currentHealth = amount;
        }

        _healthBar.SetValue(_currentHealth);
    }

    // Increments Health based on parameters
    public void IncrementHealth(float amount)
    {
        if (amount + _currentHealth > maxHealth)
        {
            _currentHealth = maxHealth;
        }
        else if (amount + _currentHealth < 0)
        {
            _currentHealth = 0;
        }
        else
        {
            _currentHealth += amount;
        }

        _healthBar.SetValue(_currentHealth);
    }
}
