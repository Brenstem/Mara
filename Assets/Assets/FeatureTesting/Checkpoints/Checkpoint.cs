using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [SerializeField] 
    private Transform respawnPosition;

    [SerializeField]
    private bool maxHealthOnRespawn;

    private void OnTriggerEnter(Collider hitInfo)
    {
        if (hitInfo.CompareTag("Player"))
        {
            GlobalState.state.CheckpointHandler.ActivateCheckpoint(respawnPosition, maxHealthOnRespawn);
        }
    }
}
