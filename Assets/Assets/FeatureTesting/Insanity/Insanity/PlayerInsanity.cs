using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInsanity : MonoBehaviour
{
    [SerializeField]
    private InsanityBar InsanityBar;

    [SerializeField]
    private float maxInsanity;

    private float currentInsanity;

    private void Start()
    {
        InsanityBar.SetInsanity(currentInsanity);
    }

    private void Update()
    {
        
    }

    public float GetInsanity()
    {
        return currentInsanity;
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
            currentInsanity = maxInsanity;
        }
        else if (amount < 0)
        {
            currentInsanity = 0;
        }
        else
        {
            currentInsanity = amount;
        }

        ActivateBuffs();

        InsanityBar.SetInsanity(currentInsanity);
    }

    // Increments insanity based on parameters
    public void IncrementInsanity(float amount)
    {
        if (amount + currentInsanity > maxInsanity)
        {
            currentInsanity = maxInsanity;
        }
        else if (amount + currentInsanity < 0)
        {
            currentInsanity = 0;
        }
        else
        {
            currentInsanity += amount;
        }

        ActivateBuffs();
        InsanityBar.SetInsanity(currentInsanity);
    }

    public void ActivateBuffs()
    {
        // print("static: " + currentInsanity);
        

        // Static based buffs
        switch (currentInsanity)
        {
            case 10:
                print("meme");
                break;
        }

        // Percentage based debuffs
        float currentInsanityPercentage = currentInsanity / maxInsanity * 100;

        print("dynamic: " + currentInsanityPercentage);

        switch (currentInsanityPercentage)
        {
            case 10:
                print("meme but in percent");
                break;
        }
    }
}
