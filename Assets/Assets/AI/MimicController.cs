using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class MimicController : MonoBehaviour
{
    public StateMachine<MimicController> stateMachine;

    public IdleState idleState = new IdleState();
    public ChasingState chasingState = new ChasingState();
    public AttackingState attackingState = new AttackingState();
    public ReturnToIdlePosState returnToIdlePosState  = new ReturnToIdlePosState();


    [SerializeField] public float aggroRange = 10f;
    [SerializeField] public float unaggroRange = 20f;
    [SerializeField] public float turnSpeed = 5f;

    //Layermask skit för line of sight raycasts
    [SerializeField] public LayerMask targetLayers;

    [SerializeField] public bool cyclePathing;
    [SerializeField] public Vector3[] pathingPoints;


    [NonSerialized] public Vector3 idlePosition;

    [NonSerialized] public GameObject target;
    [NonSerialized] public NavMeshAgent agent;

    void Awake()
    {
        idlePosition = this.transform.position;
        stateMachine = new StateMachine<MimicController>(this);

        agent = GetComponent<NavMeshAgent>();

        //borde inte göras såhär at the end of the day men måste göra skit med spelaren då och vet inte om jag får det
        target = GameObject.FindGameObjectWithTag("Player");
    }


    void Start()
    {
        stateMachine.ChangeState(idleState);
    }


    void Update()
    {
        //agent.SetDestination(new Vector3(10,0,10));
        stateMachine.Update();
    }

    //Körs när fiender ska vända sig mot spelaren
    public void FacePlayer()
    {
        Vector3 direction = (target.transform.position - this.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, lookRotation, Time.deltaTime * turnSpeed);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, unaggroRange);

        Gizmos.color = Color.blue;
        if (pathingPoints.Length > 1)
        {
            for (int i = 0; i < pathingPoints.Length-1; i++)
            {
                Gizmos.DrawLine(pathingPoints[i], pathingPoints[i + 1]);
            }
        }
    }
}


//State classer
public class IdleState : State<MimicController>
{
    RaycastHit hit;
    private int pathingIndex = 0;

    public override void EnterState(MimicController owner)
    {
    }

    public override void ExitState(MimicController owner)
    {
        owner.idlePosition = owner.transform.position;
    }

    public override void UpdateState(MimicController owner)
    {
        //idle pathing
        if (owner.pathingPoints.Length > 1)
        {
            if (owner.agent.stoppingDistance > Vector3.Distance(owner.transform.position, owner.pathingPoints[pathingIndex]))
            {
                if (!owner.cyclePathing)
                {
                    if (pathingIndex == owner.pathingPoints.Length - 1)
                    {
                        System.Array.Reverse(owner.pathingPoints);
                    }
                }
                pathingIndex = (pathingIndex + 1) % owner.pathingPoints.Length;
            }
            owner.agent.SetDestination(owner.pathingPoints[pathingIndex]);
        }

        //aggro detection
        if (owner.aggroRange > Vector3.Distance(owner.target.transform.position, owner.transform.position))
        {
            if (Physics.Raycast(owner.transform.position + new Vector3(0, 1, 0), (owner.target.transform.position - owner.transform.position).normalized, out hit, owner.aggroRange, owner.targetLayers))
            {
                //Debug.Log(hit.collider.gameObject.name);
                if (hit.transform == owner.target.transform)
                {
                    owner.stateMachine.ChangeState(owner.chasingState);
                }
            }
        }
    }
}

public class ChasingState : State<MimicController>
{
    public override void EnterState(MimicController owner)
    {
    }

    public override void ExitState(MimicController owner)
    {
    }

    public override void UpdateState(MimicController owner)
    {
        owner.agent.SetDestination(owner.target.transform.position);
        
        if (owner.unaggroRange < Vector3.Distance(owner.target.transform.position, owner.transform.position))
        {
            owner.stateMachine.ChangeState(owner.returnToIdlePosState);
        }


        if(owner.agent.stoppingDistance > Vector3.Distance(owner.target.transform.position, owner.transform.position))
        {
            owner.stateMachine.ChangeState(owner.attackingState);
        }
    }
}

//kommer antagligen få fixa olika states beroende på attacken men de lär se ut ungefär såhär
public class AttackingState : State<MimicController>
{
    public override void EnterState(MimicController owner)
    {
    }

    public override void ExitState(MimicController owner)
    {
    }

    public override void UpdateState(MimicController owner)
    {
        //lägg in attack metod här

        owner.FacePlayer();

        if (owner.agent.stoppingDistance < Vector3.Distance(owner.target.transform.position, owner.transform.position))
        {
            owner.stateMachine.ChangeState(owner.chasingState);
        }
    }
}

public class ReturnToIdlePosState : State<MimicController>
{
    RaycastHit hit;

    public override void EnterState(MimicController owner)
    {
    }

    public override void ExitState(MimicController owner)
    {
    }

    public override void UpdateState(MimicController owner)
    {

        owner.agent.SetDestination(owner.idlePosition);

        //aggro detection
        if (owner.aggroRange > Vector3.Distance(owner.target.transform.position, owner.transform.position))
        {
            if (Physics.Raycast(owner.transform.position + new Vector3(0, 1, 0), (owner.target.transform.position - owner.transform.position).normalized, out hit, owner.aggroRange, owner.targetLayers))
            {
                //Debug.Log(hit.collider.gameObject.name);
                if (hit.transform == owner.target.transform)
                {
                    owner.stateMachine.ChangeState(owner.chasingState);
                }
            }
        }

        if (owner.agent.stoppingDistance > Vector3.Distance(owner.transform.position, owner.idlePosition))
        {
            owner.stateMachine.ChangeState(owner.idleState);
        }

    }
}