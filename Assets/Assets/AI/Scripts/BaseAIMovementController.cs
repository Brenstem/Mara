using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]

public abstract class BaseAIMovementController : Entity
{
    [Header("Aggro")]
    [SerializeField] public float _aggroRange = 10f;
    [SerializeField] public float _unaggroRange = 20f;
    [SerializeField] public LayerMask _targetLayers;

    [Header("Pathing")]
    [SerializeField] public float _turnSpeed = 5f;
    [SerializeField] public bool _cyclePathing;
    [SerializeField] public bool _waitAtPoints;
    [SerializeField] public float _waitTime;
    [SerializeField] public Vector3[] _idlePathingPoints;

    [NonSerialized] public NavMeshAgent _agent;
    [NonSerialized] public Vector3 _idlePosition;
    [NonSerialized] public Timer _waitTimer;

    [Header("Attacks")]
    [SerializeField] public float _attackRange = 12f;
    [SerializeField] private float _minAttackSpeed;
    [SerializeField] private float _maxAttackSpeedIncrease;
    [SerializeField] public bool _usesHitStun = true;

    [NonSerialized] public GameObject _target;
    [HideInInspector] public Timer _attackRateTimer;
    [HideInInspector] public Animator _anim;
    [HideInInspector] public bool _animationOver;
    [HideInInspector] public bool _canEnterHitStun;
    private float _attackSpeed;

    [Header("Insanity increase drop")]
    [SerializeField] private GameObject _dropPrefab;
    [SerializeField] private float _insanityIncreaseAmount;
    [SerializeField] public bool deathDrop = true;

    [NonSerialized] public BasicMeleeAI meleeEnemy;
    [NonSerialized] public RangedEnemyAI rangedAI;
    [NonSerialized] public MylingAI mylingAI;

    [HideInInspector] public StateMachine<BaseAIMovementController> stateMachine;

    /* === UNITY FUNCTIONS === */
    virtual protected new void Awake()
    {
        base.Awake();

        _idlePosition = this.transform.position;
        stateMachine = new StateMachine<BaseAIMovementController>(this);
        _waitTimer = new Timer(_waitTime);

        _anim = GetComponentInChildren<Animator>();
        health = GetComponent<EnemyHealth>();
        _agent = GetComponent<NavMeshAgent>();

        _target = GlobalState.state.Player.gameObject;
        _canEnterHitStun = _usesHitStun;
    }

    virtual protected void Update()
    {
        stateMachine.Update();
    }

    protected virtual void OnDrawGizmosSelected()
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
            for (int i = 0; i < _idlePathingPoints.Length - 1; i++)
            {
                Gizmos.DrawLine(_idlePathingPoints[i], _idlePathingPoints[i + 1]);
            }
        }
    }

    /* === PUBLIC FUNCTIONS === */
    public virtual void FacePlayer()
    {
        Vector3 direction = (_target.transform.position - this.transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, lookRotation, Time.deltaTime * _turnSpeed);
    }

    public override void TakeDamage(HitboxValues hitbox, Entity attacker)
    {
        health.Damage(hitbox);
    }

    public void GenerateNewAttackTimer()
    {
        _attackSpeed = _minAttackSpeed;
        _attackSpeed += UnityEngine.Random.Range(0f, _maxAttackSpeedIncrease / 2);
        _attackSpeed += UnityEngine.Random.Range(0f, _maxAttackSpeedIncrease / 2);

        _attackRateTimer = new Timer(_attackSpeed);
    }

    public void GenerateNewAttackTimer(float delayAmount)
    {
        _attackSpeed = _minAttackSpeed;
        _attackSpeed += UnityEngine.Random.Range(0f, _maxAttackSpeedIncrease / 2);
        _attackSpeed += UnityEngine.Random.Range(0f, _maxAttackSpeedIncrease / 2);

        _attackRateTimer = new Timer(_attackSpeed + delayAmount);
    }

    public void GenerateNewAttackTimer(float minSpeedIncrease, float maxSpeedIncrease)
    {
        _attackSpeed = minSpeedIncrease;
        _attackSpeed += UnityEngine.Random.Range(0f, maxSpeedIncrease / 2);
        _attackSpeed += UnityEngine.Random.Range(0f, maxSpeedIncrease / 2);

        _attackRateTimer = new Timer(_attackSpeed);
    }

    public void OnDeathDrop()
    {
        GameObject dropObject = Instantiate(_dropPrefab, this.transform.position + new Vector3(0, 0.5f, 0), this.transform.rotation);
        dropObject.SendMessage("SetIncrementAmount", _insanityIncreaseAmount);
    }
}

/* === IDLE STATE === */
public class BaseIdleState : State<BaseAIMovementController>
{
    private RaycastHit _hit;
    private int _pathingIndex = 0;
    protected BaseChasingState _chasingState; // TODO Move these variables to a constructor or something similar

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
            owner._waitTimer.Time += Time.deltaTime;
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
                if (owner._waitTimer.Expired)
                {
                    owner._agent.SetDestination(owner._idlePathingPoints[_pathingIndex]);
                    owner._waitTimer.Reset();
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

/* === CHASING STATE === */
public class BaseChasingState : State<BaseAIMovementController>
{
    protected BaseReturnToIdlePosState _returnToIdleState;
    protected BaseAttackingState _attackingState;

    public override void EnterState(BaseAIMovementController owner) { }

    public override void ExitState(BaseAIMovementController owner) { }

    public override void UpdateState(BaseAIMovementController owner)
    {
        float range = owner._attackRange - owner._agent.stoppingDistance;

        Vector3 vectorToPlayer = (owner._target.transform.position - owner.transform.position).normalized * (range - 0.5f);
        Vector3 targetPosition = owner._target.transform.position - vectorToPlayer;

        owner._agent.SetDestination(targetPosition);

        if (owner._unaggroRange <= Vector3.Distance(owner._target.transform.position, owner.transform.position))
        {
            owner.stateMachine.ChangeState(_returnToIdleState);
        }

        if (owner._attackRange >= Vector3.Distance(owner._target.transform.position, owner.transform.position))
        {
            owner.stateMachine.ChangeState(_attackingState);
        }
    }
}

/* === ATTACKING STATE === */
public class BaseAttackingState : State<BaseAIMovementController>
{
    protected BaseChasingState _chasingState;

    public override void EnterState(BaseAIMovementController owner) { }

    public override void ExitState(BaseAIMovementController owner) { }

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

/* === RETURN TO IDLE STATE === */
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

/* === DEAD STATE === */
public class DeadState : State<BaseAIMovementController>
{
    public override void EnterState(BaseAIMovementController owner) 
    {
        owner.GetComponent<CapsuleCollider>().enabled = false;    
        owner.invulerable = true;

        if (owner.deathDrop)
            owner.OnDeathDrop();
    }

    public override void ExitState(BaseAIMovementController owner)
    {
        
    }

    public override void UpdateState(BaseAIMovementController owner) { }
}