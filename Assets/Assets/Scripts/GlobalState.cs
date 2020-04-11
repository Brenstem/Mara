using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalState : MonoBehaviour
{
    [SerializeField]
    private GameObject _player;

    public GameObject Player
    {
        get { return _player; }
    }

    private static GlobalState _state;
    public static GlobalState state {
        get { return _state; }
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