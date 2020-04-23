using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowAI : BaseAIMovementController
{
    protected override void Awake()
    {
        base.Awake();
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
        GetComponent<CapsuleCollider>().enabled = true;

        foreach (Transform child in this.transform)
        {
            child.gameObject.SetActive(true);
        }
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
        base.Update();
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
