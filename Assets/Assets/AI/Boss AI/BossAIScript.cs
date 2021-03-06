﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class BossAIScript : Entity
{
    public GameObject victoryScreen;

    [System.Serializable]
    public struct PhaseOneStats
    {
        public float testP1value1;
        public float testP1value2;
        public float testP1value3;
    }
    [System.Serializable]
    public struct PhaseTwoStats
    {
        public float testP2value1;
        public float testP2value2;
        public float testP2value3;
    }

    public PhaseOneStats phaseOneStats;
    public PhaseTwoStats phaseTwoStats;

    [System.Serializable]
    public struct DesiredDistanceToAngleValues
    {
        public float desiredDistanceOffset;
        public float angle;
        public float speedIncrese;
    }

    [Tooltip("Elementen MÅSTE vara i ordnade utifrån Desired Distance Offset (från störst till minst)")]
    [SerializeField] public DesiredDistanceToAngleValues[] desiredDistanceValues;

    [NonSerialized] public List<float> desiredDistanceOffsetValues = new List<float>();
    [NonSerialized] public List<float> desiredDistanceAngleValues = new List<float>();
    [NonSerialized] public List<float> desiredDistanceSpeedIncreseValues = new List<float>();

    #region Phase State Machine saker
    public StateMachine<BossAIScript> phaseStateMachine;

    public PreBossFightState preBossFightState = new PreBossFightState();
    public BossPhaseOneState bossPhaseOneState = new BossPhaseOneState();
    public BossPhaseTwoState bossPhaseTwoState = new BossPhaseTwoState();
    public BossDeadState bossDeadState = new BossDeadState();
    #endregion

    #region Action State Machine saker
    public StateMachine<BossAIScript> actionStateMachine;

    public BossIdleActionState bossIdleActionState;
    public State<BossAIScript> bossCombatState;
    public BossDashState bossDashState;
    public ChaseToAttackState chaseToAttackState;

    public MeleeAttackOneState meleeAttackOneState;
    public DrainAttackChargeState drainAttackChargeState;
    public DrainAttackActiveState drainAttackActiveState;
    public AOEAttackState aoeAttackState;
    public SpawnEnemiesAbilityState spawnEnemiesAbilityState;
    #endregion

    [SerializeField] public GameObject _fill;

    [SerializeField] public HitboxGroup meleeAttackHitboxGroup;
    [SerializeField] public HitboxGroup drainAttackHitboxGroup;

    #region Attack Speed Variables
    [Header("Attack Speed Variables")]
    [SerializeField] public float minAttackSpeed = 5f;
    [SerializeField] public float attackSpeedIncreaseMax = 5f;
    [SerializeField] public float minAttackCooldown = 1f;
    #endregion

    #region Drain Variables
    [Header("Drain Variables")]

    [SerializeField] public float drainAttackCooldown = 6f;
    [SerializeField] public float drainRange = 6f;
    [SerializeField] public float drainChargeTime = 7f;
    [SerializeField] public float drainAttackTime = 8f;
    #endregion

    #region Dash Variables
    [Header("Dash Variables")]

    [SerializeField] [Range(0, 10)] public float dashChansePerFrame = 0.1f;
    [SerializeField] [Range(0, 100)] public float dashAttackChanse = 20f;

    [SerializeField] public float dashSpeed = 20f;
    [SerializeField] public float dashAcceleration = 2000f;

    //[SerializeField] public float dashDistance = 5f;

    [SerializeField] public float dashAttackDistanceMin = 5f;
    [SerializeField] public float dashAttackDistanceMax = 5f;
    [SerializeField] public float dashStoppingDistance = 1f;

    [SerializeField] public float dashLagDurration = 0.5f;

    [NonSerialized] public float dashDistance = 5f;
    //[SerializeField] [Range(1, 180)] public float maxAngleDashAttackToPlayer = 90f;
    #endregion

    #region Chasing Variables
    [Header("Chasing Variables")]

    [SerializeField] public float chasingSpeed = 5f;
    [SerializeField] public float chasingAcceleration = 20f;
    #endregion

    #region AOE Variables
    [Header("AOE Variables")]

    [SerializeField] public GameObject[] murkyWaterPrefabs;

    [SerializeField] public Vector3 baseSpawnPosOffset;

    [SerializeField] public float aoeAttackCooldown;
    //temp variabel för design
    [SerializeField] public bool useSpiral;

    [SerializeField] public int spiralLayers;
    [SerializeField] public int poolsPerLayer;
    //[SerializeField] public int spiralArms;
    [SerializeField] public float spiralIntensity;
    [Tooltip("Om denna är 0 försvinner de aldrig")]
    [SerializeField] public float poolTimeToLive;
    [SerializeField] public float spawnTimeBetweenPools;
    [SerializeField] public float spaceBetweenLayers;
    [SerializeField] public float firstLayerOffset;
    #endregion

    #region Spawn Enemy Variables
    [Header("Spawn Enemy Variables")]

    [SerializeField] public float spawnEnemyAbilityCooldown;

    [SerializeField] public GameObject enemyToSpawnPrefab;
    [SerializeField] public float spawnedEnemyAggroRange;
    [SerializeField] public float spawnedEnemyUnaggroRange;
    [SerializeField] public float spawnEnemyAbilitySpawnTime;
    [SerializeField] public float spawnEnemyAbilityTotalCastTime;
    [SerializeField] public Transform spawnPos;

    //kan ändra till en lista om det ska kunna finnas fler fiender
    [NonSerialized] public GameObject spawnedEnemy;

    //old?
    [SerializeField] public GameObject[] enemySpawnList;

    #endregion

    #region Misc Variables
    [Header("Misc Variables")]

    [SerializeField] public LayerMask targetLayers;
    [SerializeField] public LayerMask dashCollisionLayers;

    [SerializeField] [Range(0, 1)] public float phaseTwoTransitionHP = 0.5f;

    [SerializeField] public float aggroRange = 10f;
    [SerializeField] public float defaultTurnSpeed = 5f;
    [SerializeField] public float drainActiveTurnSpeed = 0.1f;

    [SerializeField] public float meleeRange = 6f;

    [SerializeField] public float desiredDistanceToPlayer = 5f;

    [SerializeField] public GameObject placeholoderDranBeam;
    #endregion

    #region NonSerialized Variables
    [NonSerialized] public Vector3 idlePos;

    [NonSerialized] public float defaultSpeed;
    [NonSerialized] public float turnSpeed;
    //[NonSerialized] public float dashDistance;
    [NonSerialized] public bool dashAttack;

    [NonSerialized] public Vector3 currentMovementDirection = new Vector3(0f, 0f, 1f);
    [NonSerialized] public Vector3 targetMovementDirection = new Vector3(0f, 0f, 1f);
    [NonSerialized] public Vector3 dashCheckOffsetVector;
    [NonSerialized] public Vector3 dashCheckBoxSize;

    [NonSerialized] public Animator bossAnimator;
    [NonSerialized] public NavMeshAgent agent;
    [NonSerialized] public GameObject player;


    [NonSerialized] public bool animationEnded;
    [NonSerialized] public bool facePlayerBool = true;

    [NonSerialized] public AudioManager audioManager;
    #endregion

    protected new void Awake()
    {
        base.Awake();

        phaseStateMachine = new StateMachine<BossAIScript>(this);



        actionStateMachine = new StateMachine<BossAIScript>(this);

        bossIdleActionState = new BossIdleActionState();
        bossDashState = new BossDashState(dashSpeed, dashDistance, dashLagDurration, dashAcceleration);
        chaseToAttackState = new ChaseToAttackState();

        meleeAttackOneState = new MeleeAttackOneState();
        drainAttackChargeState = new DrainAttackChargeState(drainChargeTime);
        drainAttackActiveState = new DrainAttackActiveState(drainAttackTime);
        aoeAttackState = new AOEAttackState(murkyWaterPrefabs, spiralLayers, poolsPerLayer, spiralIntensity, poolTimeToLive, spawnTimeBetweenPools, spaceBetweenLayers, firstLayerOffset, baseSpawnPosOffset);
        spawnEnemiesAbilityState = new SpawnEnemiesAbilityState(enemyToSpawnPrefab, spawnedEnemyAggroRange, spawnedEnemyUnaggroRange, spawnEnemyAbilityTotalCastTime, spawnEnemyAbilitySpawnTime, spawnPos);

        player = GlobalState.state.Player.gameObject;

        bossAnimator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        idlePos = transform.position;
        turnSpeed = defaultTurnSpeed;

        dashCheckOffsetVector = new Vector3(0f, 1f, dashDistance / 2);
        dashCheckBoxSize = new Vector3(0.75f, 0.75f, dashDistance / 2);
        defaultSpeed = agent.speed;

        audioManager = GlobalState.state.AudioManager;
    }

    void Start()
    {
        for (int i = 0; i < desiredDistanceValues.Length; i++)
        {
            desiredDistanceOffsetValues.Add(desiredDistanceValues[i].desiredDistanceOffset);
            desiredDistanceAngleValues.Add(desiredDistanceValues[i].angle);
            desiredDistanceSpeedIncreseValues.Add(desiredDistanceValues[i].speedIncrese);
        }

        agent.updateRotation = false;
        actionStateMachine.ChangeState(bossIdleActionState);
        phaseStateMachine.ChangeState(preBossFightState);

        _fill.SetActive(false);
    }

    void Update()
    {
        phaseStateMachine.Update();
        actionStateMachine.Update();
        //print(actionStateMachine.currentState);
        //print(movementDirection);
        //agent.SetDestination(Vector3.forward);
    }

    public void FacePlayer()
    {
        Vector3 lookDirection = (player.transform.position - this.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(lookDirection.x, 0, lookDirection.z));
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, lookRotation, Time.deltaTime * turnSpeed);
    }

    public override void TakeDamage(HitboxValues hitbox, Entity attacker)
    {
        health.Damage(hitbox);
        audioManager.BossHurtAudio(transform.position);
        //spela hurtljud här
    }

    public override void Parried()
    {
        Debug.LogWarning("Parried implementation missing", this);
    }

    public bool CheckDashPath(Vector3 dashCheckVector, float dashCheckDistance)
    {
        float dashCheckAngle = Vector3.SignedAngle(transform.forward, dashCheckVector.normalized, transform.up);

        dashCheckOffsetVector = new Vector3(0f, 1f, dashCheckDistance / 2);

        dashCheckOffsetVector = Quaternion.AngleAxis(dashCheckAngle, Vector3.up) * dashCheckOffsetVector;

        dashCheckBoxSize = new Vector3(0.75f, 0.75f, dashCheckDistance / 2);

        return Physics.OverlapBox(transform.TransformPoint(dashCheckOffsetVector), dashCheckBoxSize, Quaternion.LookRotation(dashCheckVector.normalized), dashCollisionLayers).Length <= 0;
    }

    //flytta allt detta till aoe state? flyttat allt
    #region Murky Water Testing (AOE) (old)
    ////flyttad och funkar
    //public System.Collections.IEnumerator MurkyWaterSpiralAbility(int layers, int poolsPerLayer, float spiralIntensity, float timeToLive)
    //{
    //    Vector3 spawnPos = Vector3.forward;
    //    //float rotationAmount
    //    for (int i = 0; i < layers; i++)
    //    {
    //        for (int j = 0; j < poolsPerLayer; j++)
    //        {
    //            spawnPos = Quaternion.AngleAxis(360f / poolsPerLayer + spiralIntensity, Vector3.up) * spawnPos;
    //            //print(testVec);
    //            SpawnMurkyWater(spawnPos * (i + 1), timeToLive);
    //            yield return new WaitForSeconds(0.03f);
    //            //yield return null;
    //        }
    //        //yield return new WaitForSeconds(0.3f);
    //    }
    //    AOEAbilityOver = true;
    //    yield return null;
    //}

    //public System.Collections.IEnumerator MurkyWaterCircleAbility(int layers, int poolsPerLayer, float timeToLive)
    //{
    //    print("we in here");
    //    Vector3 spawnPos;
    //    float poolAmount = 0;
    //    for (int i = 0; i < layers; i++)
    //    {
    //        spawnPos = Vector3.forward;
    //        poolAmount += poolsPerLayer;
    //        for (int j = 0; j < poolAmount; j++)
    //        {
    //            spawnPos = Quaternion.AngleAxis(360f / poolAmount, Vector3.up) * spawnPos;
    //            //print(testVec);
    //            SpawnMurkyWater(spawnPos * (i + 1), timeToLive);
    //            //yield return new WaitForSeconds(0.03f);
    //        }
    //        yield return new WaitForSeconds(0.3f);
    //    }
    //    AOEAbilityOver = true;
    //    yield return null;
    //}

    //public System.Collections.IEnumerator MurkyWaterPolygonAbility(int layers, int sides, int poolsPerSide, float timeToLive)
    //{
    //    Vector3 spawnPos;
    //    Vector3 currentCornerPos;
    //    Vector3 nextCornerPos;
    //    Vector3 cornerToCorner;

    //    float poolAmount = 0;
    //    for (int i = 0; i < layers; i++)
    //    {
    //        poolAmount += poolsPerSide;

    //        currentCornerPos = Vector3.forward * (i * 2 + 2);
    //        nextCornerPos = Quaternion.AngleAxis(360f / sides, Vector3.up) * currentCornerPos;

    //        for (int k = 0; k < sides; k++)
    //        {
    //            cornerToCorner = nextCornerPos - currentCornerPos;
    //            spawnPos = currentCornerPos;
    //            for (int j = 0; j < poolAmount; j++)
    //            {
    //                SpawnMurkyWater(spawnPos, timeToLive);
    //                yield return new WaitForSeconds(0.03f);
    //                spawnPos += cornerToCorner.normalized * (cornerToCorner.magnitude / poolAmount);
    //            }
    //            currentCornerPos = nextCornerPos;
    //            nextCornerPos = Quaternion.AngleAxis(360f / sides, Vector3.up) * nextCornerPos;
    //        }
    //    }
    //    AOEAbilityOver = true;
    //    yield return null;
    //}

    ////flyttad och funkar
    ////hur fan ska det funka med olika Y nivå? får typ raycasta upp och ner när den spawnas för att hitta vart marken är och sen flytta den dit och rotera den baserat på normalen eller något, låter jobbigt :(
    //public void SpawnMurkyWater(Vector3 spawnPositionOffset, float timeToLive = 0f)
    //{
    //    GameObject murkyWater = (GameObject)Instantiate(murkyWaterPrefab, transform.TransformPoint(spawnPositionOffset), Quaternion.identity);
    //    if (timeToLive > 0.01f)
    //    {
    //        murkyWater.GetComponentInChildren<MurkyWaterScript>().timeToLive = timeToLive;

    //        //print("spawned murky water for " + timeToLive);
    //    }
    //    else
    //    {
    //        //print("spawned murky water for ever >:) " + timeToLive);
    //    }
    //}

    ////flyttad och funkar(?)
    ////hur fan ska det funka med olika Y nivå? får typ raycasta upp och ner när den spawnas för att hitta vart marken är och sen flytta den dit och rotera den baserat på normalen eller något, låter jobbigt :(
    //public void SpawnMurkyWater(float timeToLive = 0f)
    //{
    //    GameObject murkyWater = (GameObject)Instantiate(murkyWaterPrefab, transform.position, Quaternion.identity);
    //    if (timeToLive > 0.1f)
    //    {
    //        murkyWater.GetComponentInChildren<MurkyWaterScript>().timeToLive = timeToLive;

    //        //print("spawned murky water for " + timeToLive);
    //    }
    //    else
    //    {
    //        //print("spawned murky water for ever >:) " + timeToLive);
    //    }
    //}
    #endregion

    //flytta allt detta till spawn enemy state? flyttat allt
    #region Spawn Enemy Ability (old)
    ////vet inte om denna ska användas
    //public System.Collections.IEnumerator SpawnEnemyAbility(float distanceFromBoss, GameObject[] enemiesToSpawn)
    //{
    //    Vector3 spawnPos;
    //    spawnPos = Vector3.forward;

    //    for (int i = 0; i < enemiesToSpawn.Length; i++)
    //    {
    //        spawnPos = Quaternion.AngleAxis(360f / enemiesToSpawn.Length, Vector3.up) * spawnPos;
    //        SpawnEnemy(spawnPos * distanceFromBoss, enemiesToSpawn[i]);
    //        yield return new WaitForSeconds(0.5f);
    //    }
    //    //lägg till så fienderna aggroar här
    //    SpawnAbilityOver = true;
    //    yield return null;
    //}

    ////flyttad och funkar
    //public void SpawnEnemy(Vector3 spawnPositionOffset, GameObject enemy)
    //{
    //    GameObject spawnedEnemy = (GameObject)Instantiate(enemy, transform.TransformPoint(spawnPositionOffset), Quaternion.identity);
    //    spawnedEnemy.GetComponent<BaseAIMovementController>()._aggroRange = 30f;
    //    spawnedEnemy.GetComponent<BaseAIMovementController>()._unaggroRange = 50f;
    //}
    #endregion
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRange);

        Gizmos.color = Color.blue;

        Vector3 dashCheckOffsetVectorGizmo = new Vector3(0f, 1f, dashDistance / 2);
        Vector3 dashCheckBoxSizeGizmo = new Vector3(1.5f, 1.5f, dashDistance);

        float dashCheckAngleGizmo = Vector3.SignedAngle(transform.forward, currentMovementDirection, transform.up);

        dashCheckOffsetVectorGizmo = Quaternion.AngleAxis(dashCheckAngleGizmo, Vector3.up) * dashCheckOffsetVectorGizmo;

        Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(dashCheckOffsetVectorGizmo), Quaternion.LookRotation(currentMovementDirection.normalized), transform.localScale);
        Gizmos.DrawWireCube(Vector3.zero, dashCheckBoxSizeGizmo);
    }

    #region Action Animation events

    //Generella
    public void EndAnimation()
    {
        animationEnded = true;
    }

    public void DontFacePlayer()
    {
        facePlayerBool = false;
    }
    //P1

    public void EnableDrainBeam()
    {
        placeholoderDranBeam.SetActive(true);
        audioManager.BossDrainLaserAudio(transform);
    }

    public void DisableDrainBeam()
    {
        placeholoderDranBeam.SetActive(false);
        audioManager.BossDrainLaserAudioEnd();
    }

    public override void KillThis()
    {
        phaseStateMachine.ChangeState(bossDeadState);
    }

    //old
    //public void FlipDrainHitboxActivation()
    //{
    //    drainHitboxActive = !drainHitboxActive;
    //}
    //public void FlipMeleeHitboxActivation()
    //{
    //    meleeHitboxActive = !meleeHitboxActive;
    //}


    #endregion

    #region Audio Animation events

    public void PlayBossMeleeAudio()
    {
        audioManager.BirdMeleeAttackAudio(transform.position);
    }
    public void PlayBossWalkAudio()
    {
        audioManager.BossWalkAudio(transform.position);
    }


    #endregion
}

//////////////////
//PHASE 0 STATES//
//////////////////
public class PreBossFightState : State<BossAIScript>
{
    private RaycastHit _hit;

    public override void EnterState(BossAIScript owner)
    {
        owner.transform.position = owner.idlePos;
        //set hp till maxhp här?
        //owner.bossAnimator.SetBool("idleBool", true);
    }

    public override void ExitState(BossAIScript owner)
    {
        //kanske köra någon funktion som stänger en dörr eller något så man inte kan springa iväg
        //owner.bossAnimator.SetBool("idleBool", false);
        owner._fill.SetActive(true);
    }

    public override void UpdateState(BossAIScript owner)
    {
        //aggro detection, börjar boss fighten
        if (owner.aggroRange > Vector3.Distance(owner.player.transform.position, owner.transform.position))
        {
            if (Physics.Raycast(owner.transform.position + new Vector3(0, 1, 0), (owner.player.transform.position - owner.transform.position).normalized, out _hit, owner.aggroRange, owner.targetLayers))
            {
                if (_hit.transform == owner.player.transform)
                {
                    //owner.actionStateMachine.ChangeState(owner.bossCombatState);
                    owner.phaseStateMachine.ChangeState(owner.bossPhaseOneState);
                }
            }
        }
    }
}

//vill nog lägga till något transition state där någon aggro/bossfighten börjar nu annimation spelas

#region Action States
public class BossIdleActionState : State<BossAIScript>
{
    public override void EnterState(BossAIScript owner)
    {
    }

    public override void ExitState(BossAIScript owner)
    {
    }

    public override void UpdateState(BossAIScript owner)
    {
    }
}


public class BossDashState : State<BossAIScript>
{
    private float _dashSpeed;
    private float _oldSpeed;
    private float _dashDistance;
    private float _dashDurration;
    private float _lagDurration;
    private float _dashAcceleration;
    private float _oldAcceleration;

    private Timer _dashTimer;
    private Timer _lagTimer;

    private Vector3 _dashDirection;
    private Vector3 _dashDestination;


    public BossDashState(float speed, float distance, float lagDurration, float acceleration)
    {
        _dashSpeed = speed;
        _dashDistance = distance;
        _lagDurration = lagDurration;
        _dashAcceleration = acceleration;
    }


    public override void EnterState(BossAIScript owner)
    {
        owner.audioManager.BossDashAudio(owner.transform.position);


        //animation stuff
        if (owner.dashAttack)
        {
            owner.bossAnimator.SetTrigger("dashForwardTrigger");
        }

        _oldSpeed = owner.agent.speed;
        _oldAcceleration = owner.agent.acceleration;

        owner.agent.speed = _dashSpeed;
        owner.agent.acceleration = _dashAcceleration;

        _dashDistance = owner.dashDistance;

        _dashDurration = (_dashDistance - owner.agent.stoppingDistance) / _dashSpeed;
        Debug.Log(_dashDurration);
        //Debug.Log("zoom for, " + _dashDurration + " MPH, " + _dashSpeed);
        //Debug.Log(_dashDistance + " " + owner.bossPhaseOneParentScript.agent.stoppingDistance + " " + _dashSpeed);
        _dashTimer = new Timer(_dashDurration);
        _lagTimer = new Timer(_lagDurration);

        _dashDirection = owner.currentMovementDirection.normalized;
        _dashDestination = owner.transform.position + _dashDirection * _dashDistance;

        owner.agent.SetDestination(_dashDestination);
    }

    public override void ExitState(BossAIScript owner)
    {
        owner.agent.speed = _oldSpeed;
        owner.agent.acceleration = _oldAcceleration;
        owner.agent.SetDestination(owner.transform.position);
        //Debug.Log("hej då zoom");
    }

    public override void UpdateState(BossAIScript owner)
    {
        _dashTimer.Time += Time.deltaTime;

        if (_dashTimer.Expired)
        {
            _lagTimer.Time += Time.deltaTime;
            
            if (owner.dashAttack)
            {
                owner.dashAttack = false;
                owner.actionStateMachine.ChangeState(owner.meleeAttackOneState);
            }
            else if (_lagTimer.Expired)
            {
                owner.actionStateMachine.ChangeState(owner.bossCombatState);
            }
        }
    }
}

//public class BossDashAttackState : State<BossAIScript>
//{
//    private float _durration;
//    private Timer _timer;


//    private float _dashSpeed;
//    private float _oldSpeed;
//    private float _dashAcceleration;
//    private float _oldAcceleration;


//    private Vector3 _dashDirection;
//    private Vector3 _dashDestination;
//    public BossDashAttackState(float dashSpeed, float dashAcceleration)
//    {
//        _dashSpeed = dashSpeed;
//        _dashAcceleration = dashAcceleration;
//    }


//    public override void EnterState(BossAIScript owner)
//    {
//        _oldSpeed = owner.agent.speed;
//        _oldAcceleration = owner.agent.acceleration;

//        owner.agent.speed = _dashSpeed;
//        owner.agent.acceleration = _dashAcceleration;
//    }

//    public override void ExitState(BossAIScript owner)
//    {
//        owner.agent.speed = _oldSpeed;
//        owner.agent.acceleration = _oldAcceleration;
//        owner.agent.SetDestination(owner.transform.position);
//    }

//    public override void UpdateState(BossAIScript owner)
//    {

//        if (true)
//        {
//            owner.actionStateMachine.ChangeState(owner.meleeAttackOneState);
//        }
//    }
//}

public class MeleeAttackOneState : State<BossAIScript>
{
    public override void EnterState(BossAIScript owner)
    {
        owner.bossAnimator.SetTrigger("melee1Trigger");
        owner.meleeAttackHitboxGroup.enabled = true;

    }

    public override void ExitState(BossAIScript owner)
    {
        owner.meleeAttackHitboxGroup.enabled = false;
        owner.animationEnded = false;
        owner.facePlayerBool = true;
    }

    public override void UpdateState(BossAIScript owner)
    {
        if (owner.animationEnded)
        {
            owner.actionStateMachine.ChangeState(owner.bossCombatState);
        }
        else if (owner.facePlayerBool)
        {
            //spela attackljud här
            owner.FacePlayer();
        }
    }
}
public class ChaseToAttackState : State<BossAIScript>
{
    private Vector3 _playerPos;
    private float _distanceToPlayer;
    private float _oldSpeed;
    private float _oldAcceleration;


    public override void EnterState(BossAIScript owner)
    {
        _oldSpeed = owner.agent.speed;
        _oldAcceleration = owner.agent.acceleration;

        owner.agent.speed = owner.chasingSpeed;
        owner.agent.acceleration = owner.chasingAcceleration;
        owner.bossAnimator.SetTrigger("runningTrigger");
    }

    public override void ExitState(BossAIScript owner)
    {
        owner.agent.speed = _oldSpeed;
        owner.agent.acceleration = _oldAcceleration;

        owner.agent.SetDestination(owner.transform.position);
    }

    public override void UpdateState(BossAIScript owner)
    {
        owner.FacePlayer();

        _playerPos = owner.player.transform.position;
        _distanceToPlayer = Vector3.Distance(owner.transform.position, _playerPos);

        if (_distanceToPlayer < owner.meleeRange)
        {
            owner.actionStateMachine.ChangeState(owner.meleeAttackOneState);
        }
        else
        {
            owner.agent.SetDestination(_playerPos);
        }
    }
}
public class DrainAttackChargeState : State<BossAIScript>
{
    private float _chargeTime;
    private Timer _timer;

    public DrainAttackChargeState(float chargeTime)
    {
        _chargeTime = chargeTime;
    }

    public override void EnterState(BossAIScript owner)
    {
        owner.bossAnimator.SetTrigger("drainStartTrigger");
        _timer = new Timer(_chargeTime);
    }

    public override void ExitState(BossAIScript owner)
    {
    }

    public override void UpdateState(BossAIScript owner)
    {
        _timer.Time += Time.deltaTime;

        owner.FacePlayer();

        if (_timer.Expired)
        {
            owner.actionStateMachine.ChangeState(owner.drainAttackActiveState);
        }
    }
}
public class DrainAttackActiveState : State<BossAIScript>
{
    private float _durration;
    private Timer _timer;

    public DrainAttackActiveState(float durration)
    {
        _durration = durration;
    }

    public override void EnterState(BossAIScript owner)
    {
        owner.bossAnimator.SetBool("drainActiveBool", true);
        owner.drainAttackHitboxGroup.enabled = true;
        owner.turnSpeed = owner.drainActiveTurnSpeed;
        _timer = new Timer(_durration);
    }

    public override void ExitState(BossAIScript owner)
    {
        owner.drainAttackHitboxGroup.enabled = false;
        owner.turnSpeed = owner.defaultTurnSpeed;
        owner.animationEnded = false;
        owner.placeholoderDranBeam.SetActive(false);
        //har den här ifall man går ut pga att man byter fas
        owner.bossAnimator.SetBool("drainActiveBool", false);
    }

    public override void UpdateState(BossAIScript owner)
    {
        _timer.Time += Time.deltaTime;

        owner.FacePlayer();

        if (_timer.Expired)
        {
            owner.bossAnimator.SetBool("drainActiveBool", false);
        }

        if (owner.animationEnded)
        {
            owner.actionStateMachine.ChangeState(owner.bossCombatState);
        }
    }
}
public class AOEAttackState : State<BossAIScript>
{
    private GameObject[] _murkyWaterArray;
    private int _layers;
    private int _poolsPerLayer;
    private float _spiralIntensity;
    private float _timeToLive;
    private float _spawnTimeBetweenPools;
    private float _spaceBetweenLayers;
    private float _firstLayerOffset;
    private Vector3 _baseSpawnPosOffset;


    //construktor där man tar in murkyWater objektet?
    public AOEAttackState(GameObject[] murkyWaterPrefabs, int layers, int poolsPerLayer, float spiralIntensity, float timeToLive, float spawnTimeBetweenPools, float spaceBetweenLayers, float firstLayerOffset, Vector3 baseSpawnPosOffset)
    {
        _murkyWaterArray = murkyWaterPrefabs;
        _layers = layers;
        _poolsPerLayer = poolsPerLayer;
        _spiralIntensity = spiralIntensity;
        _timeToLive = timeToLive;
        _spawnTimeBetweenPools = spawnTimeBetweenPools;
        _spaceBetweenLayers = spaceBetweenLayers;
        _firstLayerOffset = firstLayerOffset;
        _baseSpawnPosOffset = baseSpawnPosOffset;
    }

    public override void EnterState(BossAIScript owner)
    {
        owner.agent.SetDestination(owner.transform.position);

        owner.audioManager.BossAOEAttackAudio(owner.transform);

        owner.bossAnimator.SetBool("spawnSpellBool", true);

        //SpawnMurkyWater(owner, Vector3.forward, 5f);

        if (owner.useSpiral)
        {
            owner.StartCoroutine(MurkyWaterSpiralAbility(owner));
        }
        else
        {
            owner.StartCoroutine(MurkyWaterCircleAbility(owner));
        }

        //owner.StartCoroutine(MurkyWaterSpiralAbility(owner));

        //owner.StartCoroutine(MurkyWaterCircleAbility(owner, 10, 8, 1.5f, 0.03f, 1f, 1f));
        //owner.StartCoroutine(MurkyWaterPolygonAbility(owner, 5, 5, 2, 0f, 0.03f, 1.5f, 1f));


        //owner.StartCoroutine(MurkyWaterSpiralAbility(owner, 10, 8, 1.5f, 2.5f, 0.03f, 1f));

        //owner.StartCoroutine(owner.MurkyWaterSpiralAbility(10, 8, 1.5f, 0f));

        //owner.StartCoroutine(owner.MurkyWaterCircleAbility(10, 6, 0f));
        //owner.StartCoroutine(owner.MurkyWaterPolygonAbility(5, 9, 2, 0f));
    }

    public override void ExitState(BossAIScript owner)
    {
        owner.bossAnimator.SetBool("spawnSpellBool", false);
        owner.audioManager.BossAOEAttackAudioEnd();
    }

    public override void UpdateState(BossAIScript owner)
    {
    }

    public System.Collections.IEnumerator MurkyWaterSpiralAbility(BossAIScript owner)
    {
        Vector3 spawnPos = Vector3.forward;

        for (int i = 0; i < _layers; i++)
        {
            for (int j = 0; j < _poolsPerLayer; j++)
            {
                spawnPos = Quaternion.AngleAxis(360f / _poolsPerLayer + _spiralIntensity, Vector3.up) * spawnPos;
                SpawnMurkyWater(owner, spawnPos * (i * _spaceBetweenLayers + _firstLayerOffset), _timeToLive);
                yield return new WaitForSeconds(_spawnTimeBetweenPools);
            }
        }
        owner.actionStateMachine.ChangeState(owner.bossCombatState);
        yield return null;
    }

    public System.Collections.IEnumerator MurkyWaterCircleAbility(BossAIScript owner)
    {
        Vector3 spawnPos;
        float poolAmount = 0;
        //poolAmount += poolsPerLayer;
        for (int i = 0; i < _layers; i++)
        {
            spawnPos = Vector3.forward;
            poolAmount += _poolsPerLayer;
            for (int j = 0; j < poolAmount; j++)
            {
                spawnPos = Quaternion.AngleAxis(360f / poolAmount, Vector3.up) * spawnPos;
                SpawnMurkyWater(owner, spawnPos * (i * _spaceBetweenLayers + _firstLayerOffset), _timeToLive);
                yield return new WaitForSeconds(_spawnTimeBetweenPools);
            }
            //yield return new WaitForSeconds(_spawnTimeBetweenPools);
        }
        owner.actionStateMachine.ChangeState(owner.bossCombatState);
        yield return null;
    }

    public System.Collections.IEnumerator MurkyWaterPolygonAbility(BossAIScript owner, int layers, int sides, int poolsPerSide, float timeToLive, float spawnTimeBetweenPools, float spaceBetweenLayers, float firstLayerOffset)
    {
        Vector3 spawnPos;
        Vector3 currentCornerPos;
        Vector3 nextCornerPos;
        Vector3 cornerToCorner;

        float poolAmount = 0;
        for (int i = 0; i < layers; i++)
        {
            poolAmount += poolsPerSide;

            currentCornerPos = Vector3.forward * (i * spaceBetweenLayers + firstLayerOffset);
            nextCornerPos = Quaternion.AngleAxis(360f / sides, Vector3.up) * currentCornerPos;

            for (int k = 0; k < sides; k++)
            {
                cornerToCorner = nextCornerPos - currentCornerPos;
                spawnPos = currentCornerPos;
                for (int j = 0; j < poolAmount; j++)
                {
                    SpawnMurkyWater(owner, spawnPos, timeToLive);
                    yield return new WaitForSeconds(spawnTimeBetweenPools);
                    spawnPos += cornerToCorner.normalized * (cornerToCorner.magnitude / poolAmount);
                }
                currentCornerPos = nextCornerPos;
                nextCornerPos = Quaternion.AngleAxis(360f / sides, Vector3.up) * nextCornerPos;
            }
        }
        owner.actionStateMachine.ChangeState(owner.bossCombatState);
        yield return null;
    }

    //kanske vill lägga in en funktion som spawnar dem på en sett pos
    public void SpawnMurkyWater(BossAIScript owner, Vector3 spawnPositionOffset, float timeToLive = 0f)
    {
        //GameObject murkyWater = UnityEngine.Object.Instantiate(owner.murkyWaterPrefab, owner.transform.TransformPoint(spawnPositionOffset), Quaternion.identity);
        GameObject murkyWater = UnityEngine.Object.Instantiate(_murkyWaterArray[UnityEngine.Random.Range(0, _murkyWaterArray.Length)], owner.transform.TransformPoint(spawnPositionOffset + _baseSpawnPosOffset), Quaternion.identity);
        if (timeToLive > 0.01f)
        {
            murkyWater.GetComponentInChildren<MurkyWaterScript>().timeToLive = timeToLive;
        }
    }

    //vet inte om denna behövs, tror inte den ska användas
    public void SpawnMurkyWater(BossAIScript owner, float timeToLive = 0f)
    {
        GameObject murkyWater = UnityEngine.Object.Instantiate(_murkyWaterArray[UnityEngine.Random.Range(0, _murkyWaterArray.Length)], owner.transform.position, Quaternion.identity);
        if (timeToLive > 0.1f)
        {
            murkyWater.GetComponentInChildren<MurkyWaterScript>().timeToLive = timeToLive;
        }
    }
}
//vet inte hur den ska funka men så här funkar den nu 
public class SpawnEnemiesAbilityState : State<BossAIScript>
{
    private Timer _castTimer;
    private GameObject _enemy;
    private float _newAggroRange;
    private float _newUnaggroRange;
    private float _abilityTotalCastTime;
    private float _abilitySpawnTime;
    private Transform _spawnPos;

    public SpawnEnemiesAbilityState(GameObject enemy, float newAggroRange, float newUnaggroRange, float abilityTotalCastTime, float abilitySpawnTime, Transform spawnPos)
    {
        _enemy = enemy;
        _newAggroRange = newAggroRange;
        _newUnaggroRange = newUnaggroRange;
        _abilityTotalCastTime = abilityTotalCastTime;
        _spawnPos = spawnPos;
        _abilitySpawnTime = abilitySpawnTime;
    }

    public override void EnterState(BossAIScript owner)
    {
        owner.agent.SetDestination(owner.transform.position);
        _castTimer = new Timer(_abilityTotalCastTime);

        owner.bossAnimator.SetBool("spawnSpellBool", true);

        owner.StartCoroutine(SpawnEnemyAfterTime(owner, _abilitySpawnTime));

        owner.audioManager.BirdSpawnEnemyAudio(owner.transform);


        //SpawnEnemy(owner, Vector3.forward);

        //owner.StartCoroutine(owner.SpawnEnemyAbility(3f, owner.enemySpawnList));
    }

    public override void ExitState(BossAIScript owner)
    {
        //owner.SpawnAbilityOver = false;
        owner.bossAnimator.SetBool("spawnSpellBool", false);
        owner.audioManager.BirdSpawnEnemyAudioEnd();
    }

    public override void UpdateState(BossAIScript owner)
    {
        _castTimer.Time += Time.deltaTime;

        if (_castTimer.Expired)
        {
            owner.actionStateMachine.ChangeState(owner.bossCombatState);
        }
    }

    //vet inte hur den ska funka men så här funkar den nu 
    //spawna baserat på bossen och en offset
    public void SpawnEnemy(BossAIScript owner, Vector3 spawnPositionOffset)
    {
        owner.spawnedEnemy = UnityEngine.Object.Instantiate(_enemy, owner.transform.TransformPoint(spawnPositionOffset), Quaternion.identity);
        owner.spawnedEnemy.GetComponent<BaseAIMovementController>()._aggroRange = _newAggroRange;
        owner.spawnedEnemy.GetComponent<BaseAIMovementController>()._unaggroRange = _newUnaggroRange;
    }

    //spawna på pos
    public void SpawnEnemy(BossAIScript owner)
    {
        owner.spawnedEnemy = UnityEngine.Object.Instantiate(_enemy, _spawnPos);
        //GameObject spawnedEnemy = UnityEngine.Object.Instantiate(_enemy, _spawnPos, Quaternion.identity);
        owner.spawnedEnemy.GetComponent<BaseAIMovementController>()._aggroRange = _newAggroRange;
        owner.spawnedEnemy.GetComponent<BaseAIMovementController>()._unaggroRange = _newUnaggroRange;
    }

    IEnumerator SpawnEnemyAfterTime(BossAIScript owner, float time)
    {
        yield return new WaitForSecondsRealtime(time);
        SpawnEnemy(owner);
    }
}

#endregion

#region Base Attack State
//använder bara denna som en mall för att göra nya states för jag e lat, den borde inte användas
public class BaseAttackState : State<BossAIScript>
{
    private float _durration;
    private Timer _timer;

    public BaseAttackState(float durration)
    {
        _durration = durration;
    }


    public override void EnterState(BossAIScript owner)
    {
        _timer = new Timer(_durration);
    }

    public override void ExitState(BossAIScript owner)
    {
    }

    public override void UpdateState(BossAIScript owner)
    {
        _timer.Time += Time.deltaTime;

        if (_timer.Expired)
        {
            //owner.phaseOneStateMashine.ChangeState(owner.phaseOneCombatState);
        }
    }
}
#endregion

//////////////////
//PHASE 1 STATES//
//////////////////
#region Phase 1 States
public class BossPhaseOneState : State<BossAIScript>
{
    public override void EnterState(BossAIScript owner)
    {
        owner.bossCombatState = new BossPhaseOneCombatState(owner.minAttackSpeed, owner.attackSpeedIncreaseMax, owner.minAttackCooldown, owner.meleeRange, owner.drainRange, owner.drainAttackCooldown, owner.dashStoppingDistance);

        owner.actionStateMachine.ChangeState(owner.bossCombatState);

        //owner.actionStateMachine.ChangeState(owner.aoeAttackState);        
        //owner.actionStateMachine.ChangeState(owner.spawnEnemiesAbilityState);
        //owner.actionStateMachine.ChangeState(owner.meleeAttackOneState);
    }

    public override void ExitState(BossAIScript owner)
    { }

    public override void UpdateState(BossAIScript owner)
    {
        //kolla om man ska gå över till nästa phase
        if ((owner.GetComponent<EnemyHealth>().CurrentHealth / owner.GetComponent<EnemyHealth>().MaxHealth) < owner.phaseTwoTransitionHP)
        {
            owner.phaseStateMachine.ChangeState(owner.bossPhaseTwoState);
        }
    }
}
public class BossPhaseOneCombatState : State<BossAIScript>
{
    private Timer _timer;
    private Timer _drainTimer;
    private float _minAttackSpeed;
    private float _attackSpeedIncreaseMax;
    private float _attackSpeed;
    private float _baseMinAttackCooldown;
    private float _minAttackCooldown;
    private float _meleeAttackRange;
    private float _drainAttackRange;
    private float _drainAttackCooldown;

    private Vector3 _destination;
    //private Vector3 _direction;
    private Vector3 _dashAttackDirection;
    private float _dashDistance;
    private float _dashStoppingDistance;
    //private float _dashAttackAngle;

    private Vector3 _bossToPlayer;

    private RaycastHit _hit;
    
    public BossPhaseOneCombatState(float minAttackSpeed, float attackSpeedIncreaseMax, float minAttackCooldown, float meleeAttackRange, float drainAttackRange, float drainAttackCooldown, float dashStoppingDistance)
    {
        _minAttackSpeed = minAttackSpeed;
        _attackSpeedIncreaseMax = attackSpeedIncreaseMax / 2;
        _baseMinAttackCooldown = minAttackCooldown;
        _meleeAttackRange = meleeAttackRange;
        _drainAttackRange = drainAttackRange;
        _drainAttackCooldown = drainAttackCooldown;
        _dashStoppingDistance = dashStoppingDistance;

        GenerateNewAttackSpeed();
        _timer = new Timer(_attackSpeed);

        _drainTimer = new Timer(_drainAttackCooldown * 0);
    }

    public override void EnterState(BossAIScript owner)
    {
        owner.targetMovementDirection = owner.transform.right;
        owner.currentMovementDirection = owner.transform.right;
    }

    private void GenerateNewAttackSpeed()
    {
        _attackSpeed = _minAttackSpeed;
        _attackSpeed += UnityEngine.Random.Range(0f, _attackSpeedIncreaseMax);
        _attackSpeed += UnityEngine.Random.Range(0f, _attackSpeedIncreaseMax);
    }

    public override void ExitState(BossAIScript owner)
    {
        //Debug.Log("hej då PhaseOneCombatState");
        owner.agent.SetDestination(owner.transform.position);
    }

    //tycker synd om er om ni behöver kolla i denna update (:
    public override void UpdateState(BossAIScript owner)
    {
        owner.FacePlayer();

        _timer.Time += Time.deltaTime;
        _drainTimer.Time += Time.deltaTime;

        //kanske borde dela upp detta i olika movement states pga animationer men vet inte om det behövs

        //kolla om spelaren är nära nog att slå
        if (_timer.Time > _minAttackCooldown && _meleeAttackRange > Vector3.Distance(owner.transform.position, owner.player.transform.position))
        {
            _minAttackCooldown = _timer.Time + _baseMinAttackCooldown;

            owner.actionStateMachine.ChangeState(owner.meleeAttackOneState);
        }
        //kolla om man ska attackera
        else if (_timer.Expired && _timer.Time > _minAttackCooldown)
        {
            GenerateNewAttackSpeed();
            _timer = new Timer(_attackSpeed);
            _minAttackCooldown = _baseMinAttackCooldown;



            if (_drainAttackRange < Vector3.Distance(owner.transform.position, owner.player.transform.position) && _drainTimer.Expired)
            {
                _drainTimer = new Timer(_drainAttackCooldown);
                owner.actionStateMachine.ChangeState(owner.drainAttackChargeState);
            }
            else
            {
                //funkar?? tror det
                if (TryToDash(owner))
                {
                    owner.actionStateMachine.ChangeState(owner.bossDashState);
                }
                else
                {
                    owner.actionStateMachine.ChangeState(owner.chaseToAttackState);
                }
            }

            ////nära nog för att göra melee attacken
            //if (_drainAttackRange > Vector3.Distance(owner.transform.position, owner.player.transform.position))
            //{
            //    //funkar?? tror det
            //    if (TryToDash(owner))
            //    {
            //        owner.actionStateMachine.ChangeState(owner.bossDashState);
            //    }
            //    else
            //    {
            //        owner.actionStateMachine.ChangeState(owner.chaseToAttackState);
            //    }
            //}
            ////drain
            //else
            //{
            //    owner.actionStateMachine.ChangeState(owner.drainAttackChargeState);
            //}
        }
        //idle movement
        else
        {
            //flytta till fixed uppdate (kanske)
            Physics.Raycast(owner.transform.position + new Vector3(0, 1, 0), (owner.player.transform.position - owner.transform.position).normalized, out _hit, Mathf.Infinity, owner.targetLayers);

            //om bossen kan se spelaren
            if (_hit.transform == owner.player.transform)
            {
                //gör så att den byter mellan att gå höger och vänster
                int strafeSign = 0;

                if (_timer.Ratio > 0.5f)
                {
                    strafeSign = -1;
                }
                else
                {
                    strafeSign = 1;
                }



                owner.targetMovementDirection = owner.transform.right;

                //lägga till någon randomness variabel så movement inte blir lika predictable? (kan fucka animationerna?)
                //kanske slurpa mellan de olika värdena (kan bli jobbigt och vet inte om det behövs)
                float compairValue = Vector3.Distance(owner.transform.position, owner.player.transform.position);

                //fixar så bossen går åt rätt håll baserat på desiredDistanceToPlayer och distansen från bossen till spelaren
                for (int i = 0; i < owner.desiredDistanceValues.Length; i++)
                {
                    if (compairValue > owner.desiredDistanceToPlayer + owner.desiredDistanceOffsetValues[i])
                    {
                        owner.targetMovementDirection = Quaternion.AngleAxis(owner.desiredDistanceAngleValues[i] * strafeSign * -1, Vector3.up) * owner.targetMovementDirection;
                        owner.targetMovementDirection *= strafeSign;

                        Debug.DrawRay(owner.transform.position, owner.targetMovementDirection, Color.black);

                        //ändra så jag har ngn timer här eller ngt idk

                        //owner.currentMovementDirection = Vector3.Lerp(owner.currentMovementDirection, owner.targetMovementDirection, Time.deltaTime /** ((Vector3.Dot(owner.currentMovementDirection, owner.targetMovementDirection) + 1) / 2)*/);

                        //Time.deltaTime * ngn speed
                        owner.currentMovementDirection = Vector3.MoveTowards(owner.currentMovementDirection, owner.targetMovementDirection, Time.deltaTime);

                        //Debug.Log(((Vector3.Dot(owner.currentMovementDirection, owner.targetMovementDirection) + 1) / 2));
                        //owner.currentMovementDirection = Vector3.Lerp(owner.currentMovementDirection, owner.targetMovementDirection, 0.5f * (Time.deltaTime / (1 - ((Vector3.Dot(owner.currentMovementDirection, owner.targetMovementDirection) + 1) / 2))));

                        //Debug.Log(1 - ((Vector3.Dot(owner.currentMovementDirection, owner.targetMovementDirection) + 1) / 2));
                        owner.agent.speed = owner.defaultSpeed + owner.desiredDistanceSpeedIncreseValues[i];
                        break;
                    }
                }

                //owner.currentMovementDirection = owner.transform.right;

                //float compairValue = Vector3.Distance(owner.transform.position, owner.player.transform.position);

                //for (int i = 0; i < owner.desiredDistanceValues.Length; i++)
                //{
                //    if (compairValue > owner.desiredDistanceToPlayer + owner.desiredDistanceOffsetValues[i])
                //    {
                //        owner.currentMovementDirection = Quaternion.AngleAxis(owner.desiredDistanceAngleValues[i] * strafeSign * -1, Vector3.up) * owner.currentMovementDirection;
                //        owner.currentMovementDirection *= strafeSign;
                //        owner.agent.speed = owner.defaultSpeed + owner.desiredDistanceSpeedIncreseValues[i];
                //        break;
                //    }
                //}

                //random dash in combat
                if (UnityEngine.Random.Range(0f, 100f) > 100f - owner.dashChansePerFrame)
                {
                    if (owner.CheckDashPath(owner.currentMovementDirection, owner.dashDistance))
                    {
                        owner.actionStateMachine.ChangeState(owner.bossDashState);
                    }
                    else
                    {
                        Debug.Log("kunde inte dasha för saker va i vägen");
                    }
                }
                else
                {
                    //ändra 5an till typ destinationAmplifier
                    _destination = owner.transform.position + owner.currentMovementDirection * 5;
                    owner.agent.SetDestination(_destination);
                    owner.bossAnimator.SetFloat("WalkDirX", owner.transform.InverseTransformDirection(owner.currentMovementDirection).x);
                    owner.bossAnimator.SetFloat("WalkDirY", owner.transform.InverseTransformDirection(owner.currentMovementDirection).z);
                    //Debug.Log(_destination);
                }
            }
            //om bossen inte kan se spelaren
            else
            {
                _destination = owner.player.transform.position;
                owner.agent.SetDestination(_destination);
            }
        }
    }

    private bool TryToDash(BossAIScript owner)
    {
        //vill den dash attacka?
        if (UnityEngine.Random.Range(0f, 100f) > 100f - owner.dashAttackChanse)
        {
            _bossToPlayer = owner.player.transform.position - owner.transform.position;

            //Physics.Raycast(owner.transform.position + new Vector3(0, 1, 0), _bossToPlayer.normalized, out _hit, owner.dashAttackDistanceMax + _meleeAttackRange, owner.targetLayers);

            //är spelaren innom en bra range och innom LOS?
            if (/*_hit.transform == owner.player.transform &&*/ _bossToPlayer.magnitude < owner.dashAttackDistanceMax + _dashStoppingDistance && _bossToPlayer.magnitude > owner.dashAttackDistanceMin + _dashStoppingDistance)
            {
                _dashAttackDirection = _bossToPlayer.normalized;

                _dashDistance = _bossToPlayer.magnitude - _dashStoppingDistance;

                //är det något i vägen för dashen?
                if (owner.CheckDashPath(_dashAttackDirection, _dashDistance))
                {
                    owner.currentMovementDirection = _dashAttackDirection;
                    owner.dashDistance = _dashDistance;
                    //Debug.Log("_dashDistance " + _dashDistance);
                    //Debug.Log("_bossToPlayer.mag " + _bossToPlayer.magnitude);
                    owner.dashAttack = true;
                    return true;
                }
                //kan tas bort, finns bara för debug 
                else
                {
                    Debug.Log("kan inte dasha med all denna skit ivägen juuuuuöööööö");
                }
            }
            //kan tas bort, finns bara för debug 
            else
            {
                Debug.Log("ITS TO FAR AWAY!!!! (or to close)");
            }
        }
        //kan tas bort, finns bara för debug 
        else
        {
            Debug.Log("no want dash tyvm");
        }
        return false;
    }

    //private bool TryToDashOld(BossAIScript owner)
    //{
    //    //vill den dash attacka?
    //    if (UnityEngine.Random.Range(0f, 100f) > 100f - owner.dashAttackChanse)
    //    {
    //        _bossToPlayer = owner.player.transform.position - owner.transform.position;

    //        Physics.Raycast(owner.transform.position + new Vector3(0, 1, 0), _bossToPlayer.normalized, out _hit, owner.dashDistance + _meleeAttackRange, owner.targetLayers);

    //        //är spelaren innom en bra range och innom LOS?
    //        //if (_hit.transform == _ownerParentScript.player.transform && _bossToPlayer.magnitude < _ownerParentScript.dashDistanceMax + _meleeAttackRange / 2 && _bossToPlayer.magnitude > _ownerParentScript.dashDistanceMax - _meleeAttackRange / 2)
    //        if (_hit.transform == owner.player.transform && _bossToPlayer.magnitude < owner.dashDistance + _meleeAttackRange / 2 && _bossToPlayer.magnitude > owner.dashDistance - _meleeAttackRange / 2)
    //        {
    //            int dashSign = 0;

    //            if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
    //            {
    //                dashSign = 1;
    //            }
    //            else
    //            {
    //                dashSign = -1;
    //            }

    //            _dashAttackAngle = Mathf.Rad2Deg * Mathf.Acos((Mathf.Pow(_bossToPlayer.magnitude, 2) + Mathf.Pow(owner.dashDistance, 2) - Mathf.Pow(_meleeAttackRange / 2, 2)) / (2 * _bossToPlayer.magnitude * owner.dashDistance));

    //            _dashAttackDirection = _bossToPlayer;
    //            _dashAttackDirection = Quaternion.AngleAxis(_dashAttackAngle * dashSign, Vector3.up) * _dashAttackDirection;

    //            Vector3 _playerToDashPos = (owner.transform.position + _dashAttackDirection.normalized * owner.dashDistance) - owner.player.transform.position;
    //            Vector3 _playerToBoss = owner.transform.position - owner.player.transform.position;
    //            float _angleDashAttackToPlayer = Vector3.Angle(_playerToBoss, _playerToDashPos);

    //            //är vinkeln mellan spelaren till dit bossen kommer dasha en ok vinkel 
    //            if (_angleDashAttackToPlayer < owner.maxAngleDashAttackToPlayer)
    //            {
    //                //ändra så det inte är en siffra utan att det beror på deras hittboxes storlek eller en parameter

    //                //ranomizar vart bossen kommer dasha, sålänge den inte skulle kunna krocka med spelaren
    //                if (_bossToPlayer.magnitude - 0.45f > owner.dashDistance)
    //                {
    //                    _dashAttackAngle = UnityEngine.Random.Range(0, _dashAttackAngle);
    //                    _dashAttackDirection = _bossToPlayer;
    //                    _dashAttackDirection = Quaternion.AngleAxis(_dashAttackAngle * dashSign * -1, Vector3.up) * _dashAttackDirection;
    //                }
    //                //är det något i vägen för dashen?
    //                if (owner.CheckDashPath(_dashAttackDirection, owner.dashDistance))
    //                {
    //                    owner.movementDirection = _dashAttackDirection;
    //                    owner.dashAttack = true;
    //                    return true;
    //                }
    //                else
    //                {
    //                    _dashAttackDirection = _bossToPlayer;
    //                    //"*-1" för att få andra sidan av spelaren
    //                    _dashAttackDirection = Quaternion.AngleAxis(_dashAttackAngle * dashSign * -1, Vector3.up) * _dashAttackDirection;

    //                    //är något i vägen om den dashar till andra sidan av spelaren?
    //                    if (owner.CheckDashPath(_dashAttackDirection, owner.dashDistance))
    //                    {
    //                        owner.movementDirection = _dashAttackDirection;
    //                        owner.dashAttack = true;
    //                        return true;
    //                    }
    //                    //kan tas bort, finns bara för debug 
    //                    else
    //                    {
    //                        Debug.Log("kan inte dasha med all denna skit ivägen juuuuuöööööö");
    //                    }
    //                }
    //            }
    //            //kan tas bort, finns bara för debug 
    //            else
    //            {
    //                Debug.Log("no want dash tru player");
    //            }
    //        }
    //        //kan tas bort, finns bara för debug 
    //        else
    //        {
    //            Debug.Log("ITS TO FAR AWAY!!!! (or to close)");
    //        }
    //    }
    //    //kan tas bort, finns bara för debug 
    //    else
    //    {
    //        Debug.Log("no want dash tyvm");
    //    }
    //    return false;
    //}

}
#endregion

//////////////////
//PHASE 2 STATES//
//////////////////
#region Phase 2 States
public class BossPhaseTwoState : State<BossAIScript>
{

    public override void EnterState(BossAIScript owner)
    {
        owner.bossCombatState = new BossPhaseTwoCombatState(owner.minAttackSpeed, owner.attackSpeedIncreaseMax, owner.minAttackCooldown, owner.meleeRange, owner.drainRange, owner.drainAttackCooldown, owner.dashStoppingDistance, owner.aoeAttackCooldown, owner.spawnEnemyAbilityCooldown);
        //om man vill ändra värden för states gör mad det här
        owner.bossAnimator.SetTrigger("newPhaseTrigger");

        owner.audioManager.BossChangePhaseAudio(owner.transform.position);

        owner.actionStateMachine.ChangeState(owner.bossCombatState);
    }

    public override void ExitState(BossAIScript owner)
    {
    }

    public override void UpdateState(BossAIScript owner)
    {
        //Debug.Log("nu chillar vi i Phase 2 :)");
    }
}
public class BossPhaseTwoCombatState : State<BossAIScript>
{
    private Timer _meleeAttackTimer;
    private Timer _AOEAttackTimer;
    private Timer _spawnEnemyAbilityTimer;
    private Timer _drainTimer;

    private float _aoeAttackCooldown;
    private float _spawnEnemyAbilityCooldown;
    private float _drainAttackCooldown;


    private float _minMeleeAttackSpeed;
    private float _meleeAttackSpeedIncreaseMax;
    private float _meleeAttackSpeed;
    private float _baseMinMeleeAttackCooldown;
    private float _minMeleeAttackCooldown;

    private float _meleeAttackRange;
    private float _drainAttackRange;


    private Vector3 _destination;
    private Vector3 _dashAttackDirection;
    private float _dashDistance;
    private float _dashStoppingDistance;

    private Vector3 _bossToPlayer;

    private RaycastHit _hit;

    public BossPhaseTwoCombatState(float minAttackSpeed, float attackSpeedIncreaseMax, float minAttackCooldown, float meleeAttackRange, float drainAttackRange, float drainAttackCooldown, float dashStoppingDistance, float aoeAttackCooldown, float spawnEnemyAbilityCooldown)
    {
        _minMeleeAttackSpeed = minAttackSpeed;
        _meleeAttackSpeedIncreaseMax = attackSpeedIncreaseMax / 2;
        _baseMinMeleeAttackCooldown = minAttackCooldown;
        _meleeAttackRange = meleeAttackRange;
        _drainAttackRange = drainAttackRange;
        _drainAttackCooldown = drainAttackCooldown;
        _aoeAttackCooldown = aoeAttackCooldown;
        _spawnEnemyAbilityCooldown = spawnEnemyAbilityCooldown;
        _dashStoppingDistance = dashStoppingDistance;


        GenerateNewMeleeAttackSpeed();
        _meleeAttackTimer = new Timer(_meleeAttackSpeed);
        _AOEAttackTimer = new Timer(_aoeAttackCooldown);
        _spawnEnemyAbilityTimer = new Timer(_spawnEnemyAbilityCooldown);
        _drainTimer = new Timer(_drainAttackCooldown);
    }

    public override void EnterState(BossAIScript owner)
    { }

    private void GenerateNewMeleeAttackSpeed()
    {
        _meleeAttackSpeed = _minMeleeAttackSpeed;
        _meleeAttackSpeed += UnityEngine.Random.Range(0f, _meleeAttackSpeedIncreaseMax);
        _meleeAttackSpeed += UnityEngine.Random.Range(0f, _meleeAttackSpeedIncreaseMax);
    }

    public override void ExitState(BossAIScript owner)
    {
        owner.agent.SetDestination(owner.transform.position);
    }

    //tycker synd om er om ni behöver kolla i denna update (:
    public override void UpdateState(BossAIScript owner)
    {
        owner.FacePlayer();

        _meleeAttackTimer.Time += Time.deltaTime;
        _AOEAttackTimer.Time += Time.deltaTime;
        _spawnEnemyAbilityTimer.Time += Time.deltaTime;
        _drainTimer.Time += Time.deltaTime;

        //Daggs för AOE?
        if (_AOEAttackTimer.Expired)
        {
            _AOEAttackTimer = new Timer(_aoeAttackCooldown);
            owner.actionStateMachine.ChangeState(owner.aoeAttackState);
        }
        //Daggs för spawna bois (och det fins ingen boi ute)?
        else if (_spawnEnemyAbilityTimer.Expired && owner.spawnedEnemy == null)
        {
            _spawnEnemyAbilityTimer = new Timer(_spawnEnemyAbilityCooldown);
            owner.actionStateMachine.ChangeState(owner.spawnEnemiesAbilityState);
        }
        //kolla om spelaren är nära nog att slå
        else if (_meleeAttackTimer.Time > _minMeleeAttackCooldown && _meleeAttackRange > Vector3.Distance(owner.transform.position, owner.player.transform.position))
        {
            _minMeleeAttackCooldown = _meleeAttackTimer.Time + _baseMinMeleeAttackCooldown;

            owner.actionStateMachine.ChangeState(owner.meleeAttackOneState);
        }
        //kolla om man ska attackera
        else if (_meleeAttackTimer.Expired && _meleeAttackTimer.Time > _minMeleeAttackCooldown)
        {
            GenerateNewMeleeAttackSpeed();
            _meleeAttackTimer = new Timer(_meleeAttackSpeed);
            _minMeleeAttackCooldown = _baseMinMeleeAttackCooldown;

            if (_drainAttackRange < Vector3.Distance(owner.transform.position, owner.player.transform.position) && _drainTimer.Expired)
            {
                _drainTimer = new Timer(_drainAttackCooldown);
                owner.actionStateMachine.ChangeState(owner.drainAttackChargeState);
            }
            else
            {
                //funkar?? tror det
                if (TryToDash(owner))
                {
                    owner.actionStateMachine.ChangeState(owner.bossDashState);
                }
                else
                {
                    owner.actionStateMachine.ChangeState(owner.chaseToAttackState);
                }
            }
        }
        //idle movement
        //kanske borde dela upp detta i olika movement states pga animationer men vet inte om det behövs
        else
        {
            //flytta till fixed uppdate (kanske)
            Physics.Raycast(owner.transform.position + new Vector3(0, 1, 0), (owner.player.transform.position - owner.transform.position).normalized, out _hit, Mathf.Infinity, owner.targetLayers);

            //om bossen kan se spelaren
            if (_hit.transform == owner.player.transform)
            {
                //gör så att den byter mellan att gå höger och vänster
                int strafeSign = 0;

                if (_meleeAttackTimer.Ratio > 0.5f)
                {
                    strafeSign = -1;
                }
                else
                {
                    strafeSign = 1;
                }

                owner.targetMovementDirection = owner.transform.right;

                //lägga till någon randomness variabel så movement inte blir lika predictable? (kan fucka animationerna?)
                //kanske slurpa mellan de olika värdena (kan bli jobbigt och vet inte om det behövs)
                float compairValue = Vector3.Distance(owner.transform.position, owner.player.transform.position);

                //fixar så bossen går åt rätt håll baserat på desiredDistanceToPlayer och distansen från bossen till spelaren
                for (int i = 0; i < owner.desiredDistanceValues.Length; i++)
                {
                    if (compairValue > owner.desiredDistanceToPlayer + owner.desiredDistanceOffsetValues[i])
                    {
                        owner.targetMovementDirection = Quaternion.AngleAxis(owner.desiredDistanceAngleValues[i] * strafeSign * -1, Vector3.up) * owner.targetMovementDirection;
                        owner.targetMovementDirection *= strafeSign;

                        Debug.DrawRay(owner.transform.position, owner.targetMovementDirection, Color.black);

                        //ändra så jag har ngn timer här eller ngt idk

                        //owner.currentMovementDirection = Vector3.Lerp(owner.currentMovementDirection, owner.targetMovementDirection, Time.deltaTime /** ((Vector3.Dot(owner.currentMovementDirection, owner.targetMovementDirection) + 1) / 2)*/);

                        //Time.deltaTime * ngn speed
                        owner.currentMovementDirection = Vector3.MoveTowards(owner.currentMovementDirection, owner.targetMovementDirection, Time.deltaTime);

                        //Debug.Log(((Vector3.Dot(owner.currentMovementDirection, owner.targetMovementDirection) + 1) / 2));
                        //owner.currentMovementDirection = Vector3.Lerp(owner.currentMovementDirection, owner.targetMovementDirection, 0.5f * (Time.deltaTime / (1 - ((Vector3.Dot(owner.currentMovementDirection, owner.targetMovementDirection) + 1) / 2))));

                        //Debug.Log(1 - ((Vector3.Dot(owner.currentMovementDirection, owner.targetMovementDirection) + 1) / 2));
                        owner.agent.speed = owner.defaultSpeed + owner.desiredDistanceSpeedIncreseValues[i];
                        break;
                    }
                }

                //owner.currentMovementDirection = owner.transform.right;

                //float compairValue = Vector3.Distance(owner.transform.position, owner.player.transform.position);

                //for (int i = 0; i < owner.desiredDistanceValues.Length; i++)
                //{
                //    if (compairValue > owner.desiredDistanceToPlayer + owner.desiredDistanceOffsetValues[i])
                //    {
                //        owner.currentMovementDirection = Quaternion.AngleAxis(owner.desiredDistanceAngleValues[i] * strafeSign * -1, Vector3.up) * owner.currentMovementDirection;
                //        owner.currentMovementDirection *= strafeSign;
                //        owner.agent.speed = owner.defaultSpeed + owner.desiredDistanceSpeedIncreseValues[i];
                //        break;
                //    }
                //}

                //random dash in combat (vet inte om detta ska vara med)
                if (UnityEngine.Random.Range(0f, 100f) > 100f - owner.dashChansePerFrame)
                {
                    if (owner.CheckDashPath(owner.currentMovementDirection, owner.dashDistance))
                    {
                        owner.actionStateMachine.ChangeState(owner.bossDashState);
                    }
                    else
                    {
                        Debug.Log("kunde inte dasha för saker va i vägen");
                    }
                }
                else
                {
                    //ändra 5an till typ destinationAmplifier
                    _destination = owner.transform.position + owner.currentMovementDirection * 5;
                    owner.agent.SetDestination(_destination);
                }
            }
            //om bossen inte kan se spelaren
            else
            {
                _destination = owner.player.transform.position;
                owner.agent.SetDestination(_destination);
            }
        }
    }
    private bool TryToDash(BossAIScript owner)
    {
        //vill den dash attacka?
        if (UnityEngine.Random.Range(0f, 100f) > 100f - owner.dashAttackChanse)
        {
            _bossToPlayer = owner.player.transform.position - owner.transform.position;

            //Physics.Raycast(owner.transform.position + new Vector3(0, 1, 0), _bossToPlayer.normalized, out _hit, owner.dashAttackDistanceMax + _meleeAttackRange, owner.targetLayers);

            //är spelaren innom en bra range och innom LOS?
            if (/*_hit.transform == owner.player.transform &&*/ _bossToPlayer.magnitude < owner.dashAttackDistanceMax + _dashStoppingDistance && _bossToPlayer.magnitude > owner.dashAttackDistanceMin + _dashStoppingDistance)
            {
                _dashAttackDirection = _bossToPlayer.normalized;

                _dashDistance = _bossToPlayer.magnitude - _dashStoppingDistance;

                //är det något i vägen för dashen?
                if (owner.CheckDashPath(_dashAttackDirection, _dashDistance))
                {
                    owner.currentMovementDirection = _dashAttackDirection;
                    owner.dashDistance = _dashDistance;
                    Debug.Log("_dashDistance " + _dashDistance);
                    Debug.Log("_bossToPlayer.mag " + _bossToPlayer.magnitude);
                    owner.dashAttack = true;
                    return true;
                }
                //kan tas bort, finns bara för debug 
                else
                {
                    Debug.Log("kan inte dasha med all denna skit ivägen juuuuuöööööö");
                }
            }
            //kan tas bort, finns bara för debug 
            else
            {
                Debug.Log("ITS TO FAR AWAY!!!! (or to close)");
            }
        }
        //kan tas bort, finns bara för debug 
        else
        {
            Debug.Log("no want dash tyvm");
        }
        return false;
    }

    //private bool TryToDashOld(BossAIScript owner)
    //{
    //    //vill den dash attacka?
    //    if (UnityEngine.Random.Range(0f, 100f) > 100f - owner.dashAttackChanse)
    //    {
    //        _bossToPlayer = owner.player.transform.position - owner.transform.position;

    //        Physics.Raycast(owner.transform.position + new Vector3(0, 1, 0), _bossToPlayer.normalized, out _hit, owner.dashDistance + _meleeAttackRange, owner.targetLayers);

    //        //är spelaren innom en bra range och innom LOS?
    //        //if (_hit.transform == _ownerParentScript.player.transform && _bossToPlayer.magnitude < _ownerParentScript.dashDistanceMax + _meleeAttackRange / 2 && _bossToPlayer.magnitude > _ownerParentScript.dashDistanceMax - _meleeAttackRange / 2)
    //        if (_hit.transform == owner.player.transform && _bossToPlayer.magnitude < owner.dashDistance + _meleeAttackRange / 2 && _bossToPlayer.magnitude > owner.dashDistance - _meleeAttackRange / 2)
    //        {
    //            int dashSign = 0;

    //            if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
    //            {
    //                dashSign = 1;
    //            }
    //            else
    //            {
    //                dashSign = -1;
    //            }

    //            _dashAttackAngle = Mathf.Rad2Deg * Mathf.Acos((Mathf.Pow(_bossToPlayer.magnitude, 2) + Mathf.Pow(owner.dashDistance, 2) - Mathf.Pow(_meleeAttackRange / 2, 2)) / (2 * _bossToPlayer.magnitude * owner.dashDistance));

    //            _dashAttackDirection = _bossToPlayer;
    //            _dashAttackDirection = Quaternion.AngleAxis(_dashAttackAngle * dashSign, Vector3.up) * _dashAttackDirection;

    //            Vector3 _playerToDashPos = (owner.transform.position + _dashAttackDirection.normalized * owner.dashDistance) - owner.player.transform.position;
    //            Vector3 _playerToBoss = owner.transform.position - owner.player.transform.position;
    //            float _angleDashAttackToPlayer = Vector3.Angle(_playerToBoss, _playerToDashPos);

    //            //är vinkeln mellan spelaren till dit bossen kommer dasha en ok vinkel 
    //            if (_angleDashAttackToPlayer < owner.maxAngleDashAttackToPlayer)
    //            {
    //                //ändra så det inte är en siffra utan att det beror på deras hittboxes storlek eller en parameter

    //                //ranomizar vart bossen kommer dasha, sålänge den inte skulle kunna krocka med spelaren
    //                if (_bossToPlayer.magnitude - 0.45f > owner.dashDistance)
    //                {
    //                    _dashAttackAngle = UnityEngine.Random.Range(0, _dashAttackAngle);
    //                    _dashAttackDirection = _bossToPlayer;
    //                    _dashAttackDirection = Quaternion.AngleAxis(_dashAttackAngle * dashSign * -1, Vector3.up) * _dashAttackDirection;
    //                }
    //                //är det något i vägen för dashen?
    //                if (owner.CheckDashPath(_dashAttackDirection, owner.dashDistance))
    //                {
    //                    owner.movementDirection = _dashAttackDirection;
    //                    owner.dashAttack = true;
    //                    return true;
    //                }
    //                else
    //                {
    //                    _dashAttackDirection = _bossToPlayer;
    //                    //"*-1" för att få andra sidan av spelaren
    //                    _dashAttackDirection = Quaternion.AngleAxis(_dashAttackAngle * dashSign * -1, Vector3.up) * _dashAttackDirection;

    //                    //är något i vägen om den dashar till andra sidan av spelaren?
    //                    if (owner.CheckDashPath(_dashAttackDirection, owner.dashDistance))
    //                    {
    //                        owner.movementDirection = _dashAttackDirection;
    //                        owner.dashAttack = true;
    //                        return true;
    //                    }
    //                    //saker va i vägen för dashen
    //                    else
    //                    {
    //                        Debug.Log("kan inte dasha med all denna skit ivägen juuuuuöööööö");
    //                        return false;
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                Debug.Log("no want dash tru player");
    //                return false;
    //            }
    //        }
    //        else
    //        {
    //            Debug.Log("ITS TO FAR AWAY!!!! (or to close)");
    //            return false;
    //        }
    //    }
    //    //springa och slå
    //    else
    //    {
    //        Debug.Log("no want dash tyvm");
    //        return false;
    //    }
    //}
}
#endregion

#region Dead state
public class BossDeadState : State<BossAIScript>
{
    public override void EnterState(BossAIScript owner)
    {
        owner.audioManager.BossChangePhaseAudio(owner.transform.position);
        owner.bossAnimator.SetBool("deathBool", true);
        owner.GetComponent<HitboxEventHandler>().DisableHitboxes(0);
        owner.GetComponent<HitboxEventHandler>().EndAnim();
        owner.agent.SetDestination(owner.transform.position);
        owner.StopAllCoroutines();
        if (owner.spawnedEnemy)
        {
            owner.spawnedEnemy.GetComponent<BaseAIMovementController>().KillThis();
        }
        owner.actionStateMachine.ChangeState(owner.bossIdleActionState);

        GlobalState.state.AudioManager.StopMusic();
    }

    public override void ExitState(BossAIScript owner)
    {
    }

    public override void UpdateState(BossAIScript owner)
    {
        if (owner.animationEnded)
        {
            owner.audioManager.BossMusicCancel();
            SceneData.gameStarted = false;
            SaveData.DeleteSaveFile();
            //owner.victoryScreen.SetActive(true);

            try
            {
                SceneManager.LoadScene(1);
            }
            catch
            {
                Debug.LogWarning("There is no scene index 1, add the credits scene to scene index 1");
                //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            //GameObject.Destroy(owner.gameObject);


            //owner.Destroy(owner.gameObject);
        }
    }
}
#endregion




#region Old Phase 1 States
//public class OldBossPhaseOneState : State<BossAIScript>
//{

//    public BossAIScript parentScript;

//    public StateMachine<OldBossPhaseOneState> phaseOneStateMashine;


//    //public Phase1Attack1State phase1Attack1State;

//    public PhaseOneCombatState phaseOneCombatState;
//    public PhaseOneChargeDrainAttackState phaseOneChargeDrainAttackState;
//    public PhaseOneActiveDrainAttackState phaseOneActiveDrainAttackState;
//    public PhaseOneMeleeAttackOneState phaseOneMeleeAttackOneState;
//    public PhaseOneDashState phaseOneDashState;
//    public PhaseOneChaseToAttackState phaseOneChaseToAttackState;
//    public PhaseOneAOEAttackState phaseOneAOEAttackState;
//    public PhaseOneSpawnAbilityState phaseOneSpawnAddsAbilityState;

//    public override void EnterState(BossAIScript owner)
//    {
//        phaseOneStateMashine = new StateMachine<OldBossPhaseOneState>(this);

//        //phase1Attack1State = new Phase1Attack1State(owner.drainChargeTime);

//        phaseOneCombatState = new PhaseOneCombatState(owner.minAttackSpeed, owner.attackSpeedIncreaseMax, owner.minAttackCooldown, owner.meleeRange, owner.drainRange, owner);
//        phaseOneDashState = new PhaseOneDashState(owner.dashSpeed, owner.dashDistance, owner.dashLagDurration, owner.dashAcceleration);
//        phaseOneChaseToAttackState = new PhaseOneChaseToAttackState();

//        phaseOneChargeDrainAttackState = new PhaseOneChargeDrainAttackState(owner.drainChargeTime);
//        phaseOneActiveDrainAttackState = new PhaseOneActiveDrainAttackState(owner.drainAttackTime);
//        phaseOneMeleeAttackOneState = new PhaseOneMeleeAttackOneState();
//        phaseOneAOEAttackState = new PhaseOneAOEAttackState();
//        phaseOneSpawnAddsAbilityState = new PhaseOneSpawnAbilityState();


//        parentScript = owner;

//        phaseOneStateMashine.ChangeState(phaseOneCombatState);

//        //phaseOneStateMashine.ChangeState(phaseOneAOEAttackState);
//        //phaseOneStateMashine.ChangeState(phaseOneSpawnAddsAbilityState);

//        //spela cool animation :)

//        //owner.MurkyWaterSpiralAbility(10, 6, 2f);
//        //owner.MurkyWaterCircleAbility(10, 6);
//        //owner.MurkyWaterCircleAbility(10, 1);
//        //owner.MurkyWaterPolygonAbility(5, 6, 2);

//        //owner.SpawnEnemy( Vector3.forward * 3f, owner.enemyToSpawnPrefab);

//        //owner.StartCoroutine(owner.SpawnEnemyAbility(3f, owner.enemySpawnList));

//    }

//    public override void ExitState(BossAIScript owner)
//    { }

//    public override void UpdateState(BossAIScript owner)
//    {
//        //kolla om man ska gå över till nästa phase
//        //if ((owner.GetComponent<EnemyHealth>().GetHealth() / owner.GetComponent<EnemyHealth>().GetMaxHealth()) < owner.testP2TransitionHP)
//        //{
//        //    owner.phaseControllingStateMachine.ChangeState(owner.bossPhaseTwoState);
//        //}

//        phaseOneStateMashine.Update();
//    }
//}

////vet inte om allt detta typ egentligen borde göras i parent statet (borde typ det tror jag)
//public class PhaseOneCombatState : State<OldBossPhaseOneState>
//{
//    private Timer _timer;
//    private float _minAttackSpeed;
//    private float _attackSpeedIncreaseMax;
//    private float _attackSpeed;
//    private float _baseMinAttackCooldown;
//    private float _minAttackCooldown;
//    private float _meleeAttackRange;
//    private float _drainAttackRange;

//    private Vector3 _destination;
//    //private Vector3 _direction;
//    private Vector3 _dashAttackDirection;
//    private float _dashAttackAngle;

//    private Vector3 _bossToPlayer;

//    private RaycastHit _hit;

//    private BossAIScript _ownerParentScript;

//    public PhaseOneCombatState(float minAttackSpeed, float attackSpeedIncreaseMax, float minAttackCooldown, float meleeAttackRange, float drainAttackRange, BossAIScript ownerParentScript)
//    {
//        _minAttackSpeed = minAttackSpeed;
//        _attackSpeedIncreaseMax = attackSpeedIncreaseMax / 2;
//        _baseMinAttackCooldown = minAttackCooldown;
//        _meleeAttackRange = meleeAttackRange;
//        _drainAttackRange = drainAttackRange;

//        _ownerParentScript = ownerParentScript;

//        GenerateNewAttackSpeed();
//        _timer = new Timer(_attackSpeed);
//    }

//    public override void EnterState(OldBossPhaseOneState owner)
//    {
//        //Debug.Log("in i PhaseOneCombatState");
//        if (_timer.Expired)
//        {
//            GenerateNewAttackSpeed();
//            _timer = new Timer(_attackSpeed);
//            _minAttackCooldown = _baseMinAttackCooldown;
//        }
//        else
//        {
//            _minAttackCooldown += _timer.Time;
//        }
//    }

//    private void GenerateNewAttackSpeed()
//    {
//        _attackSpeed = _minAttackSpeed;
//        _attackSpeed += UnityEngine.Random.Range(0f, _attackSpeedIncreaseMax);
//        _attackSpeed += UnityEngine.Random.Range(0f, _attackSpeedIncreaseMax);
//    }

//    public override void ExitState(OldBossPhaseOneState owner)
//    {
//        //Debug.Log("hej då PhaseOneCombatState");
//        _ownerParentScript.agent.SetDestination(_ownerParentScript.transform.position);
//    }

//    //tycker synd om er om ni behöver kolla i denna update (:
//    public override void UpdateState(OldBossPhaseOneState owner)
//    {
//        _ownerParentScript.FacePlayer();

//        _timer.Time += Time.deltaTime;

//        //kanske borde dela upp detta i olika movement states pga animationer men vet inte om det behövs

//        //kolla om spelaren är nära nog att slå
//        if (_timer.Time > _minAttackCooldown && _meleeAttackRange > Vector3.Distance(_ownerParentScript.transform.position, _ownerParentScript.player.transform.position))
//        {
//            //kanske göra AOE attack här för att tvinga iväg spelaren?
//            owner.phaseOneStateMashine.ChangeState(owner.phaseOneMeleeAttackOneState);
//        }
//        //kolla om man ska attackera
//        else if (_timer.Expired && _timer.Time > _minAttackCooldown)
//        {
//            //nära nog för att göra melee attacken
//            if (_drainAttackRange > Vector3.Distance(_ownerParentScript.transform.position, _ownerParentScript.player.transform.position))
//            {
//                //funkar?? tror det
//                if (TryToDash())
//                {
//                    owner.phaseOneStateMashine.ChangeState(owner.phaseOneDashState);
//                }
//                else
//                {
//                    owner.phaseOneStateMashine.ChangeState(owner.phaseOneChaseToAttackState);
//                }
//            }
//            //drain
//            else
//            {
//                owner.phaseOneStateMashine.ChangeState(owner.phaseOneChargeDrainAttackState);
//            }
//        }
//        //idle movement
//        else
//        {
//            //flytta till fixed uppdate (kanske)
//            Physics.Raycast(_ownerParentScript.transform.position + new Vector3(0, 1, 0), (_ownerParentScript.player.transform.position - _ownerParentScript.transform.position).normalized, out _hit, Mathf.Infinity, _ownerParentScript.targetLayers);

//            //om bossen kan se spelaren
//            if (_hit.transform == _ownerParentScript.player.transform)
//            {
//                //gör så att den byter mellan att gå höger och vänster
//                int strafeSign = 0;

//                if (_timer.Ratio > 0.5f)
//                {
//                    strafeSign = -1;
//                }
//                else
//                {
//                    strafeSign = 1;
//                }

//                _ownerParentScript.movementDirection = _ownerParentScript.transform.right;

//                //lägga till någon randomness variabel så movement inte blir lika predictable? (kan fucka animationerna?)
//                //kanske slurpa mellan de olika värdena (kan bli jobbigt och vet inte om det behövs)
//                float compairValue = Vector3.Distance(_ownerParentScript.transform.position, _ownerParentScript.player.transform.position);

//                for (int i = 0; i < _ownerParentScript.desiredDistanceValues.Length; i++)
//                {
//                    if (compairValue > _ownerParentScript.desiredDistanceToPlayer + _ownerParentScript.desiredDistanceOffsetValues[i])
//                    {
//                        _ownerParentScript.movementDirection = Quaternion.AngleAxis(_ownerParentScript.desiredDistanceAngleValues[i] * strafeSign * -1, Vector3.up) * _ownerParentScript.movementDirection;
//                        _ownerParentScript.movementDirection *= strafeSign;
//                        _ownerParentScript.agent.speed = _ownerParentScript.defaultSpeed + _ownerParentScript.desiredDistanceSpeedIncreseValues[i];
//                        break;
//                    }
//                }

//                //random dash in combat
//                if (UnityEngine.Random.Range(0f, 100f) > 100f - _ownerParentScript.dashChansePerFrame)
//                {
//                    if (_ownerParentScript.CheckDashPath(_ownerParentScript.movementDirection))
//                    {
//                        owner.phaseOneStateMashine.ChangeState(owner.phaseOneDashState);
//                    }
//                    else
//                    {
//                        Debug.Log("kunde inte dasha för saker va i vägen");
//                    }
//                }
//                else
//                {
//                    //ändra 5an till typ destinationAmplifier
//                    _destination = _ownerParentScript.transform.position + _ownerParentScript.movementDirection * 5;
//                    _ownerParentScript.agent.SetDestination(_destination);
//                }
//            }
//            //om bossen inte kan se spelaren
//            else
//            {
//                _destination = _ownerParentScript.player.transform.position;
//                _ownerParentScript.agent.SetDestination(_destination);
//            }
//        }
//    }

//    private bool TryToDash()
//    {
//        //vill den dash attacka?
//        if (UnityEngine.Random.Range(0f, 100f) > 100f - _ownerParentScript.dashAttackChanse)
//        {
//            _bossToPlayer = _ownerParentScript.player.transform.position - _ownerParentScript.transform.position;

//            Physics.Raycast(_ownerParentScript.transform.position + new Vector3(0, 1, 0), _bossToPlayer.normalized, out _hit, _ownerParentScript.dashDistance + _meleeAttackRange, _ownerParentScript.targetLayers);

//            //är spelaren innom en bra range och innom LOS?
//            //if (_hit.transform == _ownerParentScript.player.transform && _bossToPlayer.magnitude < _ownerParentScript.dashDistanceMax + _meleeAttackRange / 2 && _bossToPlayer.magnitude > _ownerParentScript.dashDistanceMax - _meleeAttackRange / 2)
//            if (_hit.transform == _ownerParentScript.player.transform && _bossToPlayer.magnitude < _ownerParentScript.dashDistance + _meleeAttackRange / 2 && _bossToPlayer.magnitude > _ownerParentScript.dashDistance - _meleeAttackRange / 2)
//            {
//                int dashSign = 0;

//                if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
//                {
//                    dashSign = 1;
//                }
//                else
//                {
//                    dashSign = -1;
//                }

//                _dashAttackAngle = Mathf.Rad2Deg * Mathf.Acos((Mathf.Pow(_bossToPlayer.magnitude, 2) + Mathf.Pow(_ownerParentScript.dashDistance, 2) - Mathf.Pow(_meleeAttackRange / 2, 2)) / (2 * _bossToPlayer.magnitude * _ownerParentScript.dashDistance));

//                _dashAttackDirection = _bossToPlayer;
//                _dashAttackDirection = Quaternion.AngleAxis(_dashAttackAngle * dashSign, Vector3.up) * _dashAttackDirection;

//                Vector3 _playerToDashPos = (_ownerParentScript.transform.position + _dashAttackDirection.normalized * _ownerParentScript.dashDistance) - _ownerParentScript.player.transform.position;
//                Vector3 _playerToBoss = _ownerParentScript.transform.position - _ownerParentScript.player.transform.position;
//                float _angleDashAttackToPlayer = Vector3.Angle(_playerToBoss, _playerToDashPos);

//                //är vinkeln mellan spelaren till dit bossen kommer dasha en ok vinkel 
//                if (_angleDashAttackToPlayer < _ownerParentScript.maxAngleDashAttackToPlayer)
//                {
//                    //ändra så det inte är en siffra utan att det beror på deras hittboxes storlek eller en parameter

//                    //ranomizar vart bossen kommer dasha, sålänge den inte skulle kunna krocka med spelaren
//                    if (_bossToPlayer.magnitude - 0.45f > _ownerParentScript.dashDistance)
//                    {
//                        _dashAttackAngle = UnityEngine.Random.Range(0, _dashAttackAngle);
//                        _dashAttackDirection = _bossToPlayer;
//                        _dashAttackDirection = Quaternion.AngleAxis(_dashAttackAngle * dashSign * -1, Vector3.up) * _dashAttackDirection;
//                    }
//                    //är det något i vägen för dashen?
//                    if (_ownerParentScript.CheckDashPath(_dashAttackDirection))
//                    {
//                        _ownerParentScript.movementDirection = _dashAttackDirection;
//                        _ownerParentScript.dashAttack = true;
//                        return true;
//                    }
//                    else
//                    {
//                        _dashAttackDirection = _bossToPlayer;
//                        //"*-1" för att få andra sidan av spelaren
//                        _dashAttackDirection = Quaternion.AngleAxis(_dashAttackAngle * dashSign * -1, Vector3.up) * _dashAttackDirection;

//                        //är något i vägen om den dashar till andra sidan av spelaren?
//                        if (_ownerParentScript.CheckDashPath(_dashAttackDirection))
//                        {
//                            _ownerParentScript.movementDirection = _dashAttackDirection;
//                            _ownerParentScript.dashAttack = true;
//                            return true;
//                        }
//                        //saker va i vägen för dashen
//                        else
//                        {
//                            Debug.Log("kan inte dasha med all denna skit ivägen juuuuuöööööö");
//                            return false;
//                        }
//                    }
//                }
//                else
//                {
//                    Debug.Log("no want dash tru player");
//                    return false;
//                }
//            }
//            else
//            {
//                Debug.Log("ITS TO FAR AWAY!!!! (or to close)");
//                return false;
//            }
//        }
//        //springa och slå
//        else
//        {
//            Debug.Log("no want dash tyvm");
//            return false;
//        }
//    }
//}

//public class PhaseOneDashState : State<OldBossPhaseOneState>
//{
//    private float _dashSpeed;
//    private float _oldSpeed;
//    private float _dashDistance;
//    private float _dashDurration;
//    private float _lagDurration;
//    private float _dashAcceleration;
//    private float _oldAcceleration;

//    private Timer _dashTimer;
//    private Timer _lagTimer;

//    private Vector3 _dashDirection;
//    private Vector3 _dashDestination;


//    public PhaseOneDashState(float speed, float distance, float lagDurration, float acceleration)
//    {
//        _dashSpeed = speed;
//        _dashDistance = distance;
//        _lagDurration = lagDurration;
//        _dashAcceleration = acceleration;
//    }


//    public override void EnterState(OldBossPhaseOneState owner)
//    {
//        //animation stuff
//        //if (owner.parentScript.dashAttack)
//        //{
//        //    //owner.parentScript.bossAnimator.SetTrigger("dashForwardTrigger");
//        //    //_dashDistance = owner.parentScript.dashDistance;
//        //}
//        //else
//        //{
//        //    //_dashDistance = owner.parentScript.dashDistanceMax;
//        //}


//        _oldSpeed = owner.parentScript.agent.speed;
//        _oldAcceleration = owner.parentScript.agent.acceleration;

//        _dashDurration = (_dashDistance - owner.parentScript.agent.stoppingDistance) / _dashSpeed;
//        //Debug.Log("zoom for, " + _dashDurration + " MPH, " + _dashSpeed);
//        //Debug.Log(_dashDistance + " " + owner.bossPhaseOneParentScript.agent.stoppingDistance + " " + _dashSpeed);
//        _dashTimer = new Timer(_dashDurration);
//        _lagTimer = new Timer(_lagDurration);

//        owner.parentScript.agent.speed = _dashSpeed;
//        owner.parentScript.agent.acceleration = _dashAcceleration;

//        _dashDirection = owner.parentScript.movementDirection.normalized;
//        _dashDestination = owner.parentScript.transform.position + _dashDirection * _dashDistance;

//        owner.parentScript.agent.SetDestination(_dashDestination);
//    }

//    public override void ExitState(OldBossPhaseOneState owner)
//    {
//        owner.parentScript.agent.speed = _oldSpeed;
//        owner.parentScript.agent.acceleration = _oldAcceleration;
//        owner.parentScript.agent.SetDestination(owner.parentScript.transform.position);
//        //Debug.Log("hej då zoom");
//    }

//    public override void UpdateState(OldBossPhaseOneState owner)
//    {
//        _dashTimer.Time += Time.deltaTime;

//        if (_dashTimer.Expired)
//        {
//            _lagTimer.Time += Time.deltaTime;

//            if (owner.parentScript.dashAttack)
//            {
//                owner.parentScript.dashAttack = false;
//                owner.phaseOneStateMashine.ChangeState(owner.phaseOneMeleeAttackOneState);
//            }
//            else if (_lagTimer.Expired)
//            {
//                owner.phaseOneStateMashine.ChangeState(owner.phaseOneCombatState);
//            }
//        }
//    }
//}

//public class PhaseOneMeleeAttackOneState : State<OldBossPhaseOneState>
//{
//    public override void EnterState(OldBossPhaseOneState owner)
//    {
//        owner.parentScript.bossAnimator.SetTrigger("melee1Trigger");
//        owner.parentScript.meleeAttackHitboxGroup.enabled = true;
//    }

//    public override void ExitState(OldBossPhaseOneState owner)
//    {
//        owner.parentScript.meleeAttackHitboxGroup.enabled = false;
//        owner.parentScript.animationEnded = false;
//        owner.parentScript.facePlayerBool = true;
//    }

//    public override void UpdateState(OldBossPhaseOneState owner)
//    {
//        if (owner.parentScript.animationEnded)
//        {
//            owner.phaseOneStateMashine.ChangeState(owner.phaseOneCombatState);
//        }
//        else if (owner.parentScript.facePlayerBool)
//        {
//            //spela attackljud här
//            owner.parentScript.FacePlayer();

//        }
//    }
//}

//public class PhaseOneChaseToAttackState : State<OldBossPhaseOneState>
//{
//    private Vector3 _playerPos;
//    private float _distanceToPlayer;
//    private float _oldSpeed;
//    private float _oldAcceleration;


//    public override void EnterState(OldBossPhaseOneState owner)
//    {
//        _oldSpeed = owner.parentScript.agent.speed;
//        _oldAcceleration = owner.parentScript.agent.acceleration;

//        owner.parentScript.agent.speed = owner.parentScript.chasingSpeed;
//        owner.parentScript.agent.acceleration = owner.parentScript.chasingAcceleration;
//        owner.parentScript.bossAnimator.SetTrigger("runningTrigger");
//    }

//    public override void ExitState(OldBossPhaseOneState owner)
//    {
//        owner.parentScript.agent.speed = _oldSpeed;
//        owner.parentScript.agent.acceleration = _oldAcceleration;

//        owner.parentScript.agent.SetDestination(owner.parentScript.transform.position);
//    }

//    public override void UpdateState(OldBossPhaseOneState owner)
//    {
//        owner.parentScript.FacePlayer();

//        _playerPos = owner.parentScript.player.transform.position;
//        _distanceToPlayer = Vector3.Distance(owner.parentScript.transform.position, _playerPos);

//        if (_distanceToPlayer > owner.parentScript.drainRange)
//        {
//            owner.phaseOneStateMashine.ChangeState(owner.phaseOneChargeDrainAttackState);
//        }
//        else if (_distanceToPlayer < owner.parentScript.meleeRange)
//        {
//            owner.phaseOneStateMashine.ChangeState(owner.phaseOneMeleeAttackOneState);
//        }
//        else
//        {
//            owner.parentScript.agent.SetDestination(_playerPos);
//        }
//    }
//}

//public class PhaseOneChargeDrainAttackState : State<OldBossPhaseOneState>
//{
//    private float _chargeTime;
//    private Timer _timer;

//    public PhaseOneChargeDrainAttackState(float chargeTime)
//    {
//        _chargeTime = chargeTime;
//    }

//    public override void EnterState(OldBossPhaseOneState owner)
//    {
//        owner.parentScript.bossAnimator.SetTrigger("drainStartTrigger");
//        _timer = new Timer(_chargeTime);
//    }

//    public override void ExitState(OldBossPhaseOneState owner)
//    {
//    }

//    public override void UpdateState(OldBossPhaseOneState owner)
//    {
//        _timer.Time += Time.deltaTime;

//        owner.parentScript.FacePlayer();

//        if (_timer.Expired)
//        {
//            owner.phaseOneStateMashine.ChangeState(owner.phaseOneActiveDrainAttackState);
//        }
//    }
//}

//public class PhaseOneActiveDrainAttackState : State<OldBossPhaseOneState>
//{
//    private float _durration;
//    private Timer _timer;

//    public PhaseOneActiveDrainAttackState(float durration)
//    {
//        _durration = durration;
//    }

//    public override void EnterState(OldBossPhaseOneState owner)
//    {
//        owner.parentScript.bossAnimator.SetBool("drainActiveBool", true);
//        owner.parentScript.drainAttackHitboxGroup.enabled = true;
//        owner.parentScript.turnSpeed = owner.parentScript.drainActiveTurnSpeed;
//        _timer = new Timer(_durration);
//    }

//    public override void ExitState(OldBossPhaseOneState owner)
//    {
//        owner.parentScript.bossAnimator.SetBool("drainActiveBool", false);
//        owner.parentScript.drainAttackHitboxGroup.enabled = false;
//        owner.parentScript.turnSpeed = owner.parentScript.defaultTurnSpeed;
//        owner.parentScript.animationEnded = false;
//    }

//    public override void UpdateState(OldBossPhaseOneState owner)
//    {
//        _timer.Time += Time.deltaTime;

//        owner.parentScript.FacePlayer();

//        //fixa så den går ut vid animationEnded, inte när timern är slut
//        if (_timer.Expired)
//        {
//            owner.phaseOneStateMashine.ChangeState(owner.phaseOneCombatState);
//        }
//    }
//}

//public class PhaseOneAOEAttackState : State<OldBossPhaseOneState>
//{
//    public override void EnterState(OldBossPhaseOneState owner)
//    {
//        owner.parentScript.agent.SetDestination(owner.parentScript.transform.position);

//        owner.parentScript.StartCoroutine(owner.parentScript.MurkyWaterCircleAbility(10, 6));
//        //owner.parentScript.StartCoroutine(owner.parentScript.MurkyWaterSpiralAbility(10, 8, 1.5f));
//        //owner.parentScript.StartCoroutine(owner.parentScript.MurkyWaterPolygonAbility(5, 9, 2));
//    }

//    public override void ExitState(OldBossPhaseOneState owner)
//    {
//        owner.parentScript.AOEAbilityOver = false;
//    }

//    public override void UpdateState(OldBossPhaseOneState owner)
//    {
//        if (owner.parentScript.AOEAbilityOver)
//        {
//            owner.phaseOneStateMashine.ChangeState(owner.phaseOneCombatState);
//        }
//    }
//}

//public class PhaseOneSpawnAbilityState : State<OldBossPhaseOneState>
//{
//    public override void EnterState(OldBossPhaseOneState owner)
//    {
//        owner.parentScript.agent.SetDestination(owner.parentScript.transform.position);

//        owner.parentScript.StartCoroutine(owner.parentScript.SpawnEnemyAbility(3f, owner.parentScript.enemySpawnList));
//    }

//    public override void ExitState(OldBossPhaseOneState owner)
//    {
//        owner.parentScript.SpawnAbilityOver = false;
//    }

//    public override void UpdateState(OldBossPhaseOneState owner)
//    {
//        if (owner.parentScript.SpawnAbilityOver)
//        {
//            owner.phaseOneStateMashine.ChangeState(owner.phaseOneCombatState);
//        }
//    }
//}

#region Basic Attack State
//public class Phase1Attack1State : State<OldBossPhaseOneState>
//{
//    private float _durration;
//    private Timer _timer;

//    public Phase1Attack1State(float durration)
//    {
//        _durration = durration;
//    }


//    public override void EnterState(OldBossPhaseOneState owner)
//    {
//        _timer = new Timer(_durration);
//    }

//    public override void ExitState(OldBossPhaseOneState owner)
//    {
//    }

//    public override void UpdateState(OldBossPhaseOneState owner)
//    {
//        _timer.Time += Time.deltaTime;

//        if (_timer.Expired)
//        {
//            owner.phaseOneStateMashine.ChangeState(owner.phaseOneCombatState);
//        }
//    }
//}
#endregion
#endregion