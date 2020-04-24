using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class SpiralCounterMusic : MonoBehaviour
{
    private Vector3 topPosition;
    private Vector3 playerPosition;
    [Range(0,100)] public float verticalDiff;

    [EventRef]
    //[SerializeField] string InsanityEventAudio;
    EventInstance spiralEvent;

    // Update is called once per frame
    void Update()
    {
        playerPosition = GlobalState.state.Player.transform.position;

        verticalDiff = playerPosition.y - topPosition.y;

        verticalDiff = verticalDiff / topPosition.y;

        if (verticalDiff > 1)
        {
            verticalDiff = 1;
        }

        spiralEvent.setParameterByName("SpiralStairsProgress", verticalDiff);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            topPosition = other.transform.position;
        }
    }
}
