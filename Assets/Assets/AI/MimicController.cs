using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class MimicController : MonoBehaviour
{
    public StateMachine<MimicController> stateMachine;

    public IdleState idleState = new IdleState();
    public ChasingState chasingState = new ChasingState();


    [SerializeField] public float aggroRange = 10f;

    public GameObject target;
    public NavMeshAgent agent;

    void Awake()
    {
        stateMachine = new StateMachine<MimicController>(this);
        stateMachine.ChangeState(idleState);

        agent = GetComponent<NavMeshAgent>();
        //borde inte göras såhär at the end of the day men orkar itne fixa skit nu
        target = GameObject.FindGameObjectWithTag("Player");


    }


    void Start()
    {
        //target = GameObject.FindWithTag("Player");

        print(target.name);
    }


    void Update()
    {
        //agent.SetDestination(new Vector3(10,0,10));
        stateMachine.Update();
    }



    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }
}


//State classer
public class IdleState : State<MimicController>
{
    public override void EnterState(MimicController owner)
    {
        Debug.Log("idle time");
    }

    public override void ExitState(MimicController owner)
    {
        Debug.Log("its no more idle time");
    }

    public override void UpdateState(MimicController owner)
    {
        if (owner.aggroRange > Vector3.Distance(owner.target.transform.position, owner.transform.position))
        //if (owner.aggroRange < owner.target.transform.position.x)
        {
            owner.stateMachine.ChangeState(owner.chasingState);
        }
    }
}

public class ChasingState : State<MimicController>
{
    public override void EnterState(MimicController owner)
    {
        Debug.Log("hunt time");
    }

    public override void ExitState(MimicController owner)
    {
        Debug.Log("its no more hunt time");
    }

    public override void UpdateState(MimicController owner)
    {
        Debug.Log("we out here");
        owner.agent.SetDestination(owner.target.transform.position);
    }
}


