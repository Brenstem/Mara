using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMeleeAI : BaseAIMovementController
{

    Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        stateMachine.ChangeState(new BasicMeleeIdleState());
        anim = GetComponent<Animator>();
        meleeEnemy = this;
    }

    public void Attack()
    {
        anim.SetTrigger("Attack");
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
        owner.meleeEnemy.Attack();

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
