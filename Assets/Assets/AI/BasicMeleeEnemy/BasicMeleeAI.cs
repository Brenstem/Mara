using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;

public class BasicMeleeAI : BaseAIMovementController
{
    [SerializeField] public GameObject _fill;

    [HideInInspector] public Timer _hitStunTimer;

    // Start is called before the first frame update
    void Start()
    {
        _fill.SetActive(false);
        stateMachine.ChangeState(new BasicMeleeIdleState());
        meleeEnemy = this;
        GenerateNewAttackTimer();
    }

    protected override void Update()
    {
        base.Update();

        //print(stateMachine.currentState);

        _anim.SetFloat("Blend", _agent.velocity.magnitude);
    }

    public override void KillThis()
    {
        base.KillThis();

        stateMachine.ChangeState(new DeadState());
        _anim.SetBool("Dead", true);
        _agent.SetDestination(transform.position);
        transform.tag = "Untagged";
    }

    public override void TakeDamage(HitboxValues hitbox, Entity attacker)
    {
        EnableHitstun(hitbox.hitstunTime);
        GlobalState.state.AudioManager.FloatingEnemyHurtAudio(this.transform.position);
        base.TakeDamage(hitbox, attacker);
    }

    public void EnableHitstun(float duration)
    {
        if (duration > 0.0f && _canEnterHitStun)
        {
            stateMachine.ChangeState(new MeleeAIHitstunState());
            _hitStunTimer = new Timer(duration);
        }
    }
}

public class BasicMeleeIdleState : BaseIdleState
{
    public override void EnterState(BaseAIMovementController owner)
    {
        _chasingState = new BasicMeleeChasingState();
    }
    public override void UpdateState(BaseAIMovementController owner)
    {
        base.UpdateState(owner);
        owner._attackRateTimer += Time.deltaTime;
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
    public override void UpdateState(BaseAIMovementController owner)
    {
        base.UpdateState(owner);
        owner._attackRateTimer += Time.deltaTime;
    }
}

public class BasicMeleeAttackingState : BaseAttackingState
{
    public override void EnterState(BaseAIMovementController owner)
    {
        _chasingState = new BasicMeleeChasingState();
    }

    public override void UpdateState(BaseAIMovementController owner)
    {
        owner._attackRateTimer += Time.deltaTime;
        
        float range = owner._attackRange - owner._agent.stoppingDistance;

        owner.FacePlayer();

        if (range < Vector3.Distance(owner._target.transform.position, owner.transform.position))
        {
            owner.stateMachine.ChangeState(_chasingState);
        }
        else if (range > Vector3.Distance(owner._target.transform.position, owner.transform.position))
        {
            if (owner._attackRateTimer.Expired)
            {
                owner.stateMachine.ChangeState(new BasicMeleeSwingState());
            }
        }
    }
}

public class BasicMeleeSwingState : State<BaseAIMovementController>
{
    public override void EnterState(BaseAIMovementController owner)
    {
        owner._anim.SetTrigger("Attack");
        owner._canEnterHitStun = false;
    }

    public override void ExitState(BaseAIMovementController owner)
    {
        owner._animationOver = false;
        owner._canEnterHitStun = owner._usesHitStun;
        owner.GenerateNewAttackTimer();
    }

    public override void UpdateState(BaseAIMovementController owner)
    {
        owner._attackRateTimer += Time.deltaTime;

        owner.FacePlayer();

        if (owner._animationOver)
        {
            owner.stateMachine.ChangeState(owner.stateMachine.previousState);
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

    public override void UpdateState(BaseAIMovementController owner)
    {
        base.UpdateState(owner);
        owner._attackRateTimer += Time.deltaTime;
    }

}

public class MeleeAIHitstunState : State<BaseAIMovementController>
{
    public override void EnterState(BaseAIMovementController owner) 
    {
        GlobalState.state.AudioManager.FloatingEnemyHurtAudio(owner.transform.position);
        owner._anim.SetTrigger("Hurt");
        owner._anim.SetBool("InHitstun", true);
    }

    public override void ExitState(BaseAIMovementController owner) 
    {
        owner._anim.SetBool("InHitstun", false);
    }

    public override void UpdateState(BaseAIMovementController owner)
    {
        owner.meleeEnemy._hitStunTimer.Time += Time.deltaTime;

        if (owner.meleeEnemy._hitStunTimer.Expired)
        {
            owner.meleeEnemy._hitStunTimer.Reset();
            owner.stateMachine.ChangeState(owner.stateMachine.previousState);
        }
    }
}