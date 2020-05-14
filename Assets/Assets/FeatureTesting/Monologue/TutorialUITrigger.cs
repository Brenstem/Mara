using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialUITrigger : MonoBehaviour
{
    public GameObject tutorialToDisplay;

    private LayerMask _triggerDetectionLayers;
    private GameObject _player;

    void Awake()
    {
        _player = GlobalState.state.Player.gameObject;
        _triggerDetectionLayers = (_triggerDetectionLayers | 1 << _player.gameObject.layer);
    }

    //vet inte om enter/exit är helt consistent men känns dumt att göra med stay 
    void OnTriggerEnter(Collider other)
    {
        if (_triggerDetectionLayers == (_triggerDetectionLayers | 1 << other.gameObject.layer))
        {
            //spela eventuell animation här
            tutorialToDisplay.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (_triggerDetectionLayers == (_triggerDetectionLayers | 1 << other.gameObject.layer))
        {
            //spela eventuell animation här
            tutorialToDisplay.SetActive(false);
        }
    }
}
