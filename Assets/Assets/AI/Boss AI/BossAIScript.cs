using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossAIScript : MonoBehaviour
{
    [System.Serializable] public struct PhaseOneStats
    {
        public float testP1value1;
        public float testP1value2;
        public float testP1value3;
    }
    [System.Serializable] public struct PhaseTwoStats
    {
        public float testP2value1;
        public float testP2value2;
        public float testP2value3;
    }

    public PhaseOneStats phaseOneStats;
    public PhaseTwoStats phaseTwoStats;
    
    [System.Serializable] public struct DesiredDistanceToAngleValues
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
    
    //galet nog är alla variabler som heter test något, inte planerat att vara permanenta
    [SerializeField] public float testMaxHP = 500f;
    [SerializeField] [Range(0,1)] public float testP2TransitionHP = 0.5f;

    [SerializeField] public float testAttackSpeed = 5f;
    [SerializeField] public float testMinAttackCooldown = 1f;

    [SerializeField] public float testDrainDPS = 5f;
    [SerializeField] public float testDrainRange = 6f;

    [SerializeField] public float testDrainChargeTime = 7f;
    [SerializeField] public float testDrainAttackTime = 8f;

    [SerializeField] public float testMeleeRange = 6f;

    [SerializeField] public float desiredDistanceToPlayer = 5f;




    [SerializeField] public float aggroRange = 10f;
    [SerializeField] public float defaultTurnSpeed = 5f;


    //borde vara nonserialized men har den som serialized för testning
    /*[NonSerialized]*/ public float testCurrentHP;

    [NonSerialized] public Animator bossAnimator;
    [NonSerialized] public float turnSpeed;

    //[SerializeField] public State<BossPhaseOneState>[] stateArray;


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

        
    }

    void Start()
    {

        for (int i = 0; i < desiredDistanceToAngleValues.Length; i++)
        {
            desiredDistanceOffsetValues.Add(desiredDistanceToAngleValues[i].desiredDistanceOffset);
            desiredDistanceAngleValues.Add(desiredDistanceToAngleValues[i].angle);

            print("added " + desiredDistanceAngleValues[i] + " with " + desiredDistanceOffsetValues[i]);

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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
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


    public override void EnterState(BossAIScript owner)
    {
        //fixa detta i konstruktor kanske?
        phaseOneStateMashine = new StateMachine<BossPhaseOneState>(this);

        phase1Attack1State = new Phase1Attack1State(owner.testDrainDPS, owner.testDrainRange, owner.testDrainChargeTime);

        phaseOneCombatState = new PhaseOneCombatState(owner.testAttackSpeed, owner.testMinAttackCooldown, owner.testMeleeRange, owner.testDrainRange, owner);
        phaseOneDashState = new PhaseOneDashState(4f, 2f, 0.1f);

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
    private float _minAttackCooldown;
    private float _meleeAttackRange;
    private float _drainAttackRange;

    private Vector3 _destination;
    private Vector3 _direction;

    private BossAIScript _ownerParentScript;

    public PhaseOneCombatState(float attackSpeed, float minAttackCooldown, float meleeAttackRange, float drainAttackRange, BossAIScript ownerParentScript)
    {
        _attackSpeed = attackSpeed;
        _minAttackCooldown = minAttackCooldown;
        _meleeAttackRange = meleeAttackRange;
        _drainAttackRange = drainAttackRange;

        _ownerParentScript = ownerParentScript;
    }

    public override void EnterState(BossPhaseOneState owner)
    {
        Debug.Log("in i PhaseOneCombatState");
        timer = new Timer(_attackSpeed);
    }

    public override void ExitState(BossPhaseOneState owner)
    {
        Debug.Log("hej då PhaseOneCombatState");
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
            if (_drainAttackRange > Vector3.Distance(_ownerParentScript.transform.position, _ownerParentScript.player.transform.position))
            {
                if (_meleeAttackRange > Vector3.Distance(_ownerParentScript.transform.position, _ownerParentScript.player.transform.position))
                {
                    owner.phaseOneStateMashine.ChangeState(owner.phaseOneMeleeAttackOneState);
                    //gå/(dash?) fram och melee attack
                }
                else
                {
                    //dasha ibland här?

                    _destination = _ownerParentScript.player.transform.position;

                    _ownerParentScript.agent.SetDestination(_destination);
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
        //idle strafe movement
        else
        {
            //gör så att den byter mellan att gå höger och vänster
            int sign = 0;
            if (timer.Ratio() > 0.5f)
            {
                sign = -1;
            }
            else
            {
                sign = 1;
            }

            _direction = _ownerParentScript.transform.right;

            //lägga till någon randomness variabel så movement inte blir lika predictable? (kan fucka animationerna?)
            //kanske slurpa mellan de olika värdena (kan bli jobbigt och vet inte om det behövs)
            float compairValue = Vector3.Distance(_ownerParentScript.transform.position, _ownerParentScript.player.transform.position);

            for (int i = 0; i < _ownerParentScript.desiredDistanceToAngleValues.Length; i++)
            {
                if (compairValue > _ownerParentScript.desiredDistanceToPlayer + _ownerParentScript.desiredDistanceOffsetValues[i])
                {
                    _direction = Quaternion.AngleAxis(_ownerParentScript.desiredDistanceAngleValues[i] * sign * -1, Vector3.up) * _direction;
                    break;
                }
            }

            /*
            switch (Vector3.Distance(_ownerParentScript.transform.position, _ownerParentScript.player.transform.position))
            {
                case float n when (n > _ownerParentScript.desiredDistanceToPlayer + 3f):
                    //gå väldigt frammåt
                    _direction = Quaternion.AngleAxis(-65 * sign, Vector3.up) * _direction;
                    Debug.Log("gå väldigt frammåt");
                    break;
                case float n when (n > _ownerParentScript.desiredDistanceToPlayer + 2f):
                    //gå ganska frammåt
                    _direction = Quaternion.AngleAxis(-45 * sign, Vector3.up) * _direction;
                    Debug.Log("gå ganska frammåt");
                    break;
                case float n when (n > _ownerParentScript.desiredDistanceToPlayer + 1f):
                    //gå lite frammåt
                    _direction = Quaternion.AngleAxis(-20 * sign, Vector3.up) * _direction;
                    Debug.Log("gå lite frammåt");
                    break;
                case float n when (n > _ownerParentScript.desiredDistanceToPlayer):
                    //gå åt sidan
                    _direction = Quaternion.AngleAxis(0 * sign, Vector3.up) * _direction;
                    Debug.Log("gå åt sidan");
                    break;
                case float n when (n > _ownerParentScript.desiredDistanceToPlayer - 1f):
                    //gå lite bakåt
                    _direction = Quaternion.AngleAxis(20 * sign, Vector3.up) * _direction;
                    Debug.Log("gå lite bakåt");
                    break;
                case float n when (n > _ownerParentScript.desiredDistanceToPlayer - 2f):
                    //gå ganska bakåt
                    _direction = Quaternion.AngleAxis(30 * sign, Vector3.up) * _direction;
                    Debug.Log("gå ganska bakåt");
                    break;
                //tog bort för det känndes wack
                case float n when (n > _ownerParentScript.desiredDistanceToPlayer - 3f):
                    //gå väldigt bakåt
                    _direction = Quaternion.AngleAxis(65 * sign, Vector3.up) * _direction;
                    Debug.Log("2");
                    break;
                //precis inpå spelaren
                case float n when (n > 1f):
                    //gå väldigt bakåt
                    _direction = Quaternion.AngleAxis(90 * sign, Vector3.up) * _direction;
                    //kanske lägga in att man dashar här istället
                    Debug.Log("2 CLOSE >:( (gå väldigt bakåt)");
                    break;
            }
            */

            //ändra 5an till typ destinationAmplifier
            _destination = _ownerParentScript.transform.position + _direction * 5 * sign;

            _ownerParentScript.agent.SetDestination(_destination);
        }
    }
}

public class PhaseOneDashState : State<BossPhaseOneState>
{
    private float _speed;
    private float _dashDurration;
    private float _lagDurration;

    private Timer _dashTimer;
    private Timer _lagTimer;

    private Vector2 _dashDirection;


    public PhaseOneDashState(float speed, float dashDurration, float lagDurration)
    {
        _speed = speed;
        _dashDurration = dashDurration;
        _lagDurration = lagDurration;
    }


    public override void EnterState(BossPhaseOneState owner)
    {
        Debug.Log("zoom for, " + _dashDurration + " MPH, " + _speed);
        _dashTimer = new Timer(_dashDurration);
        _lagTimer = new Timer(_lagDurration);
    }

    public override void ExitState(BossPhaseOneState owner)
    {
        Debug.Log("hej då zoom");
    }

    public override void UpdateState(BossPhaseOneState owner)
    {
        _dashTimer.Time += Time.deltaTime;

        if (_dashTimer.Expired())
        {
            _lagTimer.Time += Time.deltaTime;
            
            if (_lagTimer.Expired())
            {
                owner.phaseOneStateMashine.ChangeState(owner.phaseOneCombatState);
            }
        }
        else
        {
            //dash time
            Debug.Log("dash time");
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


public class PhaseOneMeleeAttackOneState : State<BossPhaseOneState>
{
    private float _damagePerSecond;
    private float _range;
    private float _totalDurration;
    private float _chargeTime;


    public PhaseOneMeleeAttackOneState(float damagePerSecond, float range, float attackTime, float chargeTime)
    {
        _damagePerSecond = damagePerSecond;
        _range = range;
        _chargeTime = chargeTime;
        _totalDurration = chargeTime + attackTime;
    }


    public override void EnterState(BossPhaseOneState owner)
    {
        owner.bossPhaseOneParentScript.bossAnimator.SetTrigger("testMelee1Trigger");
        Debug.Log("nu ska jag fan göra PhaseOneMeleeAttackState >:(, med dessa stats:  damage " + _damagePerSecond + " range " + _range + " totalDurration " + _totalDurration + " chargeTime " + _chargeTime);
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
                Debug.Log("bAp");
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
