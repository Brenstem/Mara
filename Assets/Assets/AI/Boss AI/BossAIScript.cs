using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    public StateMachine<BossAIScript> phaseControllingStateMachine;


    public PreBossFightState preBossFightState = new PreBossFightState();
    public BossPhaseOneState bossPhaseOneState = new BossPhaseOneState();
    public BossPhaseTwoState bossPhaseTwoState = new BossPhaseTwoState();


    [SerializeField] public float testMaxHP = 500f;
    [SerializeField] [Range(0,1)] public float testP2TransitionHP = 0.5f;

    [SerializeField] public float testAttackSpeed = 5f;
    [SerializeField] public float testAttack1DMG = 5f;
    [SerializeField] public float testAttack1Range = 6f;
    [SerializeField] public float testAttack1Duration = 7f;

    [SerializeField] public float testAttack2DMG = 5f;


    [SerializeField] public float aggroRange = 10f;
    [SerializeField] public float turnSpeed = 5f;
    
    [SerializeField] public LayerMask targetLayers;

    //[SerializeField] public State<BossPhaseOneState>[] stateArray;


    /*[NonSerialized]*/ public float testCurrentHP;

    [NonSerialized] public GameObject player;

    void Awake()
    {
        phaseControllingStateMachine = new StateMachine<BossAIScript>(this);

        //borde inte göras såhär at the end of the day men måste göra skit med spelaren då och vet inte om jag får det
        player = GameObject.FindGameObjectWithTag("Player");

        testCurrentHP = testMaxHP;
        testAttack1DMG = phaseOneStats.testP1value1;

    }

    void Start()
    {
        phaseControllingStateMachine.ChangeState(preBossFightState);
    }
    
    void Update()
    {
        phaseControllingStateMachine.Update();
    }

    public void FacePlayer()
    {
        Vector3 direction = (player.transform.position - this.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
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

#region Phase 1 States
public class BossPhaseOneState : State<BossAIScript>
{
    public StateMachine<BossPhaseOneState> phaseOneStateMashine;


    public Phase1IdleState phase1IdleState;
    public Phase1Attack1State phase1Attack1State;

    

    public override void EnterState(BossAIScript owner)
    {
        //fixa detta i konstruktor kanske?
        phaseOneStateMashine = new StateMachine<BossPhaseOneState>(this);

        phase1IdleState = new Phase1IdleState(owner.testAttackSpeed);
        phase1Attack1State = new Phase1Attack1State(owner.testAttack1DMG, owner.testAttack1Range, owner.testAttack1Duration);

        phaseOneStateMashine.ChangeState(phase1IdleState);

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

        //köra currentstate.uppdate
        phaseOneStateMashine.Update();

    }
}

public class Phase1IdleState : State<BossPhaseOneState>
{
    private float timer;
    private float _attackSpeed;

    public Phase1IdleState(float attackSpeed)
    {
        _attackSpeed = attackSpeed;
    }

    public override void EnterState(BossPhaseOneState owner)
    {
        Debug.Log("in i Phase1IdleState");
    }

    public override void ExitState(BossPhaseOneState owner)
    {
        Debug.Log("hej då Phase1IdleState");
    }

    public override void UpdateState(BossPhaseOneState owner)
    {

        timer += Time.deltaTime;

        //kolla om man ska attackera
        if (timer > _attackSpeed)
        {
            timer = 0;

            //bestämma vilken attack (prata med designers för att veta hur (patern, radnom, gameplay baserat (lär bli en massa fula switch cases hursom))) 
            owner.phaseOneStateMashine.ChangeState(owner.phase1Attack1State);
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
            owner.phaseOneStateMashine.ChangeState(owner.phase1IdleState);
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