using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]

public abstract class BaseAIMovementController : Entity
{
    public StateMachine<BaseAIMovementController> stateMachine;

    [SerializeField] public float _aggroRange = 10f;
    [SerializeField] public float _unaggroRange = 20f;
    [SerializeField] public float _turnSpeed = 5f;

    //Layermask skit för line of sight raycasts
    [SerializeField] public LayerMask _targetLayers;

    [SerializeField] public bool _cyclePathing;
    [SerializeField] public bool _waitAtPoints;
    [SerializeField] public float _waitTime;
    [SerializeField] public Vector3[] _idlePathingPoints;
    [SerializeField] public float _attackRange = 12f;

    [NonSerialized] public Vector3 _idlePosition;

    [NonSerialized] public GameObject _target;
    [NonSerialized] public NavMeshAgent _agent;
    [NonSerialized] public BasicMeleeAI _meleeEnemy;


    [NonSerialized] public RangedEnemyAI rangedAI;
    [NonSerialized] public Timer waitTimer;

    [HideInInspector] public Animator _anim;
    protected EnemyHealth _health;

    virtual protected void Awake()
    {
        _idlePosition = this.transform.position;
        stateMachine = new StateMachine<BaseAIMovementController>(this);
        waitTimer = new Timer(_waitTime);

        _anim = GetComponentInChildren<Animator>();
        _health = GetComponent<EnemyHealth>();
        _agent = GetComponent<NavMeshAgent>();

        _target = GlobalState.state.PlayerGameObject;
    }

    virtual protected void Update()
    {
        stateMachine.Update();
    }

    //vänder monstret mot spelaren
    virtual public void FacePlayer()
    {
        Vector3 direction = (_target.transform.position - this.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, lookRotation, Time.deltaTime * _turnSpeed);
    }

    virtual protected void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _aggroRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _unaggroRange);
        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, _attackRange);

        Gizmos.color = Color.blue;
        if (_idlePathingPoints.Length > 1)
        {
            for (int i = 0; i < _idlePathingPoints.Length-1; i++)
            {
                Gizmos.DrawLine(_idlePathingPoints[i], _idlePathingPoints[i + 1]);
            }
        }
    }

    public override void TakeDamage(HitboxValues hitbox, Entity attacker)
    {
        _health.Damage(hitbox.damageValue);
    }
}

//State classer
public class BaseIdleState : State<BaseAIMovementController>
{
    private RaycastHit _hit;
    private int _pathingIndex = 0;
    protected BaseChasingState _chasingState;

    public override void EnterState(BaseAIMovementController owner) { }

    public override void ExitState(BaseAIMovementController owner)
    {
        //sparar positionen AIn va på när den går ut idle
        owner._idlePosition = owner.transform.position;
    }

    public override void UpdateState(BaseAIMovementController owner)
    {
        if (owner._waitAtPoints)
        {
            owner.waitTimer.Time += Time.deltaTime;
        }

        //idle pathing
        if (owner._idlePathingPoints != null && owner._idlePathingPoints.Length > 1)
        {
            if (owner._agent.stoppingDistance > Vector3.Distance(owner.transform.position, owner._idlePathingPoints[_pathingIndex]))
            {

                if (!owner._cyclePathing)
                {
                    if (_pathingIndex == owner._idlePathingPoints.Length - 1)
                    {
                        System.Array.Reverse(owner._idlePathingPoints);
                    }
                }
                _pathingIndex = (_pathingIndex + 1) % owner._idlePathingPoints.Length;
            }

            //flyttar monstret mot nästa position i positions arrayen
            if (owner._waitAtPoints)
            {
                if (owner.waitTimer.Expired)
                {
                    owner._agent.SetDestination(owner._idlePathingPoints[_pathingIndex]);
                    owner.waitTimer.Reset();
                }
            }
            else
            {
                owner._agent.SetDestination(owner._idlePathingPoints[_pathingIndex]);
            }
        }

        //aggro detection
        if (owner._aggroRange > Vector3.Distance(owner._target.transform.position, owner.transform.position))
        {
            if (Physics.Raycast(owner.transform.position + new Vector3(0, 1, 0), (owner._target.transform.position - owner.transform.position).normalized, out _hit, owner._aggroRange, owner._targetLayers))
            {
                if (_hit.transform == owner._target.transform)
                {
                    owner.stateMachine.ChangeState(_chasingState);
                }
            }
        }
    }
}

public class BaseChasingState : State<BaseAIMovementController>
{
    protected BaseReturnToIdlePosState _returnToIdleState;
    protected BaseAttackingState _attackingState;


    public override void EnterState(BaseAIMovementController owner) { }

    public override void ExitState(BaseAIMovementController owner) { }

    public override void UpdateState(BaseAIMovementController owner)
    {
        float range = owner._attackRange - owner._agent.stoppingDistance;

        Vector3 vectorToPlayer = (owner._target.transform.position - owner.transform.position).normalized * range;
        Vector3 targetPosition = owner._target.transform.position - vectorToPlayer; 

        //flyttar monstret mot spelaren
        owner._agent.SetDestination(targetPosition);
        
        if (owner._unaggroRange <= Vector3.Distance(owner._target.transform.position, owner.transform.position))
        {
            owner.stateMachine.ChangeState(_returnToIdleState);
        }

        if(owner._attackRange >= Vector3.Distance(owner._target.transform.position, owner.transform.position))
        {
            owner._agent.SetDestination(targetPosition);
            owner.stateMachine.ChangeState(_attackingState);
        }
    }
}

public class BaseAttackingState : State<BaseAIMovementController>
{
    protected BaseChasingState _chasingState;

    public override void EnterState(BaseAIMovementController owner) { }

    public override void ExitState(BaseAIMovementController owner)  { }

    public override void UpdateState(BaseAIMovementController owner)
    {
        //lägg in attack metod här

        float range = owner._attackRange - owner._agent.stoppingDistance;

        Vector3 vectorToPlayer = (owner._target.transform.position - owner.transform.position).normalized * range;
        Vector3 targetPosition = owner._target.transform.position - vectorToPlayer;

        owner.FacePlayer();

        if (range < Vector3.Distance(owner._target.transform.position, owner.transform.position))
        {
            owner.stateMachine.ChangeState(_chasingState);
        }
    }
}

public class BaseReturnToIdlePosState : State<BaseAIMovementController>
{
    private RaycastHit _hit;
    protected BaseChasingState _chasingState;
    protected BaseIdleState _idleState;


    public override void EnterState(BaseAIMovementController owner) { }

    public override void ExitState(BaseAIMovementController owner) { }

    public override void UpdateState(BaseAIMovementController owner)
    {
        //flyttar monstret mot positionen den va på när den gick ur idle
        owner._agent.SetDestination(owner._idlePosition);

        //aggro detection
        if (owner._aggroRange > Vector3.Distance(owner._target.transform.position, owner.transform.position))
        {
            if (Physics.Raycast(owner.transform.position + new Vector3(0, 1, 0), (owner._target.transform.position - owner.transform.position).normalized, out _hit, owner._aggroRange, owner._targetLayers))
            {
                if (_hit.transform == owner._target.transform)
                {
                    owner.stateMachine.ChangeState(_chasingState);
                }
            }
        }

        if (owner._agent.stoppingDistance > Vector3.Distance(owner.transform.position, owner._idlePosition))
        {
            owner.stateMachine.ChangeState(_idleState);
        }

    }
}

public class DeadState : State<BaseAIMovementController>
{
    public override void EnterState(BaseAIMovementController owner) { }

    public override void ExitState(BaseAIMovementController owner) { }

    public override void UpdateState(BaseAIMovementController owner) { }
}