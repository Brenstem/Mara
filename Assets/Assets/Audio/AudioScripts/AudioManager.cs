using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class AudioManager : MonoBehaviour
{
    [Header("Player Audio")]
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
    [SerializeField] string playerLandAudio;

    [EventRef]
    [SerializeField] string playerHurtAudio;

    [EventRef]
    [SerializeField] string playerHeartBeatAudio;

    [Header("Enemy Audio")]
    [EventRef]
    [SerializeField] string rangedEnemyFireAudio;

    [EventRef]
    [SerializeField] string rangedEnemyAlertAudio;

    [EventRef]
    [SerializeField] string floatingEnemyHurtAudio;

    [EventRef]
    [SerializeField] string basicEnemyAttack;

    [Header("Boss audio")]
    [EventRef]
    [SerializeField] string bossHurtAudio;

    [EventRef]
    [SerializeField] string bossDashAudio;

    [Header("SFX audio")]
    [EventRef]
    [SerializeField] string checkpointAudio;

    [EventRef]
    [SerializeField] string collectibleAudio;

    [Header("Music")]
    [EventRef]
    [SerializeField] string combatMusic;


    #region Player Audio
    public void PlayerFootStepsAudio(Transform transform, string groundMaterial, Rigidbody rb)
    {
        RuntimeManager.PlayOneShot(PlayerFootsteps, transform.position);

        // playerMovementAudio.AudioPlayerFootsteps(obj.tag); // något liknande kan användas för att jämföra med tags istälelt för strings methinks
        /*PlayerFootstepsAudio = RuntimeManager.CreateInstance(PlayerFootsteps);
        RuntimeManager.AttachInstanceToGameObject(PlayerFootstepsAudio, transform, rb);

        switch (groundMaterial)
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
        PlayerFootstepsAudio.start();*/
    }

    public void PlayerSwordSwingAudio(Vector3 position)
    {
        RuntimeManager.PlayOneShot(playerSwordSwingAudio, position);
    }

    public void PlayerDodgeAudio(Vector3 position)
    {
        RuntimeManager.PlayOneShot(playerDodgeAudio, position);
    }

    public void PlayerJumpAudio(Vector3 position)
    {
        RuntimeManager.PlayOneShot(playerJumpAudio, position);
    }

    public void PlayerLandAudio(Vector3 position)
    {
        RuntimeManager.PlayOneShot(playerLandAudio, position);
    }

    public void PlayerHurtAudio(Vector3 position)
    {
        RuntimeManager.PlayOneShot(playerHurtAudio, position);
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

    public void PlayerInsanityHeartBeat(Vector3 position)
    {
        RuntimeManager.PlayOneShot(playerHeartBeatAudio, position);
    }
    #endregion

    #region Enemy Audio
    public void RangedEnemyFireAudio(Vector3 position)
    {
        RuntimeManager.PlayOneShot(rangedEnemyFireAudio, position);
    }

    public void RangedEnemyAlertAudio(Vector3 position)
    {
        RuntimeManager.PlayOneShot(rangedEnemyAlertAudio, position);
    }

    public void FloatingEnemyHurtAudio(Vector3 position)
    {
        RuntimeManager.PlayOneShot(floatingEnemyHurtAudio, position);
    }

    public void BasicEnemyAttack(Vector3 position)
    {
        RuntimeManager.PlayOneShot(basicEnemyAttack, position);
    }
    #endregion

    #region Boss Audio
    public void BossDash(Vector3 position)
    {
        RuntimeManager.PlayOneShot(bossDashAudio, position);
    }

    public void BossHurt(Vector3 position)
    {
        RuntimeManager.PlayOneShot(bossHurtAudio, position);
    }
    #endregion

    #region SFX
    public void CheckpointAudio(Vector3 position)
    {
        RuntimeManager.PlayOneShot(checkpointAudio, position);
    }

    public void CollectibleAudio(Vector3 position)
    {
        RuntimeManager.PlayOneShot(collectibleAudio, position);
    }

    #endregion

    #region Music
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

    public void CombatMusicParamUpdate(float enemyAmount)
    {
        RuntimeManager.StudioSystem.setParameterByName("CombatNumberOfEnemies", enemyAmount);
    }
    #endregion
}
