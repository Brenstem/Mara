using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering.HDPipeline;
using System;

[System.Serializable]
public struct Range
{
    public int start;
    public int end;
}

public class PlayerInsanity : EntityHealth
{
    [Header("References")]
    [SerializeField] private Volume vol;

    [Header("Player modifier variables")]
    [SerializeField] private float _hitstunBuffMultiplier = 1.5f;
    [SerializeField] private float _damageBuffMultiplier = 1.1f;
    [SerializeField] private float _moveSpeedBuffMultiplier = 1.1f;
    [SerializeField] private float _moveSpeedDebuffMultiplier = 0.9f; 
    [SerializeField] private float _attackSpeedMultiplier = 1.1f;

    [SerializeField] private int[] dynamicInsanityValues;
    [SerializeField] private int[] staticInsanityValues;
    [SerializeField] private float _hpIncreaseOnEnemyDeath;

    [Header("Buff Tier Values")]
    [SerializeField] private int _attackspeedBuff;
    [SerializeField] private int _hitstunBuff;
    [SerializeField] private Range _movespeedRange;
    [SerializeField] private int _damageBuff;

    [Header("Debuff tier values")]
    [SerializeField] private Range _whispersRange;
    [SerializeField] private Range _chromaticAberrationRange;
    [SerializeField] private Range _hallucinationsRange;
    [SerializeField] private Range _vignetteRange;

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
    // private float _moveSpeedMultiplier;
    private bool _playerDying;
    private float _moveSpeedMultiplier;
    private Timer _timer;
    private PlayerRevamp _player;
    private EntityModifier _playerModifier;
    private DebuffStates _debuffState;
    private BuffStates _buffState;
    private ChromaticAberration _chromaticAberration;
    private Vignette _vignette;
    private FilmGrain _filmGrain;

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

    private new void Awake()
    {
        base.Awake();
        BaseAIMovementController.onEnemyDeath += IncreaseMaxInsanity;
    }

    private void IncreaseMaxInsanity()
    {
        MaxHealth += _hpIncreaseOnEnemyDeath;
    }

    protected override void Start()
    {
        if (HealthBar != null)
        {
            HealthBar.SetMaxValue(MaxHealth);
        }

        CurrentHealth = 0;

        _player = GlobalState.state.Player.gameObject.GetComponent<PlayerRevamp>();
        _playerModifier = _player.modifier;

        GlobalState.state.AudioManager.PlayerInsanityAudio(0);


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

        /*if (_vignette != null)
        {
            _vignette.intensity.value = GetInsanityPercentage() / 200;
        }

        if (_filmGrain != null)
        {
            _filmGrain.intensity.value = GetInsanityPercentage() / 100;
        }*/
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
        // Static based buffs
        switch (CurrentHealth)
        {
            /*case float n when (n >= staticInsanityValues[4]): // Attack speed buff
                if (_buffState != BuffStates.attackSpeed)
                {
                    if (!_player.modifier.AttackSpeedMultiplier.isModified)
                    {
                        _player.modifier.AttackSpeedMultiplier = new Modifier(_attackSpeedMultiplier);
                        _player.playerAnimator.SetFloat("AttackSpeedModifier", _player.modifier.AttackSpeedMultiplier.multiplier);
                    }
                }
                _buffState = BuffStates.attackSpeed;
                TestResetBuffs();
                break;

            case float n when (n >= staticInsanityValues[3]): // Hitstun amount buff
                if (_buffState != BuffStates.hitStun)
                {
                    _player.modifier.HitstunMultiplier *= _hitstunBuffMultiplier;
                }
                _buffState = BuffStates.hitStun;
                TestResetBuffs();
                break;

            case float n when (n >= staticInsanityValues[2]): // Outline on shadows buff
                break;

            case float n when (n >= staticInsanityValues[1]): // Movement speed buff
                if (_buffState != BuffStates.movementSpeed)
                {
                    _moveSpeedMultiplier = _moveSpeedBuffMultiplier;
                    _player.IncreaseMoveSpeedOverValue(staticInsanityValues[1], staticInsanityValues[2], _moveSpeedMultiplier);
                }
                _buffState = BuffStates.movementSpeed;
                TestResetBuffs();
                break;

            case float n when (n >= staticInsanityValues[0]): // Attack damage buff
                if (_buffState != BuffStates.playerDamage)
                {
                    if (!_player.modifier.DamageMultiplier.isModified)
                        _player.modifier.DamageMultiplier = new Modifier(_damageBuffMultiplier);
                }
                _buffState = BuffStates.playerDamage;
                TestResetBuffs();
                break;

            case float n when (n < staticInsanityValues[0]): // Default buff state
                _buffState = BuffStates.defaultState;
                TestResetBuffs();
                break;*/
        }
        switch (CurrentHealth)
        {
            /* case float n when (n >= dynamicInsanityValues[4]): // Impending doom debuff
                if (_debuffState != DebuffStates.impendingDoom)
                {
                    PlayHeartBeat();
                }
                _debuffState = DebuffStates.impendingDoom;
                TestResetBuffs();
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
                TestResetBuffs();
                break;

            case float n when (n >= dynamicInsanityValues[2]): // Movement speed debuff
                if (_debuffState != DebuffStates.slow)
                {
                    PlayHeartBeat();
                }
                if (_debuffState != DebuffStates.slow && !_player.modifier.MovementSpeedMultiplier.isModified)
                {
                    _moveSpeedMultiplier = _moveSpeedDebuffMultiplier;
                }
                if (!_player.modifier.MovementSpeedMultiplier.isModified)
                {
                    _player.SlowOverValue(dynamicInsanityValues[2], dynamicInsanityValues[3], _moveSpeedMultiplier);
                }
                _debuffState = DebuffStates.slow;
                TestResetBuffs();
                break;

            case float n when (n >= dynamicInsanityValues[1]): // FX debuff
                if (_debuffState != DebuffStates.paranoia)
                {
                    PlayHeartBeat();
                }
                _debuffState = DebuffStates.paranoia;
                TestResetBuffs();
                break;

            case float n when (n >= dynamicInsanityValues[0]): // Tutorial debuff
                if (_debuffState != DebuffStates.tutorialDebuff)
                {
                    PlayHeartBeat();
                }
                _debuffState = DebuffStates.tutorialDebuff;
                TestResetBuffs();
                break;

            case float n when (n < dynamicInsanityValues[0]): // Standard state debuff
                _debuffState = DebuffStates.defaultState;
                TestResetBuffs();
                break;*/
        }

        #region Buffs
        float multiplier = 1;

        // Attack speed
        if (Step(_attackspeedBuff, CurrentHealth) == 1)
            multiplier = _attackSpeedMultiplier;

        _player.modifier.AttackSpeedMultiplier *= multiplier;

        // Hitstun
        multiplier = 1;
        if (Step(_hitstunBuff, CurrentHealth) == 1)
            multiplier = _hitstunBuffMultiplier;

        _player.modifier.HitstunMultiplier *= multiplier;

        // Movement speed
        _player.IncreaseMoveSpeedOverValue(_movespeedRange.start, _movespeedRange.end, _moveSpeedMultiplier);

        // Damage buff
        if (Step(_damageBuff, CurrentHealth) == 1)
            multiplier = _damageBuffMultiplier;

        _player.modifier.DamageMultiplier *= multiplier;

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
        intensity = SmoothStep(_vignetteRange.start, _vignetteRange.end, GetInsanityPercentage());

        if (_vignette != null)
            _vignette.intensity.value = intensity;
        #endregion

        /*if (CurrentHealth > staticInsanityValues[1] || Modifier.NearlyEquals(CurrentHealth, staticInsanityValues[1]))
        {
            
        }
        else if (GetInsanityPercentage() > dynamicInsanityValues[2] || Modifier.NearlyEquals(GetInsanityPercentage(), dynamicInsanityValues[2]))
        {
            _moveSpeedMultiplier = _moveSpeedDebuffMultiplier;
            _player.SlowOverValue(dynamicInsanityValues[2], dynamicInsanityValues[3], _moveSpeedMultiplier);
        }*/
    }

    private void TestResetBuffs()
    {
        // Reset function for attackspeed modifier just... doesn't work ??

        switch (_buffState)
        {
            case BuffStates.defaultState:
                _player.modifier.Reset();
                _player.playerAnimator.SetFloat("AttackSpeedModifier", _player.modifier.AttackSpeedMultiplier.multiplier);
                break;
            case BuffStates.playerDamage:
                _player.modifier.HitstunMultiplier.Reset();
                _player.modifier.AttackSpeedMultiplier *= 1.0f;
                _player.playerAnimator.SetFloat("AttackSpeedModifier", _player.modifier.AttackSpeedMultiplier.multiplier);
                break;
            case BuffStates.movementSpeed:
                _player.modifier.HitstunMultiplier.Reset();
                _player.modifier.AttackSpeedMultiplier.Reset();
                _player.playerAnimator.SetFloat("AttackSpeedModifier", _player.modifier.AttackSpeedMultiplier.multiplier);
                break;
            case BuffStates.heightenedSenses:
                _player.modifier.HitstunMultiplier.Reset();
                _player.modifier.AttackSpeedMultiplier *= 1.0f;
                _player.playerAnimator.SetFloat("AttackSpeedModifier", _player.modifier.AttackSpeedMultiplier.multiplier);
                break;
            case BuffStates.hitStun:
                _player.modifier.AttackSpeedMultiplier *= 1.0f;
                _player.playerAnimator.SetFloat("AttackSpeedModifier", _player.modifier.AttackSpeedMultiplier.multiplier);
                break;
            case BuffStates.attackSpeed:
                break;
            default:
                break;
        }

        switch (_debuffState)
        {
            case DebuffStates.defaultState:
                if (onDisableShadows != null)
                    onDisableShadows();
                break;
            case DebuffStates.tutorialDebuff:
                if (onDisableShadows != null)
                    onDisableShadows();
                break;
            case DebuffStates.paranoia:
                if (onDisableShadows != null)
                    onDisableShadows();
                break;
            case DebuffStates.slow:
                if (onDisableShadows != null)
                    onDisableShadows();
                break;
            case DebuffStates.hallucinations:
                break;
            case DebuffStates.impendingDoom:
                break;
            default:
                break;
        }
    }

    private void PlayHeartBeat()
    {
        GlobalState.state.AudioManager.PlayerInsanityHeartBeat(this.transform.position);
    }

    public override void Damage(HitboxValues hitbox)
    {
        CurrentHealth += hitbox.damageValue;
        HealthBar.SetValue(CurrentHealth);
        ActivateBuffs();
    }

    public override void Damage(float damage)
    {
        CurrentHealth += damage;
        ActivateBuffs();
    }

    // Returns value between 0 and 1 lerped over a range
    public float SmoothStep(float rangeStart, float rangeEnd, float current)
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

    public int Step(float rangeStart, float current)
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
}