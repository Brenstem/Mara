using System;
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

    public Animator bossAnimator;


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


    [NonSerialized] public float turnSpeed;

    //[SerializeField] public State<BossPhaseOneState>[] stateArray;

    //borde vara nonserialized men har den som serialized för testning
    /*[NonSerialized]*/ public float testCurrentHP;

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


    public override void EnterState(BossAIScript owner)
    {
        //fixa detta i konstruktor kanske?
        phaseOneStateMashine = new StateMachine<BossPhaseOneState>(this);

        phaseOneCombatState = new PhaseOneCombatState(owner.testAttackSpeed, owner.testMinAttackCooldown, owner.testMeleeRange, owner.testDrainRange, owner);
        phase1Attack1State = new Phase1Attack1State(owner.testDrainDPS, owner.testDrainRange, owner.testDrainChargeTime);

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

public class PhaseOneCombatState : State<BossPhaseOneState>
{
    private float timer;
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
        
    }

    public override void ExitState(BossPhaseOneState owner)
    {
        Debug.Log("hej då PhaseOneCombatState");
        _ownerParentScript.agent.SetDestination(_ownerParentScript.transform.position);
        timer = 0;
    }

    public override void UpdateState(BossPhaseOneState owner)
    {

        _ownerParentScript.FacePlayer();

        timer += Time.deltaTime;


        //kanske borde dela upp detta i olika movement states pga animationer men vet inte om det behövs

        //kolla om man ska attackera
        if (timer > _attackSpeed)
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
                    //_destination = _ownerParentScript.player.transform.position + (_ownerParentScript.transform.position - _ownerParentScript.player.transform.position).normalized * (_attackRange - _ownerParentScript.agent.stoppingDistance);

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
        else if (timer > _minAttackCooldown && _meleeAttackRange > Vector3.Distance(_ownerParentScript.transform.position, _ownerParentScript.player.transform.position))
        {
            //kanske göra AOE attack för att tvinga iväg spelaren?
            owner.phaseOneStateMashine.ChangeState(owner.phaseOneMeleeAttackOneState);
        }
        //idle movement
        else
        {
            //gör så att den byter mellan att gå höger och vänster
            int sign = 0;
            if (timer > _attackSpeed / 2)
            {
                sign = -1;
                //_ownerParentScript.agent.SetDestination(_ownerParentScript.transform.position + _ownerParentScript.transform.right * 2);
            }
            else
            {
                sign = 1;
                //_ownerParentScript.agent.SetDestination(_ownerParentScript.transform.position + _ownerParentScript.transform.right * 2 * -1);
            }



            _direction = _ownerParentScript.transform.right;

            //_ownerParentScript.desiredDistanceToPlayer = 5f;

            //vector = Quaternion.AngleAxis(-45, Vector3.up) * vector;

            //använd en Serialized dictionary för att kunna säga "om +3 off, ändra vinkeln med -65"
            //lägga till någon randomness variabel så movement inte blir lika predictable? (kan fucka animationerna?)
            switch (Vector3.Distance(_ownerParentScript.transform.position, _ownerParentScript.player.transform.position))
            {
                case float n when (n > _ownerParentScript.desiredDistanceToPlayer + 3f):
                    //gå väldigt frammåt
                    _direction = Quaternion.AngleAxis(-65 * sign, Vector3.up) * _direction;
                    Debug.Log("8");
                    break;
                case float n when (n > _ownerParentScript.desiredDistanceToPlayer + 2f):
                    //gå ganska frammåt
                    _direction = Quaternion.AngleAxis(-45 * sign, Vector3.up) * _direction;
                    Debug.Log("7");
                    break;
                case float n when (n > _ownerParentScript.desiredDistanceToPlayer + 1f):
                    //gå lite frammåt
                    _direction = Quaternion.AngleAxis(-20 * sign, Vector3.up) * _direction;
                    Debug.Log("6");
                    break;
                case float n when (n > _ownerParentScript.desiredDistanceToPlayer):
                    //gå åt sidan
                    _direction = Quaternion.AngleAxis(0 * sign, Vector3.up) * _direction;
                    Debug.Log("5");
                    break;
                case float n when (n > _ownerParentScript.desiredDistanceToPlayer - 1f):
                    //gå lite bakåt
                    _direction = Quaternion.AngleAxis(20 * sign, Vector3.up) * _direction;
                    Debug.Log("4");
                    break;
                case float n when (n > _ownerParentScript.desiredDistanceToPlayer - 2f):
                    //gå ganska bakåt
                    _direction = Quaternion.AngleAxis(30 * sign, Vector3.up) * _direction;
                    Debug.Log("3");
                    break;
                //tog bort för det känndes wack
                /*case float n when (n > _ownerParentScript.desiredDistanceToPlayer - 3f):
                    //gå väldigt bakåt
                    _direction = Quaternion.AngleAxis(65 * sign, Vector3.up) * _direction;
                    Debug.Log("2");
                    break;*/
                //precis inpå spelaren
                case float n when (n > 1f):
                    //gå väldigt bakåt
                    _direction = Quaternion.AngleAxis(90 * sign, Vector3.up) * _direction;
                    //kanske lägga in att man dashar här istället
                    Debug.Log("2 CLOSE >:(");
                    break;
            }
            //_direction = Quaternion.AngleAxis(-45 * sign, Vector3.up) * _direction;

            Debug.Log(_direction);

            //ändra 5an till typ destinationAmplifier
            _destination = _ownerParentScript.transform.position + _direction * 5 * sign;

            Debug.Log(_destination);

            _ownerParentScript.agent.SetDestination(_destination);
        }
    }
}



public class PhaseOneDrainAttackState : State<BossPhaseOneState>
{
    private float _damagePerSecond;
    private float _range;
    private float _totalDurration;
    private float _chargeTime;
    private float timer;


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
    private float timer;


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
    private float timer;


    public Phase1Attack1State(float damage, float range, float durration)
    {
        _damage = damage;
        _range = range;
        _durration = durration;
    }


    public override void EnterState(BossPhaseOneState owner)
    {
        Debug.Log("nu ska jag fan göra Phase1Attack1 >:(, med dessa stats:  damage " + _damage + " range " + _range + " durration " + _durration);
    }

    public override void ExitState(BossPhaseOneState owner)
    {
        Debug.Log("hej då Phase1Attack1");
    }

    public override void UpdateState(BossPhaseOneState owner)
    {
        //kör cool animation som bestämmer när attacken är över istället för durration

        timer += Time.deltaTime;

        if (timer > _durration)
        {
            timer = 0;
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
