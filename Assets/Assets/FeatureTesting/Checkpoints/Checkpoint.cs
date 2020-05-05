using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] 
    private Transform _respawnPosition;

    [SerializeField]
    private bool _maxHealthOnRespawn;

    [SerializeField]
    private bool _playSound;

    [SerializeField]
    private bool _refillInsanity;

    [SerializeField]
    private float _healAmount;

    private void OnTriggerEnter(Collider hitInfo)
    {
        if (hitInfo.CompareTag("Player"))
        {
            GlobalState.state.CheckpointHandler.ActivateCheckpoint(_respawnPosition, _maxHealthOnRespawn);

            if (_playSound)
            {
                GlobalState.state.AudioManager.CheckpointAudio(this.transform.position);
                _playSound = false;
            }
        }
    }

    private void OnTriggerStay(Collider hitInfo)
    {
        if (hitInfo.CompareTag("Player"))
        {
            hitInfo.GetComponent<PlayerInsanity>().Damage(-_healAmount);
        }
    }
}
