using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMeleeAI : BaseAIMovementController
{

    Animator anim;

    [HideInInspector]public Timer _hitStunTimer;
    [HideInInspector]public bool _useHitStun;

    // Start is called before the first frame update
    void Start()
    {
        stateMachine.ChangeState(new BasicMeleeIdleState());
        anim = GetComponent<Animator>();
        _meleeEnemy = this;
    }

    public void Attack()
    {
        anim.SetTrigger("Attack");
    }

    public override void TakeDamage(Hitbox hitbox)
    {

        print("i took damage");
        stateMachine.ChangeState(new BasicMeleeIdleState());
        EnableHitstun(1f);
        base.TakeDamage(hitbox);
    }

    public void EnableHitstun(float duration)
    {
        _hitStunTimer = new Timer(duration);
        _useHitStun = true;
        anim.SetTrigger("Hitstun");
        anim.SetBool("InHitstun", true);
    }

    public void DisabelHitStun()
    {
        _useHitStun = false;
        anim.SetBool("InHitstun", false);
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
        if (owner._meleeEnemy._useHitStun)
        {
            owner._meleeEnemy._hitStunTimer.Time += Time.deltaTime;
            if (owner._meleeEnemy._hitStunTimer.Expired)
            {
                owner._meleeEnemy._hitStunTimer.Reset();
                owner._meleeEnemy.DisabelHitStun();
            }
        }
        else
        {
            base.UpdateState(owner);
        }

    }
}

public class BasicMeleeChasingState : BaseChasingState
{
    public override void EnterState(BaseAIMovementController owner)
    {
        _attackingState = new BasicMeleeAttackingState();
        _returnToIdleState = new BasicMeleeReturnToIdleState();
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
        //lägg in attack metod här
        owner._meleeEnemy.Attack();
        base.UpdateState(owner);
    }
}

public class BasicMeleeReturnToIdleState : BaseReturnToIdlePosState
{
    public override void EnterState(BaseAIMovementController owner)
    {
        _chasingState = new BasicMeleeChasingState();
        _idleState = new BasicMeleeIdleState();

    }
}
