
using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]

public class DWEnemyAI : MonoBehaviour
{
    public StateMachine<DWEnemyAI> stateMachine;

    [SerializeField] public float aggroRange = 10f;
    [SerializeField] public float unaggroRange = 20f;
    [SerializeField] public float turnSpeed = 5f;

    //Layermask skit för line of sight raycasts
    [SerializeField] public LayerMask targetLayers;

    [SerializeField] public bool cyclePathing;
    [SerializeField] public Vector3[] idlePathingPoints;


    [NonSerialized] public Vector3 idlePosition;

    /*[NonSerialized] */
    public GameObject target;
    [NonSerialized] public NavMeshAgent agent;

    virtual protected void Awake()
    {
        idlePosition = this.transform.position;
        stateMachine = new StateMachine<DWEnemyAI>(this);

        agent = GetComponentInParent<NavMeshAgent>();

        //target = GameObject.FindGameObjectWithTag("Player"); // Byt till globalstate.state.player
    }


    virtual protected void Start()
    {
        stateMachine.ChangeState(new ShadowIdleState());
    }


    virtual protected void Update()
    {
        stateMachine.Update();
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
            for (int i = 0; i < idlePathingPoints.Length - 1; i++)
            {
                Gizmos.DrawLine(idlePathingPoints[i], idlePathingPoints[i + 1]);
            }
        }
    }
}

public class ShadowIdleState : State<DWEnemyAI>
{
    private RaycastHit _hit;
    public override void EnterState(DWEnemyAI owner)
    {

    }

    public override void ExitState(DWEnemyAI owner)
    {

    }

    public override void UpdateState(DWEnemyAI owner)
    {
        Debug.Log("idle");
        //aggro detection
        if (owner.aggroRange > Vector3.Distance(owner.target.transform.position, owner.transform.position))
        {
            Debug.Log("within range");
            if (Physics.Raycast(owner.transform.position + new Vector3(0, 1, 0), (owner.target.transform.position - owner.transform.position).normalized, out _hit, owner.aggroRange, owner.targetLayers))
            {
                Debug.Log("ray hit");
                Debug.Log(_hit.collider.gameObject);
                if (_hit.transform == owner.target.transform)
                {
                    owner.stateMachine.ChangeState(new ShadowAggroState());
                }
            }
        }
    }
}

public class ShadowAggroState : State<DWEnemyAI>
{
    private Timer _timer;
    public override void EnterState(DWEnemyAI owner)
    {
        Debug.Log("tru");
        owner.agent.SetDestination(owner.transform.position);
        _timer = new Timer(UnityEngine.Random.Range(1.4f, 2.0f));
    }

    public override void ExitState(DWEnemyAI owner)
    {
        _timer.Reset();
    }

    public override void UpdateState(DWEnemyAI owner)
    {
        _timer.Time += Time.deltaTime;
        if (!_timer.Expired())
        {
            //owner.FacePlayer();
        }
        else
        {
            owner.stateMachine.ChangeState(new ShadowMoveState());
        }
    }
}


public class ShadowMoveState : State<DWEnemyAI>
{
    private Timer _timer;
    public override void EnterState(DWEnemyAI owner)
    {
        Debug.Log("Moving to target...");
        _timer = new Timer(0.2f);
    }

    public override void ExitState(DWEnemyAI owner)
    {
        _timer.Reset();
    }

    public override void UpdateState(DWEnemyAI owner)
    {
        _timer.Time += Time.deltaTime;
        if (!_timer.Expired())
        {
            owner.agent.SetDestination(owner.target.transform.position);
        }
        else
        {
            owner.stateMachine.ChangeState(new ShadowAggroState());
        }
    }
}