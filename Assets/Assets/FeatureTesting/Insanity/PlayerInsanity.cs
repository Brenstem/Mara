using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInsanity : MonoBehaviour
{
    [Header("Insanity values")]
    [SerializeField] 
    private float _maxInsanity;

    [SerializeField] 
    private float _impendingDoomTimer;
    [Space(10)]

    [Tooltip("Add the insanity bar game object here")]
    [SerializeField] private HealthBar InsanityBar;
    [Space(10)]

    [Tooltip("Add the static and dynamic values for each insanity tier here. Do not change the array size!")]
    [SerializeField] 
    private int[] staticInsanityValues;

    [SerializeField] 
    private int[] dynamicInsanityValues;

    // Events for each stage of insanity 
    public delegate void TutorialDebuff();
    public static event TutorialDebuff OnTutorialDebuff;

    public delegate void Paranoia();
    public static event Paranoia OnParanoia;

    public delegate void Slow();
    public static event Slow OnSlow;

    public delegate void Hallucination();
    public static event Hallucination OnHallucination;

    public delegate void ShadowClone();
    public static event ShadowClone OnShadowClone;

    public delegate void Monsters();
    public static event Monsters OnMonsters;

    public delegate void ImpendingDoom();
    public static event ImpendingDoom OnImpendingDoom;

    public delegate void PlayerDead();
    public static event PlayerDead onPlayerDeath;

    private float _currentInsanity;

    private Timer _timer;

    private bool _playerDying;

    private void Start()
    {
        if (!InsanityBar)
        {
            throw new System.Exception("Healthbar prefab missing!");
        }

        InsanityBar.SetValue(_currentInsanity);
        GlobalState.state.AudioManager.PlayerInsanityAudio(GetInsanityPercentage());
    }

    private void Update()
    {
        GlobalState.state.AudioManager.PlayerInsanityAudioUpdate(GetInsanityPercentage());

        // if a timer exists and the player is "dead" wait for timer to kill player
        if (!Object.ReferenceEquals( _timer, null) && _playerDying) 
        {
            _timer.Time += Time.deltaTime;

            if (_timer.Expired())
            {
                KillPlayer();
            }
        }
    }

    public float GetInsanity()
    {
        return _currentInsanity;
    }

    public float GetMaxInsanity()
    {
        return _maxInsanity;
    }

    public float GetInsanityPercentage()
    {
        return _currentInsanity / _maxInsanity * 100; 
    }

    public void SetMaxInsanity(float amount)
    {
        _maxInsanity = amount;
        InsanityBar.SetMaxValue(_maxInsanity);
    }

    public void IncrementMaxInsanity(float amount)
    {
        _maxInsanity += amount;
        InsanityBar.SetMaxValue(_maxInsanity);
    }

    // Sets insanity based on parameters
    public void SetInsanity(float amount)
    {
        // Insanity cannot be above max or below 0
        if (amount > _maxInsanity)
        {
            _currentInsanity = _maxInsanity;
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
        // Insanity cannot be above max or below 0
        if (amount + _currentInsanity > _maxInsanity)
        {
            _currentInsanity = _maxInsanity;
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
                OnImpendingDoom();
                _timer = new Timer(_impendingDoomTimer);
                _playerDying = true;
                break;
            /*case float n when (n > staticInsanityValues[5]):
                OnMonsters();
                break;
            case float n when (n > staticInsanityValues[4]):
                OnShadowClone();
                break;
            case float n when (n > staticInsanityValues[3]):
                OnHallucination();
                break;
            case float n when (n > staticInsanityValues[2]):
                OnSlow();
                break;
            case float n when (n > staticInsanityValues[1]):
                OnParanoia();
                break;
            case float n when (n > staticInsanityValues[0]):
                OnTutorialDebuff();
                break;*/
        }

        // Percentage based debuffs
        float currentInsanityPercentage = _currentInsanity / _maxInsanity * 100;

        /*switch (currentInsanityPercentage)
        {
            case float n when (n > dynamicInsanityValues[6]):
                OnImpendingDoom();
                break;
            case float n when (n > dynamicInsanityValues[5]):
                OnMonsters();
                break;
            case float n when (n > dynamicInsanityValues[4]):
                OnShadowClone();
                break;
            case float n when (n > dynamicInsanityValues[3]):
                OnHallucination();
                break;
            case float n when (n > dynamicInsanityValues[2]):
                OnSlow();
                break;
            case float n when (n > dynamicInsanityValues[1]):
                OnParanoia();
                break;
            case float n when (n > dynamicInsanityValues[0]):
                OnTutorialDebuff();
                break;
        }*/
    }

    public void KillPlayer()
    { 
        print("Killing Player");

        _playerDying = false;
        _timer.Reset();
        onPlayerDeath();
    }
}
