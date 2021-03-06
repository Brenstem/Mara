﻿using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalState : MonoBehaviour
{
    public delegate void LoadOptionsEvent();
    public static event LoadOptionsEvent onLoadOptions;

    [Header("Hitstop")]
    [SerializeField, Range(0.0f, 1.0f)] private float _entryTimeFraction = 0.043f;
    [SerializeField, Range(0.0f, 1.0f)] private float _exitTimeFraction = 0.22f;
    /*
    [SerializeField] private float _minHitstopTime;
    [SerializeField] private float _maxHitstopTime;
    [SerializeField] private float _maxDamageHitstopThreshold;
    */
    [Header("References")]
    [SerializeField] private PlayerRevamp _player;

    [SerializeField] private Camera _camera;

    [SerializeField] private Cinemachine.CinemachineStateDrivenCamera _stateDrivenCamera;

    [SerializeField] private AudioManager _audioManager;

    [SerializeField] private CheckpointHandler _checkpointHandler;

    [SerializeField] private Fade _gameOver;

    [SerializeField] private LayerMask _playerMask;

    [SerializeField] private LayerMask _enemyMask;

    [SerializeField] private LayerMask _groundMask;

    private StartMenuManagerTest _startMenuManager;

    [SerializeField] public enum LanguageEnum
    {
        Swedish,
        English
    };

    public LanguageEnum language;

    [Header("Debug")]
    [SerializeField] private bool _lockCursorOnStart = true;

    public bool GameStarted { get { return SceneData.gameStarted;  } set { SceneData.gameStarted = value; } }

    public bool LockCursor
    {
        set
        {
            if (value)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Confined;
            }
        }
    }

    public PlayerRevamp Player
    {
        get { return _player; }
    }

    public Camera Camera
    {
        get { return _camera; }
    }

    public Cinemachine.CinemachineStateDrivenCamera StateDrivenCamera
    {
        get { return _stateDrivenCamera; }
    }

    public CheckpointHandler CheckpointHandler
    {
        get { return _checkpointHandler; }
    }

    public AudioManager AudioManager
    {
        get { return _audioManager;  }
    }

    public Fade GameOver
    {
        get { return _gameOver; }
    }

    public LayerMask PlayerMask
    {
        get { return _playerMask; }
    }

    public LayerMask EnemyMask
    {
        get { return _enemyMask; }
    }

    public LayerMask GroundMask
    {
        get { return _groundMask; }
    }

    public LanguageEnum CurrentLanguage
    {
        get { return language; }
    }

    private static GlobalState _state;
    public static GlobalState state {
        get {
            if (_state == null)
            {
                _state = GameObject.FindObjectOfType<GlobalState>();
            }
            return _state;
        }
    }

    private float _previousEntryFrac;
    private float _previousExitFrac;
    private void Awake()
    {
        _previousEntryFrac = _entryTimeFraction;
        _previousExitFrac = _exitTimeFraction;

        if (_state != null && _state != this) {
            Destroy(this.gameObject);
        }
        else {
            // DontDestroyOnLoad(this);
            _state = this;
        }

        _startMenuManager = GetComponent<StartMenuManagerTest>();
    }

    private void Start()
    {
        //debug 
        LockCursor = _lockCursorOnStart;
        if (!SceneData.gameStarted)
        {
            _player.EnabledControls = _lockCursorOnStart;
        }
        else
        {
            LockCursor = true;
            GlobalState.state.GameOver.FadeToTransparent();
        }
    }

    private void OnDestroy() {
        if (this == _state) {
            _state = null;
        }
    }

    public void LoadEvent()
    {
        if (onLoadOptions != null)
            onLoadOptions();
    }


    private bool _hitstopRunning;

    public void HitStop(HitboxValues values)
    {
        /*
        float duration = 0.0f;
        if (values.damageValue >= _maxDamageHitstopThreshold)
        {
            duration = _maxHitstopTime;
        }
        else
        {
            float fraction = values.damageValue / _maxDamageHitstopThreshold;
            duration = Mathf.Lerp(_minHitstopTime, _maxHitstopTime, fraction >= 1 ? 1.0f : fraction);
            print(duration);
        }
        */
        StartCoroutine(HitStopCoroutine(values.hitstopTime));
    }

    private IEnumerator HitStopCoroutine(float duration)
    {

        _hitstopRunning = true;
        CinemachineImpulseManager.Instance.IgnoreTimeScale = true;
        GetComponent<CinemachineImpulseSource>().m_ImpulseDefinition.m_TimeEnvelope.m_DecayTime = duration;
        //GetComponent<CinemachineImpulseSource>().m_ImpulseDefinition.m_AmplitudeGain = 1.5f + values.damageValue / 100; // todo nästa
        GetComponent<CinemachineImpulseSource>().GenerateImpulse();
        yield return new WaitForEndOfFrame();


        float time = 0.0f;
        float exitTime = 0.0f;
        float tScale = 0.0f;

        while (time < duration)
        {
            if (duration < _exitTimeFraction || time / duration > 1 - _exitTimeFraction)
            {
                exitTime += Time.unscaledDeltaTime;
                if (_exitTimeFraction > 0)
                    tScale = Mathf.Lerp(0.0f, 1.0f, (exitTime * exitTime) / (_exitTimeFraction * _exitTimeFraction));
            }
            else if (time / duration <= _entryTimeFraction)
            {
                if (_entryTimeFraction > 0)
                    tScale = Mathf.Lerp(1.0f, 0.0f, (time * time) / (_entryTimeFraction * _entryTimeFraction));
                else
                    tScale = 0.0f;
            }
            else
            {
                tScale = 0.0f;
            }

            Time.timeScale = tScale;
            time += Time.unscaledDeltaTime;
            yield return new WaitForEndOfFrame();
        }
        

        Time.timeScale = 1f;
        _hitstopRunning = false;
        yield return 0;
    }

    public GameObject CANVAS;

    public void Pause()
    {
        _startMenuManager.ToggleStartGame();
    }

    private void OnValidate()
    {
        bool exceeded = _entryTimeFraction + _exitTimeFraction > 1 ? true : false;
        if (exceeded)
        {
            if (_previousEntryFrac != _entryTimeFraction)
                _exitTimeFraction = 1 - _entryTimeFraction;
            else if (_previousExitFrac != _exitTimeFraction)
                _entryTimeFraction = 1 - _exitTimeFraction;
        }

        _previousEntryFrac = _entryTimeFraction;
        _previousExitFrac = _exitTimeFraction;
    }

    /*
    private IEnumerator _ProcessShake(float shakeIntensity = 5f, float shakeTiming = 0.5f)
    {
        Noise(1, shakeIntensity);
        yield return new WaitForSeconds(shakeTiming);
        Noise(0, 0);
    }

    public void Noise(float amplitudeGain, float frequencyGain)
    {
        cmFreeCam.topRig.Noise.m_AmplitudeGain = amplitudeGain;
        cmFreeCam.middleRig.Noise.m_AmplitudeGain = amplitudeGain;
        cmFreeCam.bottomRig.Noise.m_AmplitudeGain = amplitudeGain;

        cmFreeCam.topRig.Noise.m_FrequencyGain = frequencyGain;
        cmFreeCam.middleRig.Noise.m_FrequencyGain = frequencyGain;
        cmFreeCam.bottomRig.Noise.m_FrequencyGain = frequencyGain;

    }
    */
}