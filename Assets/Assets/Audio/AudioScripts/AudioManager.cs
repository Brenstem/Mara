using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class AudioManager : MonoBehaviour
{
    [EventRef]
    public string InsanityEventAudio;
    EventInstance InsanityEvent;

    //Player sounds
    public void PlayerFootStepsAudio(Transform obj)
    {
        // playerMovementAudio.AudioPlayerFootsteps(obj.tag); // betyder att den utgår från objektets tag istället för name
    }

    public void PlayerSwordSwingAudio()
    {

    }

    public void PlayerDodgeAudio()
    {

    }

    public void PlayerJumpAudio()
    {

    }

    public void PlayerInsanityAudio(float insanityPercentage)
    {
        InsanityEvent = RuntimeManager.CreateInstance(InsanityEventAudio); // Create a new FMOD::Studio::EventInstance.
        InsanityEvent.setParameterByName("InsanityBar", insanityPercentage); // string-värdet är parameternamnet och insanitypercentage är float-värdet
        InsanityEvent.start(); // spelar upp ljudet
    }

    public void PlayerInsanityAudioUpdate(float insanityPercentage)
    {
        InsanityEvent.setParameterByName("InsanityBar", insanityPercentage);
    }

    //Enemy sound
    public void EnemyStalkerIdleAudio()
    {

    }

    public void EnemyStalkerDieAudio()
    {

    }

    //Music
    public void MenuMusicAudio()
    {

    }


    public void BossMusicAudio()
    {

    }

    //Stingers

    public void PlayerDeathStingerAudio()
    {

    }

    public void PlayerRespawnStingerAudio()
    {

    }
}
