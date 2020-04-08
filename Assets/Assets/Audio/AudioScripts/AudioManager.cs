using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private PlayerInsanityAudio playerInsanityAudio;
    private PlayerMovementAudio playerMovementAudio;

    private void Awake()
    {

    }
    //Player sounds
    public void PlayerFootStepsAudio(Transform obj)
    {
        playerMovementAudio.AudioPlayerFootsteps(obj.tag); // betyder att den utgår från objektets tag istället för name

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

    public void PlayerInsanityAudio()
    {
        playerInsanityAudio.PlayerInsanity(); // spelar upp ljudet
    }

    //Enemy sounds

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
