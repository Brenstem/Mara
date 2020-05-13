using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowAI : BaseAIMovementController
{
    protected override void Awake()
    {
        base.Awake();
        _anim = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        PlayerInsanity.onHallucination += EnableThis;
        PlayerInsanity.onDisableShadows += DisableThis;
        stateMachine.ChangeState(new ShadowEnemyIdleState());
        DisableThis();
    }

    protected override void Update()
    {

        _anim.SetFloat("Blend", _agent.velocity.magnitude);
        base.Update();
    }

    public override void KillThis()
    {
        _anim.SetTrigger("Dead");
        stateMachine.ChangeState(new DeadState());
    }

    public override void Parried()
    {
        Debug.LogWarning("Parried implementation missing", this);
    }

    private void DisableThis()
    {
        GetComponent<CapsuleCollider>().enabled = false;

        foreach (Transform child in this.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    private void EnableThis()
    {
        print("enable this");
        GetComponent<CapsuleCollider>().enabled = true;

        foreach (Transform child in this.transform)
        {
            child.gameObject.SetActive(true);
        }
    }


}

public class ShadowEnemyIdleState : BaseIdleState
{
    public override void EnterState(BaseAIMovementController owner)
    {
        _chasingState = new ShadowEnemyChasingState();
    }
}

public class ShadowEnemyChasingState : BaseChasingState
{
    public override void EnterState(BaseAIMovementController owner)
    {
        _attackingState = new ShadowEnemyAttackingState();
        _returnToIdleState = new ShadowEnemyReturnToIdleState();
    }
}

public class ShadowEnemyAttackingState : BaseAttackingState
{
    public override void EnterState(BaseAIMovementController owner)
    {
        _chasingState = new ShadowEnemyChasingState();
    }
}

public class ShadowEnemyReturnToIdleState : BaseReturnToIdlePosState
{
    public override void EnterState(BaseAIMovementController owner)
    {
        _chasingState = new ShadowEnemyChasingState();
        _idleState = new ShadowEnemyIdleState();
    }
}
