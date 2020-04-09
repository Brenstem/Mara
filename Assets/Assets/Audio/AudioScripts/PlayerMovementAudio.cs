using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class PlayerMovementAudio : MonoBehaviour
{
    [EventRef]
    public string PlayerFootstepEvent;
    EventInstance PlayerFootsteps;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void AudioPlayerFootsteps(string GroundMaterial) // denna behöver få ett groundmaterial-värde för att kunna funka
    {
        PlayerFootsteps = RuntimeManager.CreateInstance(PlayerFootstepEvent);

        switch (GroundMaterial)
        {
            case "Stone":
                PlayerFootsteps.setParameterByName("Surface", 0f);
                break;
            case "Metal":
                PlayerFootsteps.setParameterByName("Surface", 1f);
                break;
            case "Wood":
                PlayerFootsteps.setParameterByName("Surface", 2f);
                break;

        }
        PlayerFootsteps.start();
        
    }
}
