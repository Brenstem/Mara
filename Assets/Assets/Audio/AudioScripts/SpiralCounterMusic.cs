using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class SpiralCounterMusic : MonoBehaviour
{
    private Vector3 topPosition;
    private Vector3 playerPosition;
    [Range(0, 180)] public float verticalDiff;
    
    //  [EventRef]
    //  [SerializeField] string spiralEventAudio;
    //  EventInstance spiralEvent;

    // Update is called once per frame
    void Update()
    {
        playerPosition = GlobalState.state.Player.transform.position;

        verticalDiff = playerPosition.y - topPosition.y;

        verticalDiff = verticalDiff / topPosition.y;

        if (verticalDiff > 1.8f)
        {
            verticalDiff = 1.8f;
        }
        if (verticalDiff < 0)
        {
            FMODUnity.RuntimeManager.StudioSystem.setParameterByName("SpiralStairsProgress", 0f);
        }

        FMODUnity.RuntimeManager.StudioSystem.setParameterByName("SpiralStairsProgress", verticalDiff);
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            topPosition = other.transform.position;
            //spiralEvent = RuntimeManager.CreateInstance(spiralEventAudio);
            // spiralEvent.start();
        }
    }
}
