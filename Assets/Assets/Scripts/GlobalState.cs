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
}