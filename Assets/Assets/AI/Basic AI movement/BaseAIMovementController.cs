using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]

public abstract class BaseAIMovementController : MonoBehaviour
{
    public StateMachine<BaseAIMovementController> stateMachine;

    [SerializeField] public float aggroRange = 10f;
    [SerializeField] public float unaggroRange = 20f;
    [SerializeField] public float turnSpeed = 5f;

    //Layermask skit för line of sight raycasts
    [SerializeField] public LayerMask targetLayers;

    [SerializeField] public bool cyclePathing;
    [SerializeField] public Vector3[] idlePathingPoints;
    [SerializeField] public float attackRange;

    [NonSerialized] public Vector3 idlePosition;

    [NonSerialized] public GameObject target;
    [NonSerialized] public NavMeshAgent agent;

    virtual protected void Awake()
    {
        idlePosition = this.transform.position;
        stateMachine = new StateMachine<BaseAIMovementController>(this);

        agent = GetComponent<NavMeshAgent>();

        //borde inte göras såhär at the end of the day men måste göra skit med spelaren då och vet inte om jag får det
        target = GlobalState.state.Player;
    }

    virtual protected void Start()
    {
        stateMachine.ChangeState(new BaseIdleState());
    }

    virtual protected void Update()
    {
        stateMachine.Update();
        print(stateMachine.currentState);
    }

    //vänder monstret mot spelaren
    virtual public void FacePlayer()
    {
        Vector3 direction = (target.transform.position - this.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, lookRotation, Time.deltaTime * turnSpeed);
    }

    virtual protected void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, unaggroRange);

        Gizmos.color = Color.blue;
        if (idlePathingPoints.Length > 1)
        {
            for (int i = 0; i < idlePathingPoints.Length-1; i++)
            {
                Gizmos.DrawLine(idlePathingPoints[i], idlePathingPoints[i + 1]);
            }
        }
    }
}

//State classer
public class BaseIdleState : State<BaseAIMovementController>
{
    private RaycastHit _hit;
    private int _pathingIndex = 0;

    public override void EnterState(BaseAIMovementController owner) { }

    public override void ExitState(BaseAIMovementController owner)
    {
        //sparar positionen AIn va på när den går ut idle
        owner.idlePosition = owner.transform.position;
    }

    public override void UpdateState(BaseAIMovementController owner)
    {
        //idle pathing
        if (owner.idlePathingPoints.Length > 1)
        {
            if (owner.agent.stoppingDistance > Vector3.Distance(owner.transform.position, owner.idlePathingPoints[_pathingIndex]))
            {
                if (!owner.cyclePathing)
                {
                    if (_pathingIndex == owner.idlePathingPoints.Length - 1)
                    {
                        System.Array.Reverse(owner.idlePathingPoints);
                    }
                }
                _pathingIndex = (_pathingIndex + 1) % owner.idlePathingPoints.Length;
            }
            //flyttar monstret mot nästa position i positions arrayen
            owner.agent.SetDestination(owner.idlePathingPoints[_pathingIndex]);
        }

        //aggro detection
        if (owner.aggroRange > Vector3.Distance(owner.target.transform.position, owner.transform.position))
        {
            if (Physics.Raycast(owner.transform.position + new Vector3(0, 1, 0), (owner.target.transform.position - owner.transform.position).normalized, out _hit, owner.aggroRange, owner.targetLayers))
            {
                if (_hit.transform == owner.target.transform)
                {
                    owner.stateMachine.ChangeState(new BaseChasingState());
                }
            }
        }
    }
}

public class BaseChasingState : State<BaseAIMovementController>
{
    public override void EnterState(BaseAIMovementController owner) { }

    public override void ExitState(BaseAIMovementController owner) { }

    public override void UpdateState(BaseAIMovementController owner)
    {
        float range = owner.agent.stoppingDistance + owner.attackRange;

        //
        Vector3 vectorToPlayer = (owner.target.transform.position - owner.transform.position).normalized * range;
        Vector3 targetPosition = owner.target.transform.position - vectorToPlayer; 

        //flyttar monstret mot spelaren
        owner.agent.SetDestination(targetPosition);
        
        if (owner.unaggroRange < Vector3.Distance(owner.target.transform.position, owner.transform.position))
        {
            owner.stateMachine.ChangeState(new BaseReturnToIdlePosState()); ;
        }


        Debug.Log("range " + range);
        Debug.Log("distance " + Vector3.Distance(owner.target.transform.position, owner.transform.position));
        if(range > Vector3.Distance(targetPosition, owner.transform.position))
        {
            owner.stateMachine.ChangeState(new BaseAttackingState());
        }
    }
}

//kommer antagligen få fixa olika states beroende på attacken men de lär se ut ungefär såhär
public class BaseAttackingState : State<BaseAIMovementController>
{
    public override void EnterState(BaseAIMovementController owner) { }

    public override void ExitState(BaseAIMovementController owner)  { }

    public override void UpdateState(BaseAIMovementController owner)
    {
        //lägg in attack metod här

        float range = owner.agent.stoppingDistance + owner.attackRange;

        Vector3 vectorToPlayer = (owner.target.transform.position - owner.transform.position).normalized * range;
        Vector3 targetPosition = owner.target.transform.position - vectorToPlayer; // target position - (vector between enemy and player with length of range)

        owner.FacePlayer();

        if (range < Vector3.Distance(targetPosition, owner.transform.position))
        {
            owner.stateMachine.ChangeState(new BaseChasingState());
        }
    }
}

public class BaseReturnToIdlePosState : State<BaseAIMovementController>
{
    private RaycastHit _hit;

    public override void EnterState(BaseAIMovementController owner) { }

    public override void ExitState(BaseAIMovementController owner) { }

    public override void UpdateState(BaseAIMovementController owner)
    {
        //flyttar monstret mot positionen den va på när den gick ur idle
        owner.agent.SetDestination(owner.idlePosition);

        //aggro detection
        if (owner.aggroRange > Vector3.Distance(owner.target.transform.position, owner.transform.position))
        {
            if (Physics.Raycast(owner.transform.position + new Vector3(0, 1, 0), (owner.target.transform.position - owner.transform.position).normalized, out _hit, owner.aggroRange, owner.targetLayers))
            {
                if (_hit.transform == owner.target.transform)
                {
                    owner.stateMachine.ChangeState(new BaseChasingState());
                }
            }
        }

        if (owner.agent.stoppingDistance > Vector3.Distance(owner.transform.position, owner.idlePosition))
        {
            owner.stateMachine.ChangeState(new BaseIdleState());
        }

    }
}