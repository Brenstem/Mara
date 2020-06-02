using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonologueTrigger : MonoBehaviour
{
    public bool onlyTriggerOnce;

    public Monologue monologue;


    private bool _hasTriggered;
    private LayerMask _triggerDetectionLayers;
    private MonologueManager _monologueManager;
    private GameObject _player;

    void Awake()
    {
        //ändra till singelton sen
        _monologueManager = FindObjectOfType<MonologueManager>();
        _player = GlobalState.state.Player.gameObject;
        _triggerDetectionLayers = (_triggerDetectionLayers | 1 << _player.gameObject.layer);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!_hasTriggered)
        {
            if (_triggerDetectionLayers == (_triggerDetectionLayers | 1 << other.gameObject.layer))
            {
                _hasTriggered = onlyTriggerOnce;
                triggerMonologue();
            }
        }
    }

    public void triggerMonologue()
    {
        _monologueManager.StartMonologue(monologue);
    }
}
