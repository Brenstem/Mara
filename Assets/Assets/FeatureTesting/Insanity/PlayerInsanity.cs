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
    public static event TutorialDebuff onTutorialDebuff;

    public delegate void Paranoia();
    public static event Paranoia onParanoia;

    public delegate void Slow();
    public static event Slow onSlow;

    public delegate void Hallucination();
    public static event Hallucination onHallucination;

    public delegate void ShadowClone();
    public static event ShadowClone onShadowClone;

    public delegate void Monsters();
    public static event Monsters onMonsters;

    public delegate void ImpendingDoom();
    public static event ImpendingDoom onImpendingDoom;

    public delegate void PlayerDead();
    public static event PlayerDead onPlayerDeath;

    public delegate void DamageBuff();
    public static event DamageBuff onPlayerDamageBuff;

    public delegate void IncreaseMovementSpeed();
    public static event IncreaseMovementSpeed onIncreaseMovementSpeed;

    public delegate void HeightenedSenses();
    public static event HeightenedSenses onHeightenedSenses;

    public delegate void IncreaseHitstun();
    public static event IncreaseHitstun onIncreaseHitstun;

    public delegate void IncreaseAttackSpeed();
    public static event IncreaseAttackSpeed onIncreaseAttackSpeed;

    public delegate void ResetDamageBuff();
    public static event ResetDamageBuff onDefaultBuff;

    private float _currentInsanity;

    private Timer _timer;

    private bool _playerDying;

    private DebuffStates _debuffState;

    private BuffStates _buffState;

    private enum DebuffStates
    {
        defaultState,
        tutorialDebuff,
        paranoia,
        slow,
        hallucinations,
        impendingDoom
    }

    private enum BuffStates
    {
        defaultState,
        playerDamage,
        movementSpeed,
        heightenedSenses,
        hitStun,
        attackSpeed
    }

    private void Start()
    {
        ActivateBuffs();

        if (!InsanityBar)
        {
            throw new System.Exception("Healthbar prefab missing!");
        }

        InsanityBar.SetValue(_currentInsanity);
        InsanityBar.SetMaxValue(_maxInsanity);
        GlobalState.state.AudioManager.PlayerInsanityAudio(GetInsanityPercentage());
    }

    private void Update()
    {
        GlobalState.state.AudioManager.PlayerInsanityAudioUpdate(GetInsanityPercentage());

        // if a timer exists and the player is "dead" wait for timer to kill player
        if (!Object.ReferenceEquals( _timer, null) && _playerDying) 
        {
            _timer.Time += Time.deltaTime;

            if (_timer.Expired)
            {
                KillPlayer();
            }
        }

        print(_buffState);
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
        onIncreaseMovementSpeed();
        onSlow();

        // Static based buffs
        switch (_currentInsanity)
        {
            case float n when (n >= staticInsanityValues[4]):
                //onIncreaseAttackSpeed();
                break;
            case float n when (n >= staticInsanityValues[3]):
                if (_buffState != BuffStates.hitStun)
                {
                    onIncreaseHitstun();
                }
                _buffState = BuffStates.hitStun;
                break;
            case float n when (n >= staticInsanityValues[2]):
                onHeightenedSenses();
                break;
            case float n when (n >= staticInsanityValues[1]):
                _buffState = BuffStates.movementSpeed;
                break;
            case float n when (n >= staticInsanityValues[0]):
                if (_buffState != BuffStates.playerDamage)
                {
                    onPlayerDamageBuff();
                }
                _buffState = BuffStates.playerDamage;
                break;
            case float n when (n < staticInsanityValues[0]):
                onDefaultBuff();
                _buffState = BuffStates.defaultState;
                break;
        }

        // Percentage based debuffs
        float currentInsanityPercentage = _currentInsanity / _maxInsanity * 100;

        switch (currentInsanityPercentage)
        {

            case float n when (n >= dynamicInsanityValues[4]):

                if (_debuffState != DebuffStates.impendingDoom)
                {
                    PlayHeartBeat();
                    KillPlayer();
                }

                _debuffState = DebuffStates.impendingDoom;
                break;
            case float n when (n >= dynamicInsanityValues[3]):
                //onHallucination();
                if (_debuffState != DebuffStates.hallucinations)
                {
                    PlayHeartBeat();
                }

                _debuffState = DebuffStates.hallucinations;
                break;
            case float n when (n >= dynamicInsanityValues[2]):
                if (_debuffState != DebuffStates.slow)
                {
                    PlayHeartBeat();
                }

                _debuffState = DebuffStates.slow;
                break;
            case float n when (n >= dynamicInsanityValues[1]):
                //onParanoia();
                if (_debuffState != DebuffStates.paranoia)
                {
                    PlayHeartBeat();
                }

                _debuffState = DebuffStates.paranoia;
                break;
            case float n when (n >= dynamicInsanityValues[0]):
                if (_debuffState != DebuffStates.tutorialDebuff)
                {
                    // onTutorialDebuff();
                    PlayHeartBeat();
                }

                _debuffState = DebuffStates.tutorialDebuff;
                break;
            case float n when (n < dynamicInsanityValues[0]):
                _debuffState = DebuffStates.defaultState;
                break;
        }
    }

    public void PlayHeartBeat()
    {
        GlobalState.state.AudioManager.PlayerInsanityHeartBeat();
    }


    public void KillPlayer()
    { 
        print("Killing Player");

        _playerDying = false;
        if (!Object.ReferenceEquals(_timer, null))
        {
            _timer.Reset();
        }
        onPlayerDeath();
    }
}