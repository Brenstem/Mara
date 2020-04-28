using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RangedEnemyAI : BaseAIMovementController 
{
    [SerializeField] private GameObject _projectile;
    [SerializeField] private Transform _projectileSpawnPos;
    [SerializeField] private float _firerate;

    [HideInInspector] public Timer firerateTimer;

    [HideInInspector] public Timer _hitStunTimer;
    [HideInInspector] public bool _useHitStun;

    protected override void Awake()
    {
        base.Awake();
        firerateTimer = new Timer(_firerate);
    }

    private void KillThis()
    {
        stateMachine.ChangeState(new DeadState());
        _anim.SetTrigger("Dead");
        _agent.SetDestination(transform.position);
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
        _anim.SetFloat("Blend", _agent.velocity.magnitude);

        if (_health.GetHealth() <= 0)
        {
            KillThis();
        }
    }

    public void Attack()
    {
        if (firerateTimer.Expired)
        {
            _anim.SetTrigger("Attack");
        }
    }

    public override void TakeDamage(HitboxValues hitbox, Entity attacker)
    {
        stateMachine.ChangeState(new RangedEnemyIdleState());
        EnableHitstun(hitbox.hitstunTime);
        base.TakeDamage(hitbox, attacker);
    }

    public void EnableHitstun(float duration)
    {
        _hitStunTimer = new Timer(duration);
        _useHitStun = true;
        GlobalState.state.AudioManager.FloatingEnemyHurtAudio(this.transform.position);
        _anim.SetTrigger("Hurt");
        _anim.SetBool("InHitstun", true);
    }

    public void DisableHitStun()
    {
        _useHitStun = false;
        _anim.SetBool("InHitstun", false);
    }

    public void Fire()
    {
        GlobalState.state.AudioManager.RangedEnemyFireAudio(this.transform.position);
        Instantiate(_projectile, _projectileSpawnPos.position, _projectileSpawnPos.rotation);
        firerateTimer.Reset();
    }
}

public class RangedEnemyIdleState : BaseIdleState
{
    public override void EnterState(BaseAIMovementController owner)
    {
        _chasingState = new RangedEnemyChasingState();
    }

    public override void UpdateState(BaseAIMovementController owner)
    {
        if (owner.rangedAI._useHitStun)
        {
            owner.rangedAI._hitStunTimer.Time += Time.deltaTime;
            if (owner.rangedAI._hitStunTimer.Expired)
            {
                owner.rangedAI._hitStunTimer.Reset();
                owner.rangedAI.DisableHitStun();
            }
        }
        else
        {
            base.UpdateState(owner);
        }
    }
}

public class RangedEnemyChasingState : BaseChasingState
{
    public override void EnterState(BaseAIMovementController owner)
    {
        _attackingState = new RangedEnemyAttackingState();
        _returnToIdleState = new RangedEnemyReturnToIdleState();
        GlobalState.state.AudioManager.RangedEnemyAlertAudio(owner.rangedAI.transform.position);
    }

    public override void UpdateState(BaseAIMovementController owner)
    {
        base.UpdateState(owner);
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

    public override void UpdateState(BaseAIMovementController owner)
    {
        base.UpdateState(owner);
    }
}
