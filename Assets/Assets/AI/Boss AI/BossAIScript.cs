using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossAIScript : MonoBehaviour
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
    }

    [Tooltip("Elementen MÅSTE vara i ordnade utifrån Desired Distance Offset (från störst till minst)")]
    [SerializeField] public DesiredDistanceToAngleValues[] desiredDistanceToAngleValues;

    [NonSerialized] public List<float> desiredDistanceOffsetValues = new List<float>();
    [NonSerialized] public List<float> desiredDistanceAngleValues = new List<float>();


    //[NonSerialized] public Dictionary<float, float> desierdDistanceOffsetToAngleDictionary = new Dictionary<float, float>();



    public StateMachine<BossAIScript> phaseControllingStateMachine;


    public PreBossFightState preBossFightState = new PreBossFightState();
    public BossPhaseOneState bossPhaseOneState = new BossPhaseOneState();
    public BossPhaseTwoState bossPhaseTwoState = new BossPhaseTwoState();


    [SerializeField] public LayerMask targetLayers;
    [SerializeField] public LayerMask dashCollisionLayers;

    //galet nog är alla variabler som heter test något, inte planerat att vara permanenta
    [SerializeField] public float testMaxHP = 500f;
    [SerializeField] [Range(0, 1)] public float testP2TransitionHP = 0.5f;

    [SerializeField] public float testAttackSpeed = 5f;
    [SerializeField] public float testMinAttackCooldown = 1f;

    [SerializeField] public float testDrainDPS = 5f;
    [SerializeField] public float testDrainRange = 6f;

    [SerializeField] public float testDrainChargeTime = 7f;
    [SerializeField] public float testDrainAttackTime = 8f;

    [SerializeField] public float testMeleeRange = 6f;

    [SerializeField] public float desiredDistanceToPlayer = 5f;

    [SerializeField] [Range(0, 10f)] public float testDashChansePerFrame = 0.1f;
    [SerializeField] [Range(0, 100f)] public float testDashAttackChanse = 20f;

    [SerializeField] public float testDashSpeed = 20f;
    [SerializeField] public float testDashDistance = 5f;
    [SerializeField] public float testDashLagDurration = 0.5f;
    [SerializeField] public float testDashAcceleration = 2000f;



    [SerializeField] public float aggroRange = 10f;
    [SerializeField] public float defaultTurnSpeed = 5f;


    //borde vara nonserialized men har den som serialized för testning
    /*[NonSerialized]*/
    public float testCurrentHP;

    [NonSerialized] public Animator bossAnimator;
    [NonSerialized] public float turnSpeed;

    //[SerializeField] public State<BossPhaseOneState>[] stateArray;


    [NonSerialized] public Vector3 movementDirection = new Vector3(0f, 0f, 1f);
    [NonSerialized] public Vector3 dashCheckOffsetVector;
    [NonSerialized] public float dashCheckAngle;
    [NonSerialized] public Vector3 dashCheckBoxSize;

    [NonSerialized] public bool dashAttack;



    [NonSerialized] public NavMeshAgent agent;
    [NonSerialized] public GameObject player;


    [NonSerialized] public bool animationEnded;
    [NonSerialized] public bool facePlayer;
    [NonSerialized] public bool drainHitboxActive;
    [NonSerialized] public bool meleeHitboxActive;

    void Awake()
    {
        phaseControllingStateMachine = new StateMachine<BossAIScript>(this);

        //borde inte göras såhär at the end of the day men måste göra skit med spelaren då och vet inte om jag får det
        player = GameObject.FindGameObjectWithTag("Player");

        testCurrentHP = testMaxHP;
        testDrainDPS = phaseOneStats.testP1value1;

        bossAnimator = GetComponent<Animator>();
        agent = GetComponentInParent<NavMeshAgent>();

        turnSpeed = defaultTurnSpeed;

        dashCheckOffsetVector = new Vector3(0f, 1f, testDashDistance / 2);
        dashCheckBoxSize = new Vector3(0.75f, 0.75f, testDashDistance / 2);
    }

    void Start()
    {

        for (int i = 0; i < desiredDistanceToAngleValues.Length; i++)
        {
            desiredDistanceOffsetValues.Add(desiredDistanceToAngleValues[i].desiredDistanceOffset);
            desiredDistanceAngleValues.Add(desiredDistanceToAngleValues[i].angle);

            //print("added " + desiredDistanceAngleValues[i] + " with " + desiredDistanceOffsetValues[i]);

            //desierdDistanceOffsetToAngleDictionary.Add(desierdDistanceToAngleValues[i].desierdDistanceOffset, desierdDistanceToAngleValues[i].angle);
        }

        agent.updateRotation = false;
        phaseControllingStateMachine.ChangeState(preBossFightState);
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

    public void TakeDamage(float damage)
    {
        testCurrentHP -= damage;
    }

    public bool CheckDashPath(Vector3 dashCheckVector)
    {
        //dashCheckAngle = Vector3.SignedAngle(transform.forward, movementDirection, transform.up);

        //dashCheckOffsetVector = Quaternion.AngleAxis(dashCheckAngle, Vector3.up) * dashCheckOffsetVector;

        //return Physics.OverlapBox(transform.TransformPoint(dashCheckOffsetVector), dashCheckBoxSize, Quaternion.LookRotation(movementDirection.normalized), dashCollisionLayers).Length <= 0;

        dashCheckAngle = Vector3.SignedAngle(transform.forward, dashCheckVector.normalized, transform.up);

        dashCheckOffsetVector = Quaternion.AngleAxis(dashCheckAngle, Vector3.up) * dashCheckOffsetVector;

        return Physics.OverlapBox(transform.TransformPoint(dashCheckOffsetVector), dashCheckBoxSize, Quaternion.LookRotation(dashCheckVector.normalized), dashCollisionLayers).Length <= 0;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRange);


        Gizmos.color = Color.blue;

        Vector3 dashCheckOffsetVectorGizmo = new Vector3(0f, 1f, testDashDistance / 2);
        //Vector3 testDashDir = new Vector3(-0.7f, 0f, 1f);
        //Vector3 testDashDir = movementDirection;
        Vector3 dashCheckBoxSizeGizmo = new Vector3(1.5f, 1.5f, testDashDistance);

        float dashCheckAngleGizmo = Vector3.SignedAngle(transform.forward, movementDirection, transform.up);

        dashCheckOffsetVectorGizmo = Quaternion.AngleAxis(dashCheckAngleGizmo, Vector3.up) * dashCheckOffsetVectorGizmo;

        Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(dashCheckOffsetVectorGizmo), Quaternion.LookRotation(movementDirection.normalized), transform.localScale);
        Gizmos.DrawWireCube(Vector3.zero, dashCheckBoxSizeGizmo);

        //Physics.OverlapBox(transform.TransformPoint(dashCheckOffsetVectorGizmo), new Vector3(1f, 1.5f, testDashDistance) /** 0.5f*/, Quaternion.LookRotation(movementDirection.normalized), dashCollisionLayers);

        //Physics.BoxCast(_ownerParentScript.transform.position, new Vector3(1f, 2.5f, _ownerParentScript.testDashDistance / 2), _ownerParentScript.movementDirection, Quaternion.identity, 2f, _ownerParentScript.dashCollisionLayers);

        //Gizmos.matrix = Matrix4x4.TRS(/*transform.position +*/ transform.TransformPoint(new Vector3(0f, 1f, testDashDistance/(2 * transform.localScale.z))), transform.rotation, transform.localScale);
        //Gizmos.DrawWireCube(Vector3.zero, new Vector3(1f, 1f, testDashDistance/transform.localScale.z));

    }

    //Animation attack events
    #region Animation attack events
    //Generella
    public void TestBossEvent1()
    {
        print("hahahhahahha 1");
    }

    public void EndAnimation()
    {
        animationEnded = true;
    }

    public void DontFacePlayer()
    {
        facePlayer = false;
        //print("rotation stopped");
    }
    //P1
    public void FlipDrainHitboxActivation()
    {
        drainHitboxActive = !drainHitboxActive;
    }
    public void FlipMeleeHitboxActivation()
    {
        meleeHitboxActive = !meleeHitboxActive;
    }

    #endregion


}

public class PreBossFightState : State<BossAIScript>
{
    private RaycastHit _hit;

    public override void EnterState(BossAIScript owner)
    {

    }

    public override void ExitState(BossAIScript owner)
    {
        //kanske köra någon funktion som stänger en dörr eller något så man inte kan springa iväg
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



#region Phase 1 States
public class BossPhaseOneState : State<BossAIScript>
{

    public BossAIScript bossPhaseOneParentScript;

    public StateMachine<BossPhaseOneState> phaseOneStateMashine;


    public Phase1Attack1State phase1Attack1State;

    public PhaseOneCombatState phaseOneCombatState;
    public PhaseOneDrainAttackState phaseOneDrainAttackState;
    public PhaseOneMeleeAttackOneState phaseOneMeleeAttackOneState;
    public PhaseOneDashState phaseOneDashState;
    public PhaseOneChaseToAttackState phaseOneChaseToAttackState;

    //public Vector3 movementDirection;


    public override void EnterState(BossAIScript owner)
    {
        //fixa detta i konstruktor kanske?
        phaseOneStateMashine = new StateMachine<BossPhaseOneState>(this);

        phase1Attack1State = new Phase1Attack1State(owner.testDrainDPS, owner.testDrainRange, owner.testDrainChargeTime);

        phaseOneCombatState = new PhaseOneCombatState(owner.testAttackSpeed, owner.testMinAttackCooldown, owner.testMeleeRange, owner.testDrainRange, owner);
        phaseOneDashState = new PhaseOneDashState(owner.testDashSpeed, owner.testDashDistance, owner.testDashLagDurration, owner.testDashAcceleration);
        phaseOneChaseToAttackState = new PhaseOneChaseToAttackState();


        phaseOneDrainAttackState = new PhaseOneDrainAttackState(owner.testDrainDPS, owner.testDrainRange, owner.testDrainAttackTime, owner.testDrainChargeTime);
        phaseOneMeleeAttackOneState = new PhaseOneMeleeAttackOneState(owner.testDrainDPS, owner.testMeleeRange, owner.testDrainAttackTime, owner.testDrainChargeTime);

        bossPhaseOneParentScript = owner;

        phaseOneStateMashine.ChangeState(phaseOneCombatState);

        //spela cool animation :)
    }

    public override void ExitState(BossAIScript owner)
    {

    }

    public override void UpdateState(BossAIScript owner)
    {
        //kolla om man ska gå över till nästa phase
        if ((owner.testCurrentHP / owner.testMaxHP) < owner.testP2TransitionHP)
        {
            owner.phaseControllingStateMachine.ChangeState(owner.bossPhaseTwoState);
        }

        phaseOneStateMashine.Update();
    }
}

//vet inte om allt detta typ egentligen borde göras i parent statet (borde typ det tror jag)
public class PhaseOneCombatState : State<BossPhaseOneState>
{
    private Timer timer;
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

    public PhaseOneCombatState(float attackSpeed, float minAttackCooldown, float meleeAttackRange, float drainAttackRange, BossAIScript ownerParentScript)
    {
        _attackSpeed = attackSpeed;
        _baseMinAttackCooldown = minAttackCooldown;
        _meleeAttackRange = meleeAttackRange;
        _drainAttackRange = drainAttackRange;

        _ownerParentScript = ownerParentScript;

        timer = new Timer(_attackSpeed);
    }

    public override void EnterState(BossPhaseOneState owner)
    {
        //Debug.Log("in i PhaseOneCombatState");
        if (timer.Expired())
        {
            timer = new Timer(_attackSpeed);
            _minAttackCooldown = _baseMinAttackCooldown;
        }
        else
        {
            _minAttackCooldown += timer.Time;
        }
    }

    public override void ExitState(BossPhaseOneState owner)
    {
        //Debug.Log("hej då PhaseOneCombatState");
        _ownerParentScript.agent.SetDestination(_ownerParentScript.transform.position);
    }

    public override void UpdateState(BossPhaseOneState owner)
    {

        _ownerParentScript.FacePlayer();

        timer.Time += Time.deltaTime;

        //kanske borde dela upp detta i olika movement states pga animationer men vet inte om det behövs

        //kolla om man ska attackera
        if (timer.Expired())
        {
            //nära nog för att göra melee attacken
            if (_drainAttackRange > Vector3.Distance(_ownerParentScript.transform.position, _ownerParentScript.player.transform.position))
            {
                //vill den dash attacka?
                if (UnityEngine.Random.Range(0f, 100f) > 100f - _ownerParentScript.testDashAttackChanse)
                {
                    _bossToPlayer = _ownerParentScript.player.transform.position - _ownerParentScript.transform.position;

                    Physics.Raycast(_ownerParentScript.transform.position + new Vector3(0, 1, 0), _bossToPlayer.normalized, out _hit, _ownerParentScript.testDashDistance + _ownerParentScript.testMeleeRange, _ownerParentScript.targetLayers);

                    //är spelaren innom en bra range och kan den dasha dit den vill
                    if (_hit.transform == _ownerParentScript.player.transform && _bossToPlayer.magnitude < _ownerParentScript.testDashDistance + _ownerParentScript.testMeleeRange)
                    {
                        Debug.Log(_bossToPlayer.magnitude);

                        int dashSign = 0;

                        if(UnityEngine.Random.Range(0f, 1f) > 0.5f)
                        {
                            dashSign = 1;
                        }
                        else
                        {
                            dashSign = -1;
                        }

                        _dashAttackDirection = _bossToPlayer;

                        _dashAttackAngle = Mathf.Rad2Deg * Mathf.Acos((Mathf.Pow(_bossToPlayer.magnitude, 2) + Mathf.Pow(_ownerParentScript.testDashDistance, 2) - Mathf.Pow(_ownerParentScript.testMeleeRange, 2)) / (2 * _bossToPlayer.magnitude * _ownerParentScript.testDashDistance));

                        _dashAttackDirection = Quaternion.AngleAxis(_dashAttackAngle * dashSign, Vector3.up) * _dashAttackDirection;

                        Debug.Log("_dashAttackDirection " + _dashAttackDirection);

                        if (_ownerParentScript.CheckDashPath(_dashAttackDirection))
                        {
                            Debug.Log("dash attack dags, funka på 1a försöket");
                            _ownerParentScript.movementDirection = _dashAttackDirection;
                            _ownerParentScript.dashAttack = true;
                            owner.phaseOneStateMashine.ChangeState(owner.phaseOneDashState);
                        }
                        else
                        {
                            _dashAttackDirection = _bossToPlayer;
                            _dashAttackDirection = Quaternion.AngleAxis(_dashAttackAngle * dashSign * -1, Vector3.up) * _dashAttackDirection;

                            if (_ownerParentScript.CheckDashPath(_dashAttackDirection))
                            {
                                Debug.Log("dash attack dags, funka på 2a försöket");
                                _ownerParentScript.movementDirection = _dashAttackDirection;
                                _ownerParentScript.dashAttack = true;
                                owner.phaseOneStateMashine.ChangeState(owner.phaseOneDashState);
                            }
                            else
                            {
                                Debug.Log("kan inte dasha med all denna skit ivägen juuuuuöööööö");

                                owner.phaseOneStateMashine.ChangeState(owner.phaseOneChaseToAttackState);
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("ITS TO FAR AWAY!!!!");
                        owner.phaseOneStateMashine.ChangeState(owner.phaseOneChaseToAttackState);
                    }
                }
                //springa och slå
                else
                {
                    Debug.Log("no want dash");
                    owner.phaseOneStateMashine.ChangeState(owner.phaseOneChaseToAttackState);
                }
            }
            //drain
            else
            {
                owner.phaseOneStateMashine.ChangeState(owner.phaseOneDrainAttackState);
            }
        }
        //kolla om spelaren är nära nog att slå
        else if (timer.Time > _minAttackCooldown && _meleeAttackRange > Vector3.Distance(_ownerParentScript.transform.position, _ownerParentScript.player.transform.position))
        {
            //kanske göra AOE attack här för att tvinga iväg spelaren?
            owner.phaseOneStateMashine.ChangeState(owner.phaseOneMeleeAttackOneState);
        }
        //idle movement
        else
        {

            Physics.Raycast(_ownerParentScript.transform.position + new Vector3(0, 1, 0), (_ownerParentScript.player.transform.position - _ownerParentScript.transform.position).normalized, out _hit, Mathf.Infinity, _ownerParentScript.targetLayers);

            //om bossen kan se spelaren
            if (_hit.transform == _ownerParentScript.player.transform)
            {
                //gör så att den byter mellan att gå höger och vänster
                int strafeSign = 0;
                if (timer.Ratio() > 0.5f)
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

                for (int i = 0; i < _ownerParentScript.desiredDistanceToAngleValues.Length; i++)
                {
                    if (compairValue > _ownerParentScript.desiredDistanceToPlayer + _ownerParentScript.desiredDistanceOffsetValues[i])
                    {
                        _ownerParentScript.movementDirection = Quaternion.AngleAxis(_ownerParentScript.desiredDistanceAngleValues[i] * strafeSign * -1, Vector3.up) * _ownerParentScript.movementDirection;
                        _ownerParentScript.movementDirection *= strafeSign;
                        break;
                    }
                }

                if (UnityEngine.Random.Range(0f, 100f) > 100f - _ownerParentScript.testDashChansePerFrame)
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
                    //_destination = _ownerParentScript.transform.position + _ownerParentScript.movementDirection * 5 * sign;
                    _destination = _ownerParentScript.transform.position + _ownerParentScript.movementDirection * 5;
                    _ownerParentScript.agent.SetDestination(_destination);
                }
            }
            //om bossen inte kan se spelaren
            else
            {
                //Debug.Log("vart e du!?!?!"); 

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
        _oldSpeed = owner.bossPhaseOneParentScript.agent.speed;
        _oldAcceleration = owner.bossPhaseOneParentScript.agent.acceleration;

        _dashDurration = (_dashDistance - owner.bossPhaseOneParentScript.agent.stoppingDistance) / _dashSpeed;
        //Debug.Log("zoom for, " + _dashDurration + " MPH, " + _dashSpeed);
        //Debug.Log(_dashDistance + " " + owner.bossPhaseOneParentScript.agent.stoppingDistance + " " + _dashSpeed);
        _dashTimer = new Timer(_dashDurration);
        _lagTimer = new Timer(_lagDurration);


        owner.bossPhaseOneParentScript.agent.speed = _dashSpeed;
        owner.bossPhaseOneParentScript.agent.acceleration = _dashAcceleration;

        _dashDirection = owner.bossPhaseOneParentScript.movementDirection.normalized;
        _dashDestination = owner.bossPhaseOneParentScript.transform.position + _dashDirection * _dashDistance;

        owner.bossPhaseOneParentScript.agent.SetDestination(_dashDestination);
    }

    public override void ExitState(BossPhaseOneState owner)
    {
        owner.bossPhaseOneParentScript.agent.speed = _oldSpeed;
        owner.bossPhaseOneParentScript.agent.acceleration = _oldAcceleration;
        //Debug.Log("hej då zoom");
    }

    public override void UpdateState(BossPhaseOneState owner)
    {
        _dashTimer.Time += Time.deltaTime;

        if (_dashTimer.Expired())
        {
            _lagTimer.Time += Time.deltaTime;

            if (owner.bossPhaseOneParentScript.dashAttack)
            {
                owner.bossPhaseOneParentScript.dashAttack = false;
                owner.phaseOneStateMashine.ChangeState(owner.phaseOneMeleeAttackOneState);
            }
            else if (_lagTimer.Expired())
            {
                owner.phaseOneStateMashine.ChangeState(owner.phaseOneCombatState);
            }
        }
    }
}

public class PhaseOneMeleeAttackOneState : State<BossPhaseOneState>
{
    private float _damage;
    private float _range;
    private float _totalDurration;
    private float _chargeTime;


    public PhaseOneMeleeAttackOneState(float damage, float range, float attackTime, float chargeTime)
    {
        _damage = damage;
        _range = range;
        _chargeTime = chargeTime;
        _totalDurration = chargeTime + attackTime;
    }


    public override void EnterState(BossPhaseOneState owner)
    {
        owner.bossPhaseOneParentScript.bossAnimator.SetTrigger("testMelee1Trigger");
        Debug.Log("nu ska jag fan göra PhaseOneMeleeAttackState >:(, med dessa stats:  damage " + _damage + " range " + _range + " totalDurration " + _totalDurration + " chargeTime " + _chargeTime);
    }

    public override void ExitState(BossPhaseOneState owner)
    {
        Debug.Log("hej då PhaseOneMeleeAttackState");
        owner.bossPhaseOneParentScript.animationEnded = false;
        owner.bossPhaseOneParentScript.facePlayer = true;
    }

    public override void UpdateState(BossPhaseOneState owner)
    {
        //kör cool animation som bestämmer när attacken är över istället för durration

        //kolla om det va en träff isåfall slå igen?

        if (owner.bossPhaseOneParentScript.animationEnded)
        {
            owner.phaseOneStateMashine.ChangeState(owner.phaseOneCombatState);
        }
        else
        {
            if (owner.bossPhaseOneParentScript.facePlayer)
            {
                owner.bossPhaseOneParentScript.FacePlayer();
            }
            if (owner.bossPhaseOneParentScript.meleeHitboxActive)
            {
                //här ska attacken kunna skada
                //Debug.Log("bAp");
            }
        }
    }
}

public class PhaseOneChaseToAttackState : State<BossPhaseOneState>
{
    private Vector3 _playerPos;
    private float _distanceToPlayer;
    public override void EnterState(BossPhaseOneState owner)
    {
        Debug.Log("hunt time");
    }

    public override void ExitState(BossPhaseOneState owner)
    {
        Debug.Log("no more hunt time");
        owner.bossPhaseOneParentScript.agent.SetDestination(owner.bossPhaseOneParentScript.transform.position);
    }

    public override void UpdateState(BossPhaseOneState owner)
    {

        _playerPos = owner.bossPhaseOneParentScript.player.transform.position;
        _distanceToPlayer = Vector3.Distance(owner.bossPhaseOneParentScript.transform.position, _playerPos);

        if (_distanceToPlayer > owner.bossPhaseOneParentScript.testDrainRange)
        {
            owner.phaseOneStateMashine.ChangeState(owner.phaseOneDrainAttackState);
        }
        else if (_distanceToPlayer < owner.bossPhaseOneParentScript.testMeleeRange)
        {
            owner.phaseOneStateMashine.ChangeState(owner.phaseOneMeleeAttackOneState);
        }
        else
        {
            owner.bossPhaseOneParentScript.agent.SetDestination(_playerPos);
        }
    }
}

public class PhaseOneDrainAttackState : State<BossPhaseOneState>
{
    private float _damagePerSecond;
    private float _range;
    private float _totalDurration;
    private float _chargeTime;


    public PhaseOneDrainAttackState(float damagePerSecond, float range, float attackTime, float chargeTime)
    {
        _damagePerSecond = damagePerSecond;
        _range = range;
        _chargeTime = chargeTime;
        _totalDurration = chargeTime + attackTime;
    }


    public override void EnterState(BossPhaseOneState owner)
    {
        owner.bossPhaseOneParentScript.bossAnimator.SetTrigger("testDrainTrigger");
        Debug.Log("nu ska jag fan göra PhaseOneDrainAttack >:(, med dessa stats:  damage " + _damagePerSecond + " range " + _range + " totalDurration " + _totalDurration + " chargeTime " + _chargeTime);
    }

    public override void ExitState(BossPhaseOneState owner)
    {
        Debug.Log("hej då PhaseOneDrainAttack");
        owner.bossPhaseOneParentScript.animationEnded = false;
        owner.bossPhaseOneParentScript.facePlayer = true;
    }

    public override void UpdateState(BossPhaseOneState owner)
    {
        //kör cool animation som bestämmer när attacken är över istället för durration

        if (owner.bossPhaseOneParentScript.animationEnded)
        {
            owner.phaseOneStateMashine.ChangeState(owner.phaseOneCombatState);
        }
        else
        {
            if (owner.bossPhaseOneParentScript.facePlayer)
            {
                owner.bossPhaseOneParentScript.FacePlayer();
            }
            if (owner.bossPhaseOneParentScript.drainHitboxActive)
            {
                //här ska attacken kunna skada med _damagePerSecond * time.deltatime
                Debug.Log("pew pew pew");
            }
        }
    }
}

//kan döpa de bättre när vi vet vad de är typ "P1HeavyAttack"
public class Phase1Attack1State : State<BossPhaseOneState>
{
    private float _damage;
    private float _range;
    private float _durration;
    private Timer _timer;


    public Phase1Attack1State(float damage, float range, float durration)
    {
        _damage = damage;
        _range = range;
        _durration = durration;
    }


    public override void EnterState(BossPhaseOneState owner)
    {
        Debug.Log("nu ska jag fan göra Phase1Attack1 >:(, med dessa stats:  damage " + _damage + " range " + _range + " durration " + _durration);
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

        if (_timer.Expired())
        {
            owner.phaseOneStateMashine.ChangeState(owner.phaseOneCombatState);
        }
    }
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
