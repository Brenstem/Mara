using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class EntityHealth : MonoBehaviour
{
    public Slider slider;

    private float _currentHealth;
    public float CurrentHealth
    {
        get { return _currentHealth; }
        set
        {
            if (value < 0)
            {
                _currentHealth = 0;
            }
            else if (value > MaxHealth)
            {
                _currentHealth = MaxHealth;
            }
            else
            {
                _currentHealth = value;
            }

            if (slider != null)
                slider.value = value;
        }
    }

    [SerializeField] private float _maxHealth;
    public float MaxHealth
    {
        get { return _maxHealth; }
        set
        {
            _maxHealth = value;
            if (slider != null)
                slider.maxValue = value;
        }
    }

    public abstract void TakeDamage(HitboxValues hitbox);

    public void Awake()
    {
        CurrentHealth = MaxHealth;
    }
}
