using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInsanity : MonoBehaviour
{
    [SerializeField]
    public float maxInsanity;

    private float currentInsanity;

    void Awake()
    {
        // currentInsanity = maxInsanity;
    }

    private void Update()
    {
        print(currentInsanity);
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
    }
}
