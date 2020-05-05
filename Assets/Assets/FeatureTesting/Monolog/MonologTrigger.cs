using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonologTrigger : MonoBehaviour
{
    public bool onlyTriggerOnce;

    public Monolog monolog;


    private bool _hasTriggered;
    private LayerMask _triggerDetectionLayers;
    private MonologManager _monologManager;
    private GameObject _player;

    void Awake()
    {
        //ändra till singelton sen
        _monologManager = FindObjectOfType<MonologManager>();
        _player = GlobalState.state.PlayerGameObject;
        _triggerDetectionLayers = _triggerDetectionLayers | 1 << _player.gameObject.layer;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!_hasTriggered)
        {
            if (_triggerDetectionLayers == (_triggerDetectionLayers | 1 << other.gameObject.layer))
            {
                _hasTriggered = onlyTriggerOnce;
                triggerMonolog();
            }
        }
    }

    public void triggerMonolog()
    {
        _monologManager.StartMonolog(monolog);
    }
}
