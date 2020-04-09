using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInsanity : MonoBehaviour
{
    [Header("Insanity values")]
    [SerializeField] private float maxInsanity;

    [SerializeField] private float impendingDoomTimer;
    [Space(10)]

    [Tooltip("Add the insanity bar game object here")]
    [SerializeField] private HealthBar InsanityBar;
    [Space(10)]

    [Tooltip("Add the static and dynamic values for each insanity tier here. Do not change the array size!")]
    [SerializeField] int[] staticInsanityValues;

    [SerializeField] int[] dynamicInsanityValuesnew;

    // TODO Add event support for insanity tiers
    private UnityEngine.Events.UnityEvent tutorialDebuff;
    private UnityEngine.Events.UnityEvent paranoia;
    private UnityEngine.Events.UnityEvent slow;
    private UnityEngine.Events.UnityEvent hallucination;
    private UnityEngine.Events.UnityEvent shadowClone;
    private UnityEngine.Events.UnityEvent monsters;
    private UnityEngine.Events.UnityEvent impendingDoom;

    private float _currentInsanity;

    private Timer _timer;

    private void Start()
    {
        InsanityBar.SetValue(_currentInsanity);
    }

    private void Update()
    {
        if (_timer != null)
        {
            _timer.Time += Time.deltaTime;
            if (_timer.Expired())
            {
                ImpendingDoom();
            }
        }
    }

    public float GetInsanity()
    {
        return _currentInsanity;
    }

    public float GetInsanityPercentage()
    {
        return _currentInsanity / maxInsanity * 100; 
    }

    public void SetMaxInsanity(float amount)
    {
        maxInsanity = amount;
        InsanityBar.SetMaxValue(maxInsanity);
    }

    public void IncrementMaxInsanity(float amount)
    {
        maxInsanity += amount;
        InsanityBar.SetMaxValue(maxInsanity);
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

        InsanityBar.SetValue(_currentInsanity);
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
        InsanityBar.SetValue(_currentInsanity);
    }

    public void ActivateBuffs()
    {
        // Static based buffs
        switch (_currentInsanity)
        {
            case float n when (n > staticInsanityValues[6]):
                Debug.Log("<color=red>Impending doom static</color>");
                _timer = new Timer(impendingDoomTimer);
                break;
            case float n when (n > staticInsanityValues[5]):
                Debug.Log("<color=red>Monsters static</color>");
                monsters.Invoke();
                break;
            case float n when (n > staticInsanityValues[4]):
                Debug.Log("<color=red>Shadow clone static</color>");
                shadowClone.Invoke();
                break;
            case float n when (n > staticInsanityValues[3]):
                Debug.Log("<color=red>Hallucination static</color>");
                hallucination.Invoke();
                break;
            case float n when (n > staticInsanityValues[2]):
                Debug.Log("<color=red>slow static</color>");
                slow.Invoke();

                break;
            case float n when (n > staticInsanityValues[1]):
                Debug.Log("<color=red>Paranoia static</color>");
                paranoia.Invoke();

                break;
            case float n when (n > staticInsanityValues[0]):
                Debug.Log("<color=red>Tutorial debuff static</color>");
                tutorialDebuff.Invoke();
                break;
        }

        // Percentage based debuffs
        float currentInsanityPercentage = _currentInsanity / maxInsanity * 100;

        switch (currentInsanityPercentage)
        {
            case float n when (n > dynamicInsanityValuesnew[6]):
                Debug.Log("<color=red>Impending doom</color>");
                break;
            case float n when (n > dynamicInsanityValuesnew[5]):
                Debug.Log("<color=red>Monsters</color>");
                monsters.Invoke();
                break;
            case float n when (n > dynamicInsanityValuesnew[4]):
                Debug.Log("<color=red>Shadow clone</color>");
                shadowClone.Invoke();
                break;
            case float n when (n > dynamicInsanityValuesnew[3]):
                Debug.Log("<color=red>Hallucination</color>");
                hallucination.Invoke();
                break;
            case float n when (n > dynamicInsanityValuesnew[2]):
                Debug.Log("<color=red>slow</color>");
                slow.Invoke();
                break;
            case float n when (n > dynamicInsanityValuesnew[1]):
                Debug.Log("<color=red>Paranoia</color>");
                paranoia.Invoke();
                break;
            case float n when (n > dynamicInsanityValuesnew[0]):
                Debug.Log("<color=red>Tutorial debuff</color>");
                tutorialDebuff.Invoke();
                break;
        }
    }

    public void ImpendingDoom()
    { 
        print("player dead woo");
        _timer.Reset();
    }
}
