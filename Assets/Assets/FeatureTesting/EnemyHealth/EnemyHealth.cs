using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    //[SerializeField]
    //private HealthBar InsanityBar;

    [SerializeField]
    private float maxHealth;

    private float _currentHealth;

    private void Start()
    {
        //HealthBar.SetHealth(currentHealth);
    }

    public float GetHealth()
    {
        return _currentHealth;
    }

    public void SetMaxHealth(float amount)
    {
        maxHealth = amount;
        //HealthBar.SetMaxHealth(maxHealth);
    }

    public void IncrementMaxHealth(float amount)
    {
        maxHealth += amount;
        //HealthBar.SetMaxHealth(maxHealth);

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

        //HealthBar.SetHealth(currentHealth);
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

        //HealthBar.SetHealth(currentHealth);
    }
}
