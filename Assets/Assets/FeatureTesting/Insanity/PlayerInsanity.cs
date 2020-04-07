using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInsanity : MonoBehaviour
{
    [SerializeField]
    private InsanityBar InsanityBar;

    [SerializeField]
    private float maxInsanity;

    private float _currentInsanity;

    private void Start()
    {
        InsanityBar.SetInsanity(_currentInsanity);
    }

    private void Update()
    {
        
    }

    public float GetInsanity()
    {
        return _currentInsanity;
    }

    public void SetMaxInsanity(float amount)
    {
        maxInsanity = amount;
        InsanityBar.SetMaxInsanity(maxInsanity);
    }

    public void IncrementMaxInsanity(float amount)
    {
        maxInsanity += amount;
        InsanityBar.SetMaxInsanity(maxInsanity);

    }

    // Sets insanity based on parameters
    public void SetInsanity(float amount)
    {
        if (amount > maxInsanity)
        {
            _currentInsanity = maxInsanity;
        }
        else if (amount < 0)
        {
            _currentInsanity = 0;
        }
        else
        {
            _currentInsanity = amount;
        }

        ActivateBuffs();

        InsanityBar.SetInsanity(_currentInsanity);
    }

    // Increments insanity based on parameters
    public void IncrementInsanity(float amount)
    {
        if (amount + _currentInsanity > maxInsanity)
        {
            _currentInsanity = maxInsanity;
        }
        else if (amount + _currentInsanity < 0)
        {
            _currentInsanity = 0;
        }
        else
        {
            _currentInsanity += amount;
        }

        ActivateBuffs();
        InsanityBar.SetInsanity(_currentInsanity);
    }

    public void ActivateBuffs()
    {
        // print("static: " + currentInsanity);
        

        // Static based buffs
        switch (_currentInsanity)
        {
            case 10:
                print("meme");
                break;
        }

        // Percentage based debuffs
        float currentInsanityPercentage = _currentInsanity / maxInsanity * 100;

        print("dynamic: " + currentInsanityPercentage);

        switch (currentInsanityPercentage)
        {
            case 10:
                print("meme but in percent");
                break;
        }
    }
}
