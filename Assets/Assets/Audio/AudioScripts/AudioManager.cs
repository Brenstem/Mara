using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class AudioManager : MonoBehaviour
{
    [EventRef]
    [SerializeField] string InsanityEventAudio;
    EventInstance InsanityEvent;
    [EventRef]
    [SerializeField] string PlayerFootsteps;
    EventInstance PlayerFootstepsAudio;
    [EventRef]
    [SerializeField] string playerSwordSwingAudio;
    [EventRef]
    [SerializeField] string playerDodgeAudio;
    [EventRef]
    [SerializeField] string playerJumpAudio;
    [EventRef]
    [SerializeField] string playerHeartBeatAudio;


    //Player sounds
    public void PlayerFootStepsAudio(string GroundMaterial)
    {
        // playerMovementAudio.AudioPlayerFootsteps(obj.tag); // något liknande kan användas för att jämföra med tags istälelt för strings methinks
        PlayerFootstepsAudio = RuntimeManager.CreateInstance(PlayerFootsteps);
        switch (GroundMaterial)
        {
            case "Gravel":
                PlayerFootstepsAudio.setParameterByName("Surface", 0f);
                break;
            case "Water":
                PlayerFootstepsAudio.setParameterByName("Surface", 1f);
                break;
            case "Wood":
                PlayerFootstepsAudio.setParameterByName("Surface", 2f);
                break;
        }
        PlayerFootstepsAudio.start();
    }

    public void PlayerSwordSwingAudio()
    {
     RuntimeManager.PlayOneShot(playerSwordSwingAudio, transform.position);
    }

    public void PlayerDodgeAudio()
    {
    RuntimeManager.PlayOneShot(playerDodgeAudio, transform.position);

    }

    public void PlayerJumpAudio()
    {
        RuntimeManager.PlayOneShot(playerJumpAudio, transform.position);
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

    public void PlayerInsanityHeartBeat()
    {
        RuntimeManager.PlayOneShot(playerHeartBeatAudio, transform.position);
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

    public void FloatingWorldMusic()
    {

    }

    public void CaveMusic()
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
