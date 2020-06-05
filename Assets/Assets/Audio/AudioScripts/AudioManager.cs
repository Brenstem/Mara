using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class AudioManager : MonoBehaviour
{
    [Header("Music")]
    [EventRef]
    [SerializeField] string combatMusic;

    [EventRef]
    [SerializeField] string deathStingerMusic;

    [Header("Player Audio")]
    [EventRef]
    [SerializeField] string InsanityEventAudio;
    EventInstance InsanityEvent;

    [EventRef]
    [SerializeField] string insanityDecayAudio;                     // not implemented in FMOD

    [EventRef]
    [SerializeField] string insanityRegen;                          // not implemented in FMOD

    [EventRef]
    [SerializeField] string PlayerFootsteps;
    EventInstance PlayerFootstepsAudio;

    [EventRef]
    [SerializeField] string playerSwordSwingAudio;

    [EventRef]
    [SerializeField] string playerHeavyAttackAudio;                 // added. parameter values: 0 = chargeup and loop, 1 = chargeup done, 2 = attack. Parameter name: "Charge attack"
    EventInstance Heavyattack;

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


    [Header("Parry")]
    [EventRef]
    [SerializeField] string parryIndicatorAudio;                    // added

    [EventRef]
    [SerializeField] string parrySuccessAudio;                      // added

    [EventRef]
    [SerializeField] string parryBreakAudio;                        // added

    [EventRef]
    [SerializeField] string unparryableAttackAudio;                 // added


    [Header("Ranged Enemy Audio")]
    [EventRef]
    [SerializeField] string rangedEnemyFireAudio;

    [EventRef]
    [SerializeField] string rangedProjectileHit;

    [EventRef]
    [SerializeField] string rangedEnemyAlertAudio;

    [EventRef]
    [SerializeField] string rangedEnemyMeleeAttackAudio;            // added

    [EventRef]
    [SerializeField] string rangedEnemyMeleeAttackHitAudio;         // added

    [EventRef]
    [SerializeField] string rangedEnemyChantAudio;                  // added


    [Header("Basic Enemy Audio")]
    [EventRef]
    [SerializeField] string floatingEnemyHurtAudio;

    [EventRef]
    [SerializeField] string basicEnemyAttack;

    [EventRef]
    [SerializeField] string basicEnemyHitPlayer;                    // added

    [EventRef]
    [SerializeField] string basicEnemyAlerted;                      // added (again?)

    [EventRef]
    [SerializeField] string basicEnemyDies;                         // added

    [Header("Mylingen")]
    [EventRef]
    [SerializeField] string mylingAlertedAudio;                     // not implemented in FMOD

    [EventRef]
    [SerializeField] string mylingChargeAttackAudio;                // not implemented in FMOD

    [EventRef]
    [SerializeField] string mylingDiesAudio;                        // not implemented in FMOD

    [EventRef]
    [SerializeField] string mylingFootstepAudio;                    // not implemented in FMOD

    [Header("Sister")]
    [EventRef]
    [SerializeField] string sisterAlertedAudio;                     // not implemented in FMOD

    [EventRef]
    [SerializeField] string sisterDissappearAudio;                  // not implemented in FMOD

    [EventRef]
    [SerializeField] string sisteridleAudio;                        // not implemented in FMOD

    [Header("Boss audio")]
    [EventRef]
    [SerializeField] string bossHurtAudio;                          // added

    [EventRef]
    [SerializeField] string bossHeavyHurt;                          // added

    [EventRef]
    [SerializeField] string bossDashAudio;                          // added

    [EventRef]
    [SerializeField] string birdAoeAttackAudio;                     // added
    EventInstance birdAoeAttackEvent;

    [EventRef]
    [SerializeField] string birdAttackAudio;                        // added

    [EventRef]
    [SerializeField] string birdWalkAudio;                          // added

    [EventRef]
    [SerializeField] string birdSpawnEnemyAudio;                    // added
    EventInstance birdSpawnEnemyEvent;


    [EventRef]
    [SerializeField] string birdDrainLaserAudio;                    // added
    EventInstance birdDrainLaserEvent;

    [Header("SFX audio")]
    [EventRef]
    [SerializeField] string checkpointAudio;

    [EventRef]
    [SerializeField] string collectibleAudio;

    [EventRef]
    [SerializeField] string idleCollectibleAudio;                   // not implemented in FMOD

    [EventRef]
    [SerializeField] string grusRasAudio;                           // added

    [EventRef]
    [SerializeField] string murkyWater;                             // not implemented in FMOD

    [EventRef]
    [SerializeField] string murkyWaterDamage;                       // not implemented in FMOD

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

    public void PlayerHeavyAttackAudio(float heavyattackvalue, Transform transform)                  //added (kan dela upp denna ljudeffekten om det är lättare att spela upp / timea separata events)
    {
        Heavyattack = RuntimeManager.CreateInstance(playerHeavyAttackAudio);
        Heavyattack.setParameterByName("Charge attack", heavyattackvalue);      // 0= charge, 1= charge done, 2= attack

        RuntimeManager.AttachInstanceToGameObject(Heavyattack, transform);

        Heavyattack.start();
        if (heavyattackvalue == 2)
        {
            Heavyattack.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }
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
        InsanityEvent = RuntimeManager.CreateInstance(InsanityEventAudio);        // Create a new FMOD::Studio::EventInstance.
        InsanityEvent.setParameterByName("InsanityBar", insanityPercentage);     // string-värdet är parameternamnet och insanitypercentage är float-värdet

        InsanityEvent.start();                                                  // spelar upp ljudet
    }

    public void InsanityDecay()                                  // not implemented in FMOD                                      
    {

    }

    public void InsanityRegen()                                  // not implemented in FMOD
    {

    }

    public void PlayerInsanityAudioUpdate(float insanityPercentage)
    {
        InsanityEvent.setParameterByName("InsanityBar", insanityPercentage);
    }
    public void PlayerHeavyAudioUpdate(float heavyattackvalue)
    {
        Heavyattack.setParameterByName("Charge attack", heavyattackvalue);
    }

    public void PlayerInsanityHeartBeat(Vector3 position)
    {
        RuntimeManager.PlayOneShot(playerHeartBeatAudio, position);
    }
    #endregion

    #region Parry
    public void ParryindicatorAudio(Vector3 position)                       //added
    {
        RuntimeManager.PlayOneShot(parryIndicatorAudio, position);
    }
    public void ParrySuccessAudio(Vector3 position)                        //added
    {
        RuntimeManager.PlayOneShot(parrySuccessAudio, position);
    }

    public void ParryBreakAudio(Vector3 position)                          //added
    {
        RuntimeManager.PlayOneShot(parryBreakAudio, position);
    }

    public void UnparryableAttackAudio(Vector3 position)                   //added
    {
        RuntimeManager.PlayOneShot(unparryableAttackAudio, position);
    }

    #endregion

    #region Basic Enemy Audio


    public void FloatingEnemyHurtAudio(Vector3 position)
    {
        RuntimeManager.PlayOneShot(floatingEnemyHurtAudio, position);
    }

    public void BasicEnemyAttack(Vector3 position)
    {
        RuntimeManager.PlayOneShot(basicEnemyAttack, position);
    }

    public void BasicEnemyHitPlayer(Vector3 position)                               //added               
    {
        RuntimeManager.PlayOneShot(basicEnemyHitPlayer, position);
    }

    public void BasicEnemyAlerted(Vector3 position)                                //added (again?)
    {
        RuntimeManager.PlayOneShot(basicEnemyAlerted, position);
    }

    public void BasicEnemyDies(Vector3 position)
    {
        RuntimeManager.PlayOneShot(basicEnemyDies, position);                       //added
    }
    #endregion

    #region Ranged Enemy Audio
    public void RangedEnemyAlertAudio(Vector3 position)
    {
        RuntimeManager.PlayOneShot(rangedEnemyAlertAudio, position);
    }

    public void RangedEnemyFireAudio(Vector3 position)
    {
        RuntimeManager.PlayOneShot(rangedEnemyFireAudio, position);
    }

    public void RangedProjectileHit(Vector3 position)
    {
        RuntimeManager.PlayOneShot(rangedProjectileHit, position);
    }

    public void RangedEnemyMeleeAttackAudio(Vector3 position)                       //added
    {
        RuntimeManager.PlayOneShot(rangedEnemyMeleeAttackAudio, position);
    }

    public void RangedEnemyMeleeAttackHitAudio(Vector3 position)                   //added
    {
        RuntimeManager.PlayOneShot(rangedEnemyMeleeAttackHitAudio, position);
    }
    public void RangedEnemyChantAudio(Vector3 position)                             //added (might be used for boss enemy spawn if it don't fit
    {
        RuntimeManager.PlayOneShot(rangedEnemyChantAudio, position);
    }

    #endregion

    #region Myling
    public void MylingAlertedAudio(Vector3 position)               // added
    {
        RuntimeManager.PlayOneShot(mylingAlertedAudio, position);
    }

    public void MylingChargeAttackAudio(Vector3 position)          // added
    {
        RuntimeManager.PlayOneShot(mylingChargeAttackAudio, position);
    }

    public void MylingDiesAudio(Vector3 position)                  // added
    {
        RuntimeManager.PlayOneShot(mylingDiesAudio, position);
    }

    public void MylingFootstepAudio(Vector3 position)              // added
    {
        //kan behöva specialösning p.g.a fotsteg
    }
    #endregion

    #region sister
    public void SisterAlertedAudio(Vector3 position)                // not implemented in FMOD
    {
        RuntimeManager.PlayOneShot(sisterAlertedAudio, position);
    }

    public void SisterDissappearAudio(Vector3 position)             // not implemented in FMOD
    {
        RuntimeManager.PlayOneShot(sisterAlertedAudio, position);
    }

    public void SisteridleAudio(Vector3 position)                  // not implemented in FMOD
    {
        RuntimeManager.PlayOneShot(sisteridleAudio, position);
    }

    #endregion

    #region Boss Audio
    public void BossDashAudio(Vector3 position)
    {
        RuntimeManager.PlayOneShot(bossDashAudio, position);
    }

    public void BirdMeleeAttackAudio(Vector3 position)
    {
        RuntimeManager.PlayOneShot(birdAttackAudio, position);
    }

    public void BirdSpawnEnemyAudio(Transform position)
    {
        birdSpawnEnemyEvent = RuntimeManager.CreateInstance(birdSpawnEnemyAudio);
        RuntimeManager.AttachInstanceToGameObject(birdSpawnEnemyEvent, position);
        birdSpawnEnemyEvent.start();
    }
    public void BirdSpawnEnemyAudioEnd()
    {
        birdSpawnEnemyEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    public void BossHurtAudio(Vector3 position)
    {
        RuntimeManager.PlayOneShot(bossHurtAudio, position);
    }
    public void BossChangePhaseAudio(Vector3 position)
    {
        RuntimeManager.PlayOneShot(bossHeavyHurt, position);
    }

    public void BossAOEAttackAudio(Transform position)
    {
        birdAoeAttackEvent = RuntimeManager.CreateInstance(birdAoeAttackAudio);
        RuntimeManager.AttachInstanceToGameObject(birdAoeAttackEvent, position);
        birdAoeAttackEvent.start();
    }
    public void BossAOEAttackAudioEnd()
    {
        birdAoeAttackEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
    
    public void BossWalkAudio(Vector3 position)
    {
        RuntimeManager.PlayOneShot(birdWalkAudio, position);
    }

    public void BossDrainLaserAudio(Transform position)
    {
        birdDrainLaserEvent = RuntimeManager.CreateInstance(birdDrainLaserAudio);
        RuntimeManager.AttachInstanceToGameObject(birdDrainLaserEvent, position);
        birdDrainLaserEvent.start();
    }
    public void BossDrainLaserAudioEnd()
    {
        birdDrainLaserEvent.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
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

    public void IdleCollectibleAudio(Vector3 position)                        // not implemented in FMOD
    {
    }

    public void MurkyWater(Vector3 position)                                 // not implemented in FMOD
    {
        RuntimeManager.PlayOneShot(murkyWater, position);
    }

    public void MurkyWaterDamage(Vector3 position)                           // not implemented in FMOD
    {
        RuntimeManager.PlayOneShot(murkyWaterDamage, position);
    }
    #endregion

    #region Music
    public void MenuMusicAudio()
    {

    }

    public void DeathStingerMusic()
    {
        RuntimeManager.PlayOneShot(deathStingerMusic);
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
