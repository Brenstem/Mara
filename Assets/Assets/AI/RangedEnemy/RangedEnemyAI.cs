using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedEnemyAI : BaseAIMovementController 
{
    [SerializeField] private GameObject _projectile;
    [SerializeField] private Transform _projectileSpawnPos;
    [SerializeField] private float _firerate;

    [HideInInspector] public Timer firerateTimer;

    protected override void Awake()
    {
        base.Awake();
        firerateTimer = new Timer(_firerate);
    }

    private void Start()
    {
        stateMachine.ChangeState(new RangedEnemyIdleState());
        rangedAI = this;
    }

    protected override void Update()
    {
        base.Update();
        firerateTimer.Time += Time.deltaTime;
    }

    public void Attack()
    {
        if (firerateTimer.Expired)
        {
            Instantiate(_projectile, _projectileSpawnPos.position, _projectileSpawnPos.rotation);
            firerateTimer.Reset();
        }
    }

    public override void TakeDamage(Hitbox hitbox)
    {
        // hitstun logic here
        GetComponent<EnemyHealth>().Damage(hitbox.damageValue);
    }

    public override void TakeDamage(float damage)
    {
        GetComponent<EnemyHealth>().Damage(damage);
    }
}

public class RangedEnemyIdleState : BaseIdleState
{
    public override void EnterState(BaseAIMovementController owner)
    {
        _chasingState = new RangedEnemyChasingState();
    }
}

public class RangedEnemyChasingState : BaseChasingState
{
    public override void EnterState(BaseAIMovementController owner)
    {
        _attackingState = new RangedEnemyAttackingState();
        _returnToIdleState = new RangedEnemyReturnToIdleState();
    }
}

public class RangedEnemyAttackingState : BaseAttackingState
{
    public override void EnterState(BaseAIMovementController owner)
    {
        _chasingState = new RangedEnemyChasingState();
        owner.rangedAI.firerateTimer.Reset();
    }

    public override void UpdateState(BaseAIMovementController owner)
    {
        owner.rangedAI.Attack();

        base.UpdateState(owner);
    }
}

public class RangedEnemyReturnToIdleState : BaseReturnToIdlePosState
{
    public override void EnterState(BaseAIMovementController owner)
    {
        _chasingState = new RangedEnemyChasingState();
        _idleState = new RangedEnemyIdleState();
    }
}