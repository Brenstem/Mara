using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class SpiralCounterMusic : MonoBehaviour
{
    private Vector3 topPosition;
    private Vector3 playerPosition;
    private float verticalDiff;

    [EventRef]
    //[SerializeField] string InsanityEventAudio;
    EventInstance spiralEvent;

    // Update is called once per frame
    void Update()
    {
        playerPosition = GlobalState.state.Player.transform.position;

        verticalDiff = topPosition.y - playerPosition.y;

        verticalDiff = verticalDiff / topPosition.y * 100;

        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            topPosition = other.transform.position;
        }
    }
}
