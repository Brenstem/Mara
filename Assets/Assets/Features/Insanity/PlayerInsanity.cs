using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering.HDPipeline;
using System;

public class PlayerInsanity : EntityHealth
{
    [Header("References")]
    [SerializeField] private Volume vol;

    [Header("Variables")]
    [SerializeField] private float _hitstunBuffMultiplier = 1.5f;
    [SerializeField] private float _damageBuffMultiplier = 1.1f;

    [Header("Insanity tier values")]
    [Tooltip("Add the static and dynamic values for each insanity tier here. Do not change the array size!")]
    [SerializeField]  int[] staticInsanityValues;
    [SerializeField] private int[] dynamicInsanityValues;


    #region Insanity Tier Events
    // Events for each stage of insanity 
    public delegate void Slow();
    public static event Slow onSlow;

    public delegate void Hallucination();
    public static event Hallucination onHallucination;

    public delegate void ShadowClone();
    public static event ShadowClone onShadowClone;

    public delegate void Monsters();
    public static event Monsters onMonsters;
    
    public delegate void IncreaseMovementSpeed();
    public static event IncreaseMovementSpeed onIncreaseMovementSpeed;

    public delegate void IncreaseAttackSpeed();
    public static event IncreaseAttackSpeed onIncreaseAttackSpeed;
    
    public delegate void DisableShadows();
    public static event DisableShadows onDisableShadows;
    #endregion

    // PRIVATE VARIABLES
    private bool _playerDying;
    private Timer _timer;
    private HitboxModifier _playerModifier;
    private DebuffStates _debuffState;
    private BuffStates _buffState;
    private ChromaticAberration _chromaticAberration;
    private Vignette _vignette;
    private FilmGrain _filmGrain;

    // Insanity tier states
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

    protected new void Start()
    {
        base.Start();

        _playerModifier = GlobalState.state.Player.gameObject.GetComponent<PlayerRevamp>().modifier;

        if (vol != null)
        {
            ChromaticAberration chroTmp;
            Vignette vigTmp;
            FilmGrain filTmp;

            if (vol.profile.TryGet<ChromaticAberration>(out chroTmp))
            {
                _chromaticAberration = chroTmp;
            }

            if (vol.profile.TryGet<Vignette>(out vigTmp))
            {
                _vignette = vigTmp;
            }

            if (vol.profile.TryGet<FilmGrain>(out filTmp))
            {
                _filmGrain = filTmp;
            }
        }
        

        if (!HealthBar)
        {
            throw new System.Exception("Healthbar prefab missing!");
        }

        GlobalState.state.AudioManager.PlayerInsanityAudio(GetInsanityPercentage());
    }

    private void Update()
    {
        GlobalState.state.AudioManager.PlayerInsanityAudioUpdate(GetInsanityPercentage());

        if (_chromaticAberration != null)
        {
            _chromaticAberration.intensity.value = GetInsanityPercentage() / 100;
        }

        if (_vignette != null)
        {
            _vignette.intensity.value = GetInsanityPercentage() / 200;
        }

        if (_filmGrain != null)
        {
            _filmGrain.intensity.value = GetInsanityPercentage() / 100;
        }
    }

    public float GetInsanityPercentage()
    {
        return CurrentHealth / MaxHealth * 100; 
    }

    public void SetInsanity(float amount)
    {
        // Insanity cannot be above max or below 0
        if (amount > MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }
        else if (amount < 0)
        {
            CurrentHealth = 0;
        }
        else
        {
            CurrentHealth = amount;
        }

        ActivateBuffs();
        HealthBar.SetValue(CurrentHealth);
    }

    public void ActivateBuffs()
    {
        onSlow();
        onIncreaseMovementSpeed();
        
        // Static based buffs
        switch (CurrentHealth)
        {
            case float n when (n >= staticInsanityValues[4]): // Attack speed buff
                break;
            case float n when (n >= staticInsanityValues[3]): // Hitstun amount buff
                if (_buffState != BuffStates.hitStun)
                {
                    if (_playerModifier.HitstunMultiplier == 1.0f)
                        _playerModifier.HitstunMultiplier = _hitstunBuffMultiplier;
                }
                _buffState = BuffStates.hitStun;
                break;

            case float n when (n >= staticInsanityValues[2]): // Outline on shadows buff
                break;

            case float n when (n >= staticInsanityValues[1]): // Movement speed buff
                _buffState = BuffStates.movementSpeed;
                break;

            case float n when (n >= staticInsanityValues[0]): // Attack damage buff
                if (_buffState != BuffStates.playerDamage)
                {
                    if (_playerModifier.DamageMultiplier == 1.0f)
                        _playerModifier.DamageMultiplier = _damageBuffMultiplier;
                }
                _buffState = BuffStates.playerDamage;
                break;

            case float n when (n < staticInsanityValues[0]): // Default buff state
                _playerModifier.Reset();
                _buffState = BuffStates.defaultState;
                break;
        }

        // Percentage based debuffs
        float currentInsanityPercentage = CurrentHealth / MaxHealth * 100;

        switch (currentInsanityPercentage)
        {
            case float n when (n >= dynamicInsanityValues[4]): // Impending doom debuff
                if (_debuffState != DebuffStates.impendingDoom)
                {
                    PlayHeartBeat();
                }
                _debuffState = DebuffStates.impendingDoom;
                break;

            case float n when (n >= dynamicInsanityValues[3]): // Shadows appear debuff
                if (onHallucination != null)
                {
                    onHallucination();
                }
                if (_debuffState != DebuffStates.hallucinations)
                {
                    PlayHeartBeat();
                }
                _debuffState = DebuffStates.hallucinations;
                break;

            case float n when (n >= dynamicInsanityValues[2]): // Movement speed debuff
                if (_debuffState != DebuffStates.slow)
                {
                    PlayHeartBeat();
                }
                _debuffState = DebuffStates.slow;
                if (onDisableShadows != null)
                    onDisableShadows();
                break;

            case float n when (n >= dynamicInsanityValues[1]): // FX debuff
                if (_debuffState != DebuffStates.paranoia)
                {
                    PlayHeartBeat();
                }
                _debuffState = DebuffStates.paranoia;
                if (onDisableShadows != null)
                    onDisableShadows();
                break;

            case float n when (n >= dynamicInsanityValues[0]): // Tutorial debuff
                if (_debuffState != DebuffStates.tutorialDebuff)
                {
                    PlayHeartBeat();
                }
                _debuffState = DebuffStates.tutorialDebuff;
                if (onDisableShadows != null)
                    onDisableShadows();
                break;

            case float n when (n < dynamicInsanityValues[0]): // Standard state debuff
                _debuffState = DebuffStates.defaultState;
                if (onDisableShadows != null)
                    onDisableShadows();
                break;
        }
    }

    private void PlayHeartBeat()
    {
        GlobalState.state.AudioManager.PlayerInsanityHeartBeat(this.transform.position);
    }

    public override void Damage(HitboxValues hitbox)
    {
        ActivateBuffs();
        base.CurrentHealth -= hitbox.damageValue;
        base.HealthBar.SetValue(base.CurrentHealth);
    }
}