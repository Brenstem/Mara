using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalState : MonoBehaviour
{
    [SerializeField] private GameObject _playerGameObject;

    [SerializeField] private Player _player;

    [SerializeField] private Camera _camera;

    [SerializeField] private AudioManager _audioManager;

    [SerializeField] private GameObject _playerMesh;

    [SerializeField] private CheckpointHandler _checkpointHandler;

    [SerializeField] private LayerMask _playerMask;

    [SerializeField] private LayerMask _enemyMask;


    public GameObject PlayerGameObject
    {
        get { return _playerGameObject; }
    }

    public Player Player
    {
        get { return _player; }
    }

    public Camera Camera
    {
        get { return _camera; }
    }

    public CheckpointHandler CheckpointHandler
    {
        get { return _checkpointHandler; }
    }

    public AudioManager AudioManager
    {
        get { return _audioManager;  }
    }

    public GameObject PlayerMesh
    {
        get { return _playerMesh; }
    }

    public LayerMask PlayerMask
    {
        get { return _playerMask; }
    }

    public LayerMask EnemyMask
    {
        get { return _enemyMask; }
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

    private void Awake() {
        if (_state != null && _state != this) {
            Destroy(this.gameObject);
        }
        else {
            DontDestroyOnLoad(this);
            _state = this;
        }
    }

    private void OnDestroy() {
        if (this == _state) {
            _state = null;
        }
    }

    private bool _hitstopRunning;
    public void HitStop(float duration)
    {
        StartCoroutine(HitStopCoroutine(duration));
    }

    private IEnumerator HitStopCoroutine(float duration)
    {
        _hitstopRunning = true;
        CinemachineImpulseManager.Instance.IgnoreTimeScale = true;
        GetComponent<CinemachineImpulseSource>().m_ImpulseDefinition.m_TimeEnvelope.m_DecayTime = duration;
        GetComponent<CinemachineImpulseSource>().GenerateImpulse();
        yield return new WaitForEndOfFrame();
        Time.timeScale = 0.0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        _hitstopRunning = false;
        yield return 0;
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