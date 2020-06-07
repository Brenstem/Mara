using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering.HDPipeline;
using System;
using FMOD;

[System.Serializable]
public struct Range
{
    public float start;
    public float end;
}

public class PlayerInsanity : EntityHealth
{
    [Header("References")]
    [SerializeField] private Volume vol;

    private PlayerRevamp _player;
    private PlayerRevamp Player
    {
        get
        {
            if (_player == null)
                _player = GlobalState.state.Player;
            return _player;
        }
    }

    private ChromaticAberration _chromaticAberration;
    private Vignette _vignette;
    private FilmGrain _filmGrain;

    [Header("Heal over time")]
    [SerializeField] private float _healAmount = 0.1f;
    [SerializeField] private float _timeBeforeHeal = 2;

    private Timer _healTimer;
    private float _lastTierHealth;

    [Header("Player modifier variables")]
    [SerializeField] private float _hitstunBuffMultiplier = 1.5f;
    [SerializeField] private float _damageBuffMultiplier = 1.1f;
    [SerializeField] private float _moveSpeedBuffMultiplier = 1.1f;
    [SerializeField] private float _attackSpeedMultiplier = 1.1f;

    [Header("Buff Tier Values")]
    [SerializeField] private int _attackspeedBuff;
    [SerializeField] private int _hitstunBuff;
    [SerializeField] private Range _movespeedRange;
    [SerializeField] private int _damageBuff;
    [SerializeField] private int _actionAttackBuff;

    [Header("Debuff tier values")]
    [SerializeField] private Range _whispersRange;
    [SerializeField] private Range _chromaticAberrationRange;
    [SerializeField] private Range _hallucinationsRange;
    [SerializeField] private Range _vignetteRange;

    [Header("Temp")]
    [SerializeField] private Light _playerLight;


    public override float CurrentHealth
    {
        get { return _currentHealth; }
        set
        {
            if (value <= 0)
            {
                _currentHealth = 0;
            }
            else if (value > MaxHealth)
            {
                _currentHealth = MaxHealth;
                KillThis();
            }
            else
            {
                _currentHealth = value;
            }

            if (HealthBar != null)
            {
                HealthBar.SetValue(value);
            }
        }
    }

    private new void Awake()
    {
        base.Awake();
        _healTimer = new Timer(0);
    }

    protected override void Start()
    {
        if (_playerLight)
        {
            _playerLight.intensity = GetInsanityPercentage();
        }

        if (HealthBar != null)
        {
            HealthBar.SetMaxValue(MaxHealth);
        }

        CurrentHealth = 0;

        GlobalState.state.AudioManager.PlayerInsanityAudio(0);
        GlobalState.state.AudioManager.PlayerInsanityAudioUpdate(0);


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
    }

    private void Update()
    {
        if (_filmGrain != null)
        {
            _filmGrain.intensity.value = GetInsanityPercentage() / 100;
        }

        if (_healTimer.Expired)
        {
            Heal(_healAmount * Time.deltaTime, _lastTierHealth);
        }
        else
        {
            _healTimer += Time.deltaTime;
        }
    }

    // PUBLIC FUNCTIONS
    public void Heal(float amount)
    {
        CurrentHealth -= amount;
        ActivateBuffs();
    }

    public void Heal(float amount, float targetHealth)
    {
        if (CurrentHealth > targetHealth)
        {
            CurrentHealth -= amount;
            ActivateBuffs();
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

    public override void Damage(HitboxValues hitbox)
    {
        _healTimer = new Timer(_timeBeforeHeal);

        CurrentHealth += hitbox.damageValue;
        HealthBar.SetValue(CurrentHealth);
        ActivateBuffs();
        UpdateHealthTier();
    }

    public override void Damage(float damage)
    {
        _healTimer = new Timer(_timeBeforeHeal);

        CurrentHealth += damage;
        ActivateBuffs();
        UpdateHealthTier();
    }

    // PRIVATE FUNCTIONS

    private void UpdateHealthTier()
    {
        // Sets health to heal to based on insanity debuff tier
        switch (GetInsanityPercentage())
        {
            case float n when (n >= _vignetteRange.start):
                _lastTierHealth = (_vignetteRange.start / 100) * MaxHealth;
                break;
            case float n when (n >= _hallucinationsRange.start):
                _lastTierHealth = (_hallucinationsRange.start / 100) * MaxHealth;
                break;
            case float n when (n >= _chromaticAberrationRange.start):
                _lastTierHealth = (_chromaticAberrationRange.start / 100) * MaxHealth;
                break;
            case float n when (n >= _whispersRange.start):
                _lastTierHealth = (_whispersRange.start / 100) * MaxHealth;
                break;
            default:
                _lastTierHealth = 0;
                break;
        }
    }

    private void ActivateBuffs()
    {
        if (_playerLight)
        {
            _playerLight.intensity = GetInsanityPercentage();
        }

        #region Buffs
        float multiplier = 1;

        // Attack speed
        if (Step(_attackspeedBuff, CurrentHealth) == 1)
            multiplier = _attackSpeedMultiplier;

        Player.modifier.AttackSpeedMultiplier *= multiplier;

        // Hitstun
        multiplier = 1;
        if (Step(_hitstunBuff, CurrentHealth) == 1)
            multiplier = _hitstunBuffMultiplier;

        Player.modifier.HitstunMultiplier *= multiplier;

        // Movement speed
        Player.IncreaseMoveSpeedOverValue(_movespeedRange.start, _movespeedRange.end, _moveSpeedBuffMultiplier);

        // Damage buff
        multiplier = 1;
        if (Step(_damageBuff, CurrentHealth) == 1)
            multiplier = _damageBuffMultiplier; 


        Player.modifier.DamageMultiplier *= multiplier;

        // Action attack buff
        Player.actionAttackActive = false;
        if (Step(_actionAttackBuff, CurrentHealth) == 1)
            Player.actionAttackActive = true; 
        #endregion

        #region Debuff FX
        float intensity;

        // Whispers audio
        intensity = SmoothStep(_whispersRange.start, _whispersRange.end, GetInsanityPercentage());
        intensity *= 100; // player insanity audio wants percentage value between 0 and 100
        GlobalState.state.AudioManager.PlayerInsanityAudioUpdate(intensity);

        // Chromatic aberration
        intensity = SmoothStep(_chromaticAberrationRange.start, _chromaticAberrationRange.end, GetInsanityPercentage());

        if (_chromaticAberration != null)
            _chromaticAberration.intensity.value = intensity;

        // Vignette
        intensity = Mathf.Clamp(SmoothStep(_vignetteRange.start, _vignetteRange.end, GetInsanityPercentage()), 0, 0.35f);

        if (_vignette != null)
            _vignette.intensity.value = intensity;
        #endregion
    }

    private void PlayHeartBeat()
    {
        GlobalState.state.AudioManager.PlayerInsanityHeartBeat(this.transform.position);
    }

    // Returns value between 0 and 1 lerped over a range
    private float SmoothStep(float rangeStart, float rangeEnd, float current)
    {
        if (current < rangeStart)
        {
            return 0;
        }

        if (current > rangeEnd)
        {
            return 1;
        }
        
        float range = rangeEnd - rangeStart;

        current -= rangeStart;

        float interpolationValue = Mathf.Clamp(current / range, 0, 1);

        float finalMultiplier = Mathf.Lerp(0, 1, interpolationValue);

        return finalMultiplier;
    }

    private int Step(float rangeStart, float current)
    {
        if (current < rangeStart)
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }

    private void IncreaseMaxInsanity(float amount)
    {
        MaxHealth += amount;
    }
}