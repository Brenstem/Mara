using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;

public class BasicMeleeAI : BaseAIMovementController
{
    [Header("Parry")]
    [SerializeField] private float _hitstunOnParry;

    [Header("Hitstun")]
    [SerializeField] private float _attackDelayAfterHitstun;

    [Header("References")]
    [SerializeField] public GameObject _healthBar;
    [SerializeField] public HitboxGroup hitboxGroup;

    // [SerializeField] public float _attackDelayAfterHitstun;

    [HideInInspector] public Timer _hitStunTimer;

    /* === UNITY FUNCTIONS === */
    void Start()
    {
        for (int i = 0; i < _healthBar.transform.childCount; i++)
        {
            _healthBar.transform.GetChild(i).gameObject.SetActive(false);
        }

        stateMachine.ChangeState(new BasicMeleeIdleState());
        meleeEnemy = this;
        GenerateNewAttackTimer();
    }

    protected override void Update()
    {
        base.Update();

        _anim.SetFloat("Blend", _agent.velocity.magnitude);
    }

    /* === PUBLIC FUNCTIONS === */
    public override void KillThis()
    {
        stateMachine.ChangeState(new DeadState());
        _anim.SetBool("Dead", true);
        _agent.SetDestination(transform.position);
        transform.tag = "Untagged";
    }

    public override void TakeDamage(HitboxValues hitbox, Entity attacker)
    {
        EnableHitstun(hitbox.hitstunTime, hitbox.ignoreArmor);
        GlobalState.state.AudioManager.FloatingEnemyHurtAudio(this.transform.position);
        base.TakeDamage(hitbox, attacker);

        for (int i = 0; i < _healthBar.transform.childCount; i++)
        {
            _healthBar.transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    public override void Parried()
    {
        EnableHitstun(_hitstunOnParry, true);
        //Debug.LogWarning("Parried implementation missing", this);
    }

    public void EnableHitstun(float duration, bool ignoreArmor)
    {
        if (duration > 0.0f && ignoreArmor)
        {
            _hitStunTimer = new Timer(duration);
            stateMachine.ChangeState(new MeleeAIHitstunState());
        }
        else if (duration > 0.0f && _canEnterHitStun)
        {
            print("meme");
            _hitStunTimer = new Timer(duration);
            stateMachine.ChangeState(new MeleeAIHitstunState());
        }
        else if (duration > 0.0f && !_canEnterHitStun)
        {
            GenerateNewAttackTimer(_attackDelayAfterHitstun);
        }
    }
}

/* === IDLE STATE === */
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

/* === CHASING STATE === */ 
public class BasicMeleeChasingState : BaseChasingState
{
    public override void EnterState(BaseAIMovementController owner)
    {
        _attackingState = new BasicMeleeAttackingState();
        _returnToIdleState = new BasicMeleeReturnToIdleState();
        // GlobalState.state.AudioManager.RangedEnemyAlertAudio(owner._meleeEnemy.transform.position);
        
    }
    public override void UpdateState(BaseAIMovementController owner)
    {
        base.UpdateState(owner);
        owner._attackRateTimer += Time.deltaTime;
    }
}

/* === ATTACKING STATE === */
public class BasicMeleeAttackingState : BaseAttackingState
{
    public override void EnterState(BaseAIMovementController owner)
    {
        _chasingState = new BasicMeleeChasingState();
    }
    
    public override void UpdateState(BaseAIMovementController owner)
    {
        owner._attackRateTimer += Time.deltaTime;
        
        //float range = owner._attackRange - owner._agent.stoppingDistance;
        float range = owner._attackRange;
        
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

/* === SWING STATE === */
public class BasicMeleeSwingState : State<BaseAIMovementController>
{
    public override void EnterState(BaseAIMovementController owner)
    {
        owner.meleeEnemy.hitboxGroup.enabled = true;
        owner._anim.SetTrigger("Attack");
        owner._canEnterHitStun = false;
    }

    public override void ExitState(BaseAIMovementController owner)
    {
        owner.meleeEnemy.hitboxGroup.enabled = false;
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
            owner.stateMachine.ChangeState(new BasicMeleeAttackingState());
        }
    }
}

/* === RETURN TO IDLE STATE === */
public class BasicMeleeReturnToIdleState : BaseReturnToIdlePosState
{
    public override void EnterState(BaseAIMovementController owner)
    {
        _chasingState = new BasicMeleeChasingState();
        _idleState = new BasicMeleeIdleState();
    }

    public override void ExitState(BaseAIMovementController owner)
    {
        for (int i = 0; i < owner.meleeEnemy._healthBar.transform.childCount; i++)
        {
            owner.meleeEnemy._healthBar.transform.GetChild(i).gameObject.SetActive(false);
        }

        base.ExitState(owner);
    }

    public override void UpdateState(BaseAIMovementController owner)
    {
        base.UpdateState(owner);
        owner._attackRateTimer += Time.deltaTime;
    }

}

/* === HITSTUN STATE === */
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

        // Dont know if we want this feature??
        // owner.GenerateNewAttackTimer(owner.meleeEnemy._attackDelayAfterHitstun);
    }

    public override void UpdateState(BaseAIMovementController owner)
    {
        owner.meleeEnemy._hitStunTimer.Time += Time.deltaTime;

        if (owner.meleeEnemy._hitStunTimer.Expired)
        {
            owner.meleeEnemy._hitStunTimer.Reset();
            owner.stateMachine.ChangeState(new BasicMeleeAttackingState());
        }
    }
}