using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossAIScript : Entity
{
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

    //lägga till en speed change variabel, beroende på hur nära man e desiredDistance
    [Tooltip("Elementen MÅSTE vara i ordnade utifrån Desired Distance Offset (från störst till minst)")]
    [SerializeField] public DesiredDistanceToAngleValues[] desiredDistanceValues;

    [NonSerialized] public List<float> desiredDistanceOffsetValues = new List<float>();
    [NonSerialized] public List<float> desiredDistanceAngleValues = new List<float>();
    [NonSerialized] public List<float> desiredDistanceSpeedIncreseValues = new List<float>();

    public StateMachine<BossAIScript> phaseControllingStateMachine;

    public PreBossFightState preBossFightState = new PreBossFightState();
    public BossPhaseOneState bossPhaseOneState = new BossPhaseOneState();
    public BossPhaseTwoState bossPhaseTwoState = new BossPhaseTwoState();
    public BossDeadState bossDeadState = new BossDeadState();

    [SerializeField] public GameObject _fill;

    [SerializeField] public GameObject murkyWaterPrefab;

    [SerializeField] public LayerMask targetLayers;
    [SerializeField] public LayerMask dashCollisionLayers;

    //galet nog är alla variabler som heter test något, inte planerat att vara permanenta
    [SerializeField] [Range(0, 1)] public float phaseTwoTransitionHP = 0.5f;

    [SerializeField] public HitboxGroup meleeAttackHitboxGroup;
    [SerializeField] public HitboxGroup drainAttackHitboxGroup;

    //lägga till ranomness på attack speed, kan göra det med 2 randoms för att få en normal distribution
    #region Attack Speed Variables
    [SerializeField] public float minAttackSpeed = 5f;
    [SerializeField] public float attackSpeedIncreaseMax = 5f;
    [SerializeField] public float minAttackCooldown = 1f;
    #endregion

    #region Drain Variables
    [SerializeField] public float drainRange = 6f;
    [SerializeField] public float drainChargeTime = 7f;
    [SerializeField] public float drainAttackTime = 8f;
    #endregion

    #region Dash Variables
    [SerializeField] [Range(0, 10)] public float dashChansePerFrame = 0.1f;
    [SerializeField] [Range(0, 100)] public float dashAttackChanse = 20f;

    [SerializeField] public float dashSpeed = 20f;
    [SerializeField] public float dashDistance = 5f;
    //[SerializeField] public float dashDistanceMin = 5f;
    [SerializeField] public float dashLagDurration = 0.5f;
    [SerializeField] public float dashAcceleration = 2000f;
    [SerializeField] [Range(1, 180)] public float maxAngleDashAttackToPlayer = 90f;
    #endregion

    #region Chasing Variables
    [SerializeField] public float chasingSpeed = 5f;
    [SerializeField] public float chasingAcceleration = 20f;
    #endregion

    #region Misc Variables
    [SerializeField] public float aggroRange = 10f;
    [SerializeField] public float defaultTurnSpeed = 5f;
    [SerializeField] public float drainActiveTurnSpeed = 0.1f;

    [SerializeField] public float meleeRange = 6f;

    [SerializeField] public float desiredDistanceToPlayer = 5f;

    [SerializeField] public GameObject placeholoderDranBeam;
    #endregion


    [NonSerialized] public Vector3 idlePos;

    [NonSerialized] public float defaultSpeed;
    [NonSerialized] public float turnSpeed;
    //[NonSerialized] public float dashDistance;
    [NonSerialized] public bool dashAttack;

    [NonSerialized] public Vector3 movementDirection = new Vector3(0f, 0f, 1f);
    [NonSerialized] public Vector3 dashCheckOffsetVector;
    [NonSerialized] public Vector3 dashCheckBoxSize;

    [NonSerialized] public Animator bossAnimator;
    [NonSerialized] public NavMeshAgent agent;
    [NonSerialized] public GameObject player;


    [NonSerialized] public bool animationEnded;
    [NonSerialized] public bool facePlayerBool = true;

    void Awake()
    {
        phaseControllingStateMachine = new StateMachine<BossAIScript>(this);

        //borde inte göras såhär at the end of the day men måste göra skit med spelaren då och vet inte om jag får det
        player = GlobalState.state.PlayerGameObject;

        bossAnimator = GetComponent<Animator>();
        agent = GetComponentInParent<NavMeshAgent>();

        idlePos = transform.position;
        turnSpeed = defaultTurnSpeed;

        dashCheckOffsetVector = new Vector3(0f, 1f, dashDistance / 2);
        dashCheckBoxSize = new Vector3(0.75f, 0.75f, dashDistance / 2);
        defaultSpeed = agent.speed;
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
        phaseControllingStateMachine.ChangeState(preBossFightState);

        _fill.SetActive(false);
    }

    void Update()
    {
        phaseControllingStateMachine.Update();
    }

    public void FacePlayer()
    {
        Vector3 lookDirection = (player.transform.position - this.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(lookDirection.x, 0, lookDirection.z));
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, lookRotation, Time.deltaTime * turnSpeed);
    }

    public override void TakeDamage(HitboxValues hitbox, Entity attacker)
    {
        GetComponent<EnemyHealth>().Damage(hitbox.damageValue);
        //spela hurtljud här
    }

    public bool CheckDashPath(Vector3 dashCheckVector)
    {
        float dashCheckAngle = Vector3.SignedAngle(transform.forward, dashCheckVector.normalized, transform.up);

        dashCheckOffsetVector = Quaternion.AngleAxis(dashCheckAngle, Vector3.up) * dashCheckOffsetVector;

        return Physics.OverlapBox(transform.TransformPoint(dashCheckOffsetVector), dashCheckBoxSize, Quaternion.LookRotation(dashCheckVector.normalized), dashCollisionLayers).Length <= 0;
    }

    #region Murky Water Testing (AOE)
    public void MurkyWaterSpiralAbility(int layers, int poolsPerLayer, float spiralIntensity)
    {
        Vector3 spawnPos = Vector3.forward;
        //float rotationAmount
        for (int i = 0; i < layers; i++)
        {
            for (int j = 0; j < poolsPerLayer; j++)
            {
                spawnPos = Quaternion.AngleAxis(360f / poolsPerLayer + spiralIntensity, Vector3.up) * spawnPos;
                //print(testVec);
                SpawnMurkyWater(spawnPos * (i + 1));
            }
        }
    }
    
    public void MurkyWaterCircleAbility(int layers, int poolsPerLayer)
    {
        Vector3 spawnPos;
        float poolAmount = 0;
        for (int i = 0; i < layers; i++)
        {
            spawnPos = Vector3.forward;
            poolAmount += poolsPerLayer;
            for (int j = 0; j < poolAmount; j++)
            {
                spawnPos = Quaternion.AngleAxis(360f / poolAmount, Vector3.up) * spawnPos;
                //print(testVec);
                SpawnMurkyWater(spawnPos * (i + 1));
            }
        }
    }
    
    public void MurkyWaterPolygonAbility(int layers, int sides, int poolsPerSide)
    {
        Vector3 spawnPos;
        Vector3 currentCornerPos;
        Vector3 nextCornerPos;
        Vector3 cornerToCorner;

        float poolAmount = 0;
        for (int i = 0; i < layers; i++)
        {
            poolAmount += poolsPerSide;

            currentCornerPos = Vector3.forward * (i * 2 + 2);
            nextCornerPos = Quaternion.AngleAxis(360f / sides, Vector3.up) * currentCornerPos;

            for (int k = 0; k < sides; k++)
            {
                cornerToCorner = nextCornerPos - currentCornerPos;
                spawnPos = currentCornerPos;
                for (int j = 0; j < poolAmount; j++)
                {
                    SpawnMurkyWater(spawnPos);
                    spawnPos += cornerToCorner.normalized * (cornerToCorner.magnitude / poolAmount);
                }
                currentCornerPos = nextCornerPos;
                nextCornerPos = Quaternion.AngleAxis(360f / sides, Vector3.up) * nextCornerPos;
            }
        }
    }

    //public void drainBeamTest()
    //{
    //    Physics.BoxCast(transform.TransformPoint(new Vector3(0f, 1f, 0f)), new Vector3 ()  );

    //}


    //hur fan ska det funka med olika Y nivå? får typ raycasta upp och ner när den spawnas för att hitta vart marken är och sen flytta den dit och rotera den baserat på normalen eller något, låter jobbigt :(
    public void SpawnMurkyWater(Vector3 spawnPositionOffset, float timeToLive = 0f)
    {
        GameObject murkyWater = (GameObject)Instantiate(murkyWaterPrefab, transform.TransformPoint(spawnPositionOffset), Quaternion.identity);
        if (timeToLive > 0.01f)
        {
            murkyWater.GetComponentInChildren<MurkyWaterScript>().timeToLive = timeToLive;

            //print("spawned murky water for " + timeToLive);
        }
        else
        {
            //print("spawned murky water for ever >:) " + timeToLive);
        }
    }

    //hur fan ska det funka med olika Y nivå? får typ raycasta upp och ner när den spawnas för att hitta vart marken är och sen flytta den dit och rotera den baserat på normalen eller något, låter jobbigt :(
    public void SpawnMurkyWater(float timeToLive = 0f)
    {
        GameObject murkyWater = (GameObject)Instantiate(murkyWaterPrefab, transform.position, Quaternion.identity);
        if (timeToLive > 0.1f)
        {
            murkyWater.GetComponentInChildren<MurkyWaterScript>().timeToLive = timeToLive;

            //print("spawned murky water for " + timeToLive);
        }
        else
        {
            //print("spawned murky water for ever >:) " + timeToLive);
        }
    }
    #endregion

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRange);

        Gizmos.color = Color.blue;

        Vector3 dashCheckOffsetVectorGizmo = new Vector3(0f, 1f, dashDistance / 2);
        Vector3 dashCheckBoxSizeGizmo = new Vector3(1.5f, 1.5f, dashDistance);

        float dashCheckAngleGizmo = Vector3.SignedAngle(transform.forward, movementDirection, transform.up);

        dashCheckOffsetVectorGizmo = Quaternion.AngleAxis(dashCheckAngleGizmo, Vector3.up) * dashCheckOffsetVectorGizmo;

        Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(dashCheckOffsetVectorGizmo), Quaternion.LookRotation(movementDirection.normalized), transform.localScale);
        Gizmos.DrawWireCube(Vector3.zero, dashCheckBoxSizeGizmo);
    }

    #region Animation attack events
    
    //Generella
    public void EndAnimation()
    {
        animationEnded = true;
    }

    public void DontFacePlayer()
    {
        facePlayerBool = false;
        //print("rotation stopped");
    }
    //P1

    public void EnableDrainBeam()
    {
        placeholoderDranBeam.SetActive(true);
    }
    
    public void DisableDrainBeam()
    {
        placeholoderDranBeam.SetActive(false);
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
}

public class PreBossFightState : State<BossAIScript>
{
    private RaycastHit _hit;

    public override void EnterState(BossAIScript owner)
    {
        owner.transform.position = owner.idlePos;
        //set hp till maxhp här?
        owner.bossAnimator.SetBool("idleBool", true);
    }

    public override void ExitState(BossAIScript owner)
    {
        //kanske köra någon funktion som stänger en dörr eller något så man inte kan springa iväg
        owner.bossAnimator.SetBool("idleBool", false);
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
                    owner.phaseControllingStateMachine.ChangeState(owner.bossPhaseOneState);
                }
            }
        }
    }
}

//////////////////
//PHASE 1 STATES//
//////////////////

//drain (charge, shoot, stay)
//idle/movement/bestämma nästa attack (gå runt lite, vara vänd mot spelaren, bestämma vilket state man ska in i sen, alla states går in i detta state)
//slå attack (om nära -> slå, typ?)
//dash (dasha, fast vart?) (dash attack, random dash(kan bli dåligt), dash om nära, dash om wiff)

//vill nog lägga till något transition state där någon aggro/bossfighten börjar nu annimation spelas


#region Phase 1 States
public class BossPhaseOneState : State<BossAIScript>
{

    public BossAIScript parentScript;

    public StateMachine<BossPhaseOneState> phaseOneStateMashine;


    public Phase1Attack1State phase1Attack1State;

    public PhaseOneCombatState phaseOneCombatState;
    public PhaseOneChargeDrainAttackState phaseOneChargeDrainAttackState;
    public PhaseOneActiveDrainAttackState phaseOneActiveDrainAttackState;
    public PhaseOneMeleeAttackOneState phaseOneMeleeAttackOneState;
    public PhaseOneDashState phaseOneDashState;
    public PhaseOneChaseToAttackState phaseOneChaseToAttackState;
    public override void EnterState(BossAIScript owner)
    {
        //fixa detta i konstruktor kanske?
        phaseOneStateMashine = new StateMachine<BossPhaseOneState>(this);

        phase1Attack1State = new Phase1Attack1State(owner.drainRange, owner.drainChargeTime);

        phaseOneCombatState = new PhaseOneCombatState(owner.minAttackSpeed, owner.attackSpeedIncreaseMax,  owner.minAttackCooldown, owner.meleeRange, owner.drainRange, owner);
        phaseOneDashState = new PhaseOneDashState(owner.dashSpeed, owner.dashDistance, owner.dashLagDurration, owner.dashAcceleration);
        phaseOneChaseToAttackState = new PhaseOneChaseToAttackState();


        phaseOneChargeDrainAttackState = new PhaseOneChargeDrainAttackState(owner.drainChargeTime);
        phaseOneActiveDrainAttackState = new PhaseOneActiveDrainAttackState(owner.drainAttackTime);
        phaseOneMeleeAttackOneState = new PhaseOneMeleeAttackOneState(owner.meleeRange, owner.drainAttackTime, owner.drainChargeTime);

        parentScript = owner;

        phaseOneStateMashine.ChangeState(phaseOneCombatState);

        //spela cool animation :)

        //owner.MurkyWaterSpiralAbility(10, 6, 2f);
        //owner.MurkyWaterCircleAbility(10, 6);
        //owner.MurkyWaterCircleAbility(10, 1);
        //owner.MurkyWaterPolygonAbility(5, 4, 2);

    }

    public override void ExitState(BossAIScript owner)
    {

    }

    public override void UpdateState(BossAIScript owner)
    {
        //kolla om man ska gå över till nästa phase
        //if ((owner.GetComponent<EnemyHealth>().GetHealth() / owner.GetComponent<EnemyHealth>().GetMaxHealth()) < owner.testP2TransitionHP)
        //{
        //    owner.phaseControllingStateMachine.ChangeState(owner.bossPhaseTwoState);
        //}
        
        //kan skapa problem (?)
        if (owner.GetComponent<EnemyHealth>().GetHealth() <= 0)
        {
            owner.phaseControllingStateMachine.ChangeState(owner.bossDeadState);
        }

        phaseOneStateMashine.Update();
    }
}

//vet inte om allt detta typ egentligen borde göras i parent statet (borde typ det tror jag)
public class PhaseOneCombatState : State<BossPhaseOneState>
{
    private Timer _timer;
    private float _minAttackSpeed;
    private float _attackSpeedIncreaseMax;
    private float _attackSpeed;
    private float _baseMinAttackCooldown;
    private float _minAttackCooldown;
    private float _meleeAttackRange;
    private float _drainAttackRange;

    private Vector3 _destination;
    //private Vector3 _direction;
    private Vector3 _dashAttackDirection;
    private float _dashAttackAngle;

    private Vector3 _bossToPlayer;

    private RaycastHit _hit;

    private BossAIScript _ownerParentScript;

    public PhaseOneCombatState(float minAttackSpeed, float attackSpeedIncreaseMax, float minAttackCooldown, float meleeAttackRange, float drainAttackRange, BossAIScript ownerParentScript)
    {
        _minAttackSpeed = minAttackSpeed;
        _attackSpeedIncreaseMax = attackSpeedIncreaseMax/2;
        _baseMinAttackCooldown = minAttackCooldown;
        _meleeAttackRange = meleeAttackRange;
        _drainAttackRange = drainAttackRange;

        _ownerParentScript = ownerParentScript;

        GenarateNewAttackSpeed();
        _timer = new Timer(_attackSpeed);
    }

    public override void EnterState(BossPhaseOneState owner)
    {
        //Debug.Log("in i PhaseOneCombatState");
        if (_timer.Expired)
        {
            GenarateNewAttackSpeed();
            _timer = new Timer(_attackSpeed);
            _minAttackCooldown = _baseMinAttackCooldown;
        }
        else
        {
            _minAttackCooldown += _timer.Time;
        }
    }

    private void GenarateNewAttackSpeed()
    {
        _attackSpeed = _minAttackSpeed;
        _attackSpeed += UnityEngine.Random.Range(0f, _attackSpeedIncreaseMax);
        _attackSpeed += UnityEngine.Random.Range(0f, _attackSpeedIncreaseMax);
    }

    public override void ExitState(BossPhaseOneState owner)
    {
        //Debug.Log("hej då PhaseOneCombatState");
        _ownerParentScript.agent.SetDestination(_ownerParentScript.transform.position);
    }

    //tycker synd om er om ni behöver kolla i denna update (:
    public override void UpdateState(BossPhaseOneState owner)
    {
        _ownerParentScript.FacePlayer();

        _timer.Time += Time.deltaTime;

        //kanske borde dela upp detta i olika movement states pga animationer men vet inte om det behövs

        //kolla om spelaren är nära nog att slå
        if (_timer.Time > _minAttackCooldown && _meleeAttackRange > Vector3.Distance(_ownerParentScript.transform.position, _ownerParentScript.player.transform.position))
        {
            //kanske göra AOE attack här för att tvinga iväg spelaren?
            owner.phaseOneStateMashine.ChangeState(owner.phaseOneMeleeAttackOneState);
        }
        //kolla om man ska attackera
        else if (_timer.Expired && _timer.Time > _minAttackCooldown)
        {
            //nära nog för att göra melee attacken
            if (_drainAttackRange > Vector3.Distance(_ownerParentScript.transform.position, _ownerParentScript.player.transform.position))
            {
                //vill den dash attacka?
                if (UnityEngine.Random.Range(0f, 100f) > 100f - _ownerParentScript.dashAttackChanse)
                {
                    _bossToPlayer = _ownerParentScript.player.transform.position - _ownerParentScript.transform.position;

                    Physics.Raycast(_ownerParentScript.transform.position + new Vector3(0, 1, 0), _bossToPlayer.normalized, out _hit, _ownerParentScript.dashDistance + _meleeAttackRange, _ownerParentScript.targetLayers);

                    //är spelaren innom en bra range och innom LOS?
                    //if (_hit.transform == _ownerParentScript.player.transform && _bossToPlayer.magnitude < _ownerParentScript.dashDistanceMax + _meleeAttackRange / 2 && _bossToPlayer.magnitude > _ownerParentScript.dashDistanceMax - _meleeAttackRange / 2)
                    if (_hit.transform == _ownerParentScript.player.transform && _bossToPlayer.magnitude < _ownerParentScript.dashDistance + _meleeAttackRange / 2 && _bossToPlayer.magnitude > _ownerParentScript.dashDistance - _meleeAttackRange / 2)
                    {
                        //Debug.Log("_bossToPlayer " + _bossToPlayer.magnitude);

                        int dashSign = 0;

                        if (UnityEngine.Random.Range(0f, 1f) > 0.5f)
                        {
                            dashSign = 1;
                        }
                        else
                        {
                            dashSign = -1;
                        }

                        _dashAttackAngle = Mathf.Rad2Deg * Mathf.Acos((Mathf.Pow(_bossToPlayer.magnitude, 2) + Mathf.Pow(_ownerParentScript.dashDistance, 2) - Mathf.Pow(_meleeAttackRange / 2, 2)) / (2 * _bossToPlayer.magnitude * _ownerParentScript.dashDistance));

                        _dashAttackDirection = _bossToPlayer;

                        _dashAttackDirection = Quaternion.AngleAxis(_dashAttackAngle * dashSign, Vector3.up) * _dashAttackDirection;

                        Vector3 _playerToDashPos = (_ownerParentScript.transform.position + _dashAttackDirection.normalized * _ownerParentScript.dashDistance) - _ownerParentScript.player.transform.position;

                        Vector3 _playerToBoss = _ownerParentScript.transform.position - _ownerParentScript.player.transform.position;

                        float _angleDashAttackToPlayer = Vector3.Angle(_playerToBoss, _playerToDashPos);

                        //är vinkeln mellan spelaren till dit bossen kommer dasha en ok vinkel 
                        if (_angleDashAttackToPlayer < _ownerParentScript.maxAngleDashAttackToPlayer)
                        {
                            //ändra så det inte är en siffra utan att det beror på deras hittboxes storlek eller en parameter

                            //ranomizar vart bossen kommer dasha, sålänge den inte skulle kunna krocka med spelaren
                            if (_bossToPlayer.magnitude - 0.45f > _ownerParentScript.dashDistance)
                            {
                                _dashAttackAngle = UnityEngine.Random.Range(0, _dashAttackAngle);
                                _dashAttackDirection = _bossToPlayer;
                                _dashAttackDirection = Quaternion.AngleAxis(_dashAttackAngle * dashSign * -1, Vector3.up) * _dashAttackDirection;
                            }
                            //är det något i vägen för dashen?
                            if (_ownerParentScript.CheckDashPath(_dashAttackDirection))
                            {
                                _ownerParentScript.movementDirection = _dashAttackDirection;
                                _ownerParentScript.dashAttack = true;
                                owner.phaseOneStateMashine.ChangeState(owner.phaseOneDashState);
                            }
                            else
                            {
                                _dashAttackDirection = _bossToPlayer;
                                //"*-1" för att få andra sidan av spelaren
                                _dashAttackDirection = Quaternion.AngleAxis(_dashAttackAngle * dashSign * -1, Vector3.up) * _dashAttackDirection;

                                //är något i vägen om den dashar till andra sidan av spelaren?
                                if (_ownerParentScript.CheckDashPath(_dashAttackDirection))
                                {
                                    _ownerParentScript.movementDirection = _dashAttackDirection;
                                    _ownerParentScript.dashAttack = true;
                                    owner.phaseOneStateMashine.ChangeState(owner.phaseOneDashState);
                                }
                                //saker va i vägen för dashen
                                else
                                {
                                    Debug.Log("kan inte dasha med all denna skit ivägen juuuuuöööööö");

                                    owner.phaseOneStateMashine.ChangeState(owner.phaseOneChaseToAttackState);
                                }
                            }
                        }
                        else
                        {
                            Debug.Log("no want dash tru player");
                            owner.phaseOneStateMashine.ChangeState(owner.phaseOneChaseToAttackState);
                        }
                    }
                    else
                    {
                        Debug.Log("ITS TO FAR AWAY!!!! (or to close)");
                        owner.phaseOneStateMashine.ChangeState(owner.phaseOneChaseToAttackState);
                    }
                }
                //springa och slå
                else
                {
                    Debug.Log("no want dash tyvm");
                    owner.phaseOneStateMashine.ChangeState(owner.phaseOneChaseToAttackState);
                }
            }
            //drain
            else
            {
                owner.phaseOneStateMashine.ChangeState(owner.phaseOneChargeDrainAttackState);
            }
        }
        //idle movement
        else
        {
            //flytta till fixed uppdate (kanske)
            Physics.Raycast(_ownerParentScript.transform.position + new Vector3(0, 1, 0), (_ownerParentScript.player.transform.position - _ownerParentScript.transform.position).normalized, out _hit, Mathf.Infinity, _ownerParentScript.targetLayers);

            //om bossen kan se spelaren
            if (_hit.transform == _ownerParentScript.player.transform)
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

                _ownerParentScript.movementDirection = _ownerParentScript.transform.right;

                //lägga till någon randomness variabel så movement inte blir lika predictable? (kan fucka animationerna?)
                //kanske slurpa mellan de olika värdena (kan bli jobbigt och vet inte om det behövs)
                float compairValue = Vector3.Distance(_ownerParentScript.transform.position, _ownerParentScript.player.transform.position);

                for (int i = 0; i < _ownerParentScript.desiredDistanceValues.Length; i++)
                {
                    if (compairValue > _ownerParentScript.desiredDistanceToPlayer + _ownerParentScript.desiredDistanceOffsetValues[i])
                    {
                        _ownerParentScript.movementDirection = Quaternion.AngleAxis(_ownerParentScript.desiredDistanceAngleValues[i] * strafeSign * -1, Vector3.up) * _ownerParentScript.movementDirection;
                        _ownerParentScript.movementDirection *= strafeSign;
                        _ownerParentScript.agent.speed = _ownerParentScript.defaultSpeed + _ownerParentScript.desiredDistanceSpeedIncreseValues[i];
                        break;
                    }
                }

                //random dash in combat
                if (UnityEngine.Random.Range(0f, 100f) > 100f - _ownerParentScript.dashChansePerFrame)
                {
                    if (_ownerParentScript.CheckDashPath(_ownerParentScript.movementDirection))
                    {
                        owner.phaseOneStateMashine.ChangeState(owner.phaseOneDashState);
                    }
                    else
                    {
                        Debug.Log("kunde inte dasha för saker va i vägen");
                    }
                }
                else
                {
                    //ändra 5an till typ destinationAmplifier
                    _destination = _ownerParentScript.transform.position + _ownerParentScript.movementDirection * 5;
                    _ownerParentScript.agent.SetDestination(_destination);
                }
            }
            //om bossen inte kan se spelaren
            else
            {
                _destination = _ownerParentScript.player.transform.position;
                _ownerParentScript.agent.SetDestination(_destination);
            }
        }
    }
}

public class PhaseOneDashState : State<BossPhaseOneState>
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


    public PhaseOneDashState(float speed, float distance, float lagDurration, float acceleration)
    {
        _dashSpeed = speed;
        _dashDistance = distance;
        _lagDurration = lagDurration;
        _dashAcceleration = acceleration;
    }


    public override void EnterState(BossPhaseOneState owner)
    {
        //if (owner.parentScript.dashAttack)
        //{
        //    //owner.parentScript.bossAnimator.SetTrigger("dashForwardTrigger");
        //    //_dashDistance = owner.parentScript.dashDistance;
        //}
        //else
        //{
        //    //_dashDistance = owner.parentScript.dashDistanceMax;
        //}


        _oldSpeed = owner.parentScript.agent.speed;
        _oldAcceleration = owner.parentScript.agent.acceleration;

        _dashDurration = (_dashDistance - owner.parentScript.agent.stoppingDistance) / _dashSpeed;
        //Debug.Log("zoom for, " + _dashDurration + " MPH, " + _dashSpeed);
        //Debug.Log(_dashDistance + " " + owner.bossPhaseOneParentScript.agent.stoppingDistance + " " + _dashSpeed);
        _dashTimer = new Timer(_dashDurration);
        _lagTimer = new Timer(_lagDurration);

        owner.parentScript.agent.speed = _dashSpeed;
        owner.parentScript.agent.acceleration = _dashAcceleration;

        _dashDirection = owner.parentScript.movementDirection.normalized;
        _dashDestination = owner.parentScript.transform.position + _dashDirection * _dashDistance;

        owner.parentScript.agent.SetDestination(_dashDestination);
    }

    public override void ExitState(BossPhaseOneState owner)
    {
        owner.parentScript.agent.speed = _oldSpeed;
        owner.parentScript.agent.acceleration = _oldAcceleration;
        owner.parentScript.agent.SetDestination(owner.parentScript.transform.position);
        //Debug.Log("hej då zoom");
    }

    public override void UpdateState(BossPhaseOneState owner)
    {
        _dashTimer.Time += Time.deltaTime;

        if (_dashTimer.Expired)
        {
            _lagTimer.Time += Time.deltaTime;

            if (owner.parentScript.dashAttack)
            {
                owner.parentScript.dashAttack = false;
                owner.phaseOneStateMashine.ChangeState(owner.phaseOneMeleeAttackOneState);
            }
            else if (_lagTimer.Expired)
            {
                owner.phaseOneStateMashine.ChangeState(owner.phaseOneCombatState);
            }
        }
    }
}

public class PhaseOneMeleeAttackOneState : State<BossPhaseOneState>
{
    private float _range;
    private float _totalDurration;
    private float _chargeTime;


    public PhaseOneMeleeAttackOneState( float range, float attackTime, float chargeTime)
    {
        _range = range;
        _chargeTime = chargeTime;
        _totalDurration = chargeTime + attackTime;
    }


    public override void EnterState(BossPhaseOneState owner)
    {
        owner.parentScript.bossAnimator.SetTrigger("melee1Trigger");
        owner.parentScript.meleeAttackHitboxGroup.enabled = true;
        //Debug.Log("nu ska jag fan göra PhaseOneMeleeAttackState >:(, med dessa stats:  damage " + _damage + " range " + _range + " totalDurration " + _totalDurration + " chargeTime " + _chargeTime);
    }

    public override void ExitState(BossPhaseOneState owner)
    {
        //Debug.Log("hej då PhaseOneMeleeAttackState");
        owner.parentScript.meleeAttackHitboxGroup.enabled = false;
        owner.parentScript.animationEnded = false;
        owner.parentScript.facePlayerBool = true;
    }

    public override void UpdateState(BossPhaseOneState owner)
    {
        if (owner.parentScript.animationEnded)
        {
            owner.phaseOneStateMashine.ChangeState(owner.phaseOneCombatState);
        }
        else
        {
            if (owner.parentScript.facePlayerBool)
            {
                //spela attackljud här
                owner.parentScript.FacePlayer();
            }
        }
    }
}

public class PhaseOneChaseToAttackState : State<BossPhaseOneState>
{
    private Vector3 _playerPos;
    private float _distanceToPlayer;
    private float _oldSpeed;
    private float _oldAcceleration;


    public override void EnterState(BossPhaseOneState owner)
    {
        _oldSpeed = owner.parentScript.agent.speed;
        _oldAcceleration = owner.parentScript.agent.acceleration;

        owner.parentScript.agent.speed = owner.parentScript.chasingSpeed;
        owner.parentScript.agent.acceleration = owner.parentScript.chasingAcceleration;
        owner.parentScript.bossAnimator.SetTrigger("runningTrigger");
    }

    public override void ExitState(BossPhaseOneState owner)
    {
        owner.parentScript.agent.speed = _oldSpeed;
        owner.parentScript.agent.acceleration = _oldAcceleration;

        owner.parentScript.agent.SetDestination(owner.parentScript.transform.position);
    }

    public override void UpdateState(BossPhaseOneState owner)
    {
        owner.parentScript.FacePlayer();

        _playerPos = owner.parentScript.player.transform.position;
        _distanceToPlayer = Vector3.Distance(owner.parentScript.transform.position, _playerPos);

        if (_distanceToPlayer > owner.parentScript.drainRange)
        {
            owner.phaseOneStateMashine.ChangeState(owner.phaseOneChargeDrainAttackState);
        }
        else if (_distanceToPlayer < owner.parentScript.meleeRange)
        {
            owner.phaseOneStateMashine.ChangeState(owner.phaseOneMeleeAttackOneState);
        }
        else
        {
            owner.parentScript.agent.SetDestination(_playerPos);
        }
    }
}

public class PhaseOneChargeDrainAttackState : State<BossPhaseOneState>
{
    private float _chargeTime;
    private Timer _timer;

    public PhaseOneChargeDrainAttackState(float chargeTime)
    {
        _chargeTime = chargeTime;
    }

    public override void EnterState(BossPhaseOneState owner)
    {
        owner.parentScript.bossAnimator.SetTrigger("drainStartTrigger");
        //Debug.Log("nu ska jag fan göra PhaseOneChargeDrainAttackState >:(, med dessa stats: chargeTime " + _chargeTime);
        _timer = new Timer(_chargeTime);
    }

    public override void ExitState(BossPhaseOneState owner)
    {
        //Debug.Log("hej då PhaseOneChargeDrainAttackState");
    }

    public override void UpdateState(BossPhaseOneState owner)
    {
        _timer.Time += Time.deltaTime;

        if (_timer.Expired)
        {
            owner.phaseOneStateMashine.ChangeState(owner.phaseOneActiveDrainAttackState);
        }
        else if (owner.parentScript.facePlayerBool)
        {
            owner.parentScript.FacePlayer();
        }
    }
}

//dela upp detta state så man kan hålla animationen
public class PhaseOneActiveDrainAttackState : State<BossPhaseOneState>
{
    private float _durration;
    private Timer _timer;

    public PhaseOneActiveDrainAttackState(float durration)
    {
        _durration = durration;
    }

    public override void EnterState(BossPhaseOneState owner)
    {
        owner.parentScript.bossAnimator.SetBool("drainActiveBool", true);
        owner.parentScript.drainAttackHitboxGroup.enabled = true;
        owner.parentScript.turnSpeed = owner.parentScript.drainActiveTurnSpeed;
        //Debug.Log("nu ska jag fan göra PhaseOneActiveDrainAttackState >:(, med dessa stats:  damage " + _damagePerSecond + " range " + _range + " durration " + _durration);
        _timer = new Timer(_durration);
    }

    public override void ExitState(BossPhaseOneState owner)
    {
        //Debug.Log("hej då PhaseOneActiveDrainAttackState");
        owner.parentScript.bossAnimator.SetBool("drainActiveBool", false);
        owner.parentScript.drainAttackHitboxGroup.enabled = false;
        owner.parentScript.turnSpeed = owner.parentScript.defaultTurnSpeed;
        owner.parentScript.animationEnded = false;
        owner.parentScript.facePlayerBool = true;
    }

    public override void UpdateState(BossPhaseOneState owner)
    {
        _timer.Time += Time.deltaTime;

        if (owner.parentScript.facePlayerBool)
        {
            owner.parentScript.FacePlayer();
        }
        if (_timer.Expired)
        {
            owner.phaseOneStateMashine.ChangeState(owner.phaseOneCombatState);
        }
    }
}

#region Basic Attack State
public class Phase1Attack1State : State<BossPhaseOneState>
{
    private float _range;
    private float _durration;
    private Timer _timer;

    public Phase1Attack1State(float range, float durration)
    {
        _range = range;
        _durration = durration;
    }


    public override void EnterState(BossPhaseOneState owner)
    {
        Debug.Log("nu ska jag fan göra Phase1Attack1 >:(, med dessa stats: range " + _range + " durration " + _durration);
        _timer = new Timer(_durration);
    }

    public override void ExitState(BossPhaseOneState owner)
    {
        Debug.Log("hej då Phase1Attack1");
    }

    public override void UpdateState(BossPhaseOneState owner)
    {
        //kör cool animation som bestämmer när attacken är över istället för durration

        _timer.Time += Time.deltaTime;

        if (_timer.Expired)
        {
            owner.phaseOneStateMashine.ChangeState(owner.phaseOneCombatState);
        }
    }
}
#endregion

#endregion

//////////////////
//PHASE 2 STATES//
//////////////////
//används inte atm
#region Phase 2 States
public class BossPhaseTwoState : State<BossAIScript>
{

    public override void EnterState(BossAIScript owner)
    {

    }

    public override void ExitState(BossAIScript owner)
    {

    }

    public override void UpdateState(BossAIScript owner)
    {
        Debug.Log("nu chillar vi i Phase 2 :)");
    }
}
#endregion

#region Dead state
public class BossDeadState : State<BossAIScript>
{

    public override void EnterState(BossAIScript owner)
    {
        owner.bossAnimator.SetBool("deathBool", true);
        owner.GetComponent<HitboxEventHandler>().DisableHitboxes(0);
        owner.GetComponent<HitboxEventHandler>().EndAnim();
    }

    public override void ExitState(BossAIScript owner)
    {

    }

    public override void UpdateState(BossAIScript owner)
    {
        if (owner.animationEnded)
        {
            GameObject.Destroy(owner.gameObject);
            //owner.Destroy(owner.gameObject);
        }
    }
}
#endregion