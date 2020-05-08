using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMeleeAI : BaseAIMovementController
{
    [SerializeField] public GameObject _fill;

    [HideInInspector] public Timer _hitStunTimer;
    [HideInInspector] public bool _useHitStun;
    [HideInInspector] public bool _animationOver;

    // Start is called before the first frame update
    void Start()
    {
        _fill.SetActive(false);
        stateMachine.ChangeState(new BasicMeleeIdleState());
        meleeEnemy = this;
    }

    protected override void Update()
    {
        base.Update();

        _anim.SetFloat("Blend", _agent.velocity.magnitude);
    }

    public override void KillThis()
    {
        stateMachine.ChangeState(new DeadState());
        _anim.SetBool("Dead", true);
        _agent.SetDestination(transform.position);
        transform.tag = "Untagged";
    }

    public void Attack()
    {
        _anim.SetTrigger("Attack");
    }

    public override void TakeDamage(HitboxValues hitbox, Entity attacker)
    {
        EnableHitstun(hitbox.hitstunTime);
        GlobalState.state.AudioManager.FloatingEnemyHurtAudio(this.transform.position);
        base.TakeDamage(hitbox, attacker);
    }

    public void EnableHitstun(float duration)
    {
        if (duration > 0.0f)
        {
            stateMachine.ChangeState(new MeleeAIHitstunState());
            _hitStunTimer = new Timer(duration);
            _useHitStun = true;
            GlobalState.state.AudioManager.FloatingEnemyHurtAudio(this.transform.position);
            _anim.SetTrigger("Hurt");
            _anim.SetBool("InHitstun", true);
        }
    }

    public void DisableHitStun()
    {
        stateMachine.ChangeState(stateMachine.previousState);
        _useHitStun = false;
        _anim.SetBool("InHitstun", false);
    }

}

public class BasicMeleeIdleState : BaseIdleState
{
    public override void EnterState(BaseAIMovementController owner)
    {
        _chasingState = new BasicMeleeChasingState();
    }
}

public class BasicMeleeChasingState : BaseChasingState
{
    public override void EnterState(BaseAIMovementController owner)
    {
        _attackingState = new BasicMeleeAttackingState();
        _returnToIdleState = new BasicMeleeReturnToIdleState();
        // GlobalState.state.AudioManager.RangedEnemyAlertAudio(owner._meleeEnemy.transform.position);
        owner.meleeEnemy._fill.SetActive(true);
    }
}

public class BasicMeleeAttackingState : BaseAttackingState
{
    public override void EnterState(BaseAIMovementController owner)
    {
        _chasingState = new BasicMeleeChasingState();
        owner.meleeEnemy._animationOver = false;
        owner.meleeEnemy.Attack();
    }

    public override void UpdateState(BaseAIMovementController owner)
    {
        float range = owner._attackRange - owner._agent.stoppingDistance;

        Vector3 vectorToPlayer = (owner._target.transform.position - owner.transform.position).normalized * range;
        Vector3 targetPosition = owner._target.transform.position - vectorToPlayer;

        owner.FacePlayer();

        if (range < Vector3.Distance(owner._target.transform.position, owner.transform.position) && owner.meleeEnemy._animationOver)
        {
            owner.stateMachine.ChangeState(_chasingState);
        }
        else if (range > Vector3.Distance(owner._target.transform.position, owner.transform.position) && owner.meleeEnemy._animationOver)
        {
            owner.stateMachine.ChangeState(new BasicMeleeAttackingState());
        }
    }
}

public class BasicMeleeReturnToIdleState : BaseReturnToIdlePosState
{
    public override void EnterState(BaseAIMovementController owner)
    {
        _chasingState = new BasicMeleeChasingState();
        _idleState = new BasicMeleeIdleState();
    }

    public override void ExitState(BaseAIMovementController owner)
    {
        owner.meleeEnemy._fill.SetActive(false);
        base.ExitState(owner);
    }
}

public class MeleeAIHitstunState : State<BaseAIMovementController>
{
    public override void EnterState(BaseAIMovementController owner) {  }

    public override void ExitState(BaseAIMovementController owner) {  }

    public override void UpdateState(BaseAIMovementController owner)
    {
        if (owner.meleeEnemy._useHitStun)
        {
            owner.meleeEnemy._hitStunTimer.Time += Time.deltaTime;
            if (owner.meleeEnemy._hitStunTimer.Expired)
            {
                owner.meleeEnemy._hitStunTimer.Reset();
                owner.meleeEnemy.DisableHitStun();
            }
        }
    }
}