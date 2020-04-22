using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowAI : BaseAIMovementController
{
    protected override void Awake()
    {
        PlayerInsanity.onHallucination += EnableThis;
        PlayerInsanity.onDisableShadows += DisableThis;
        base.Awake();
    }

    private void DisableThis()
    {
        print("disable");
        print(this.transform.GetComponent<CapsuleCollider>());

        foreach (Transform child in this.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    private void EnableThis()
    {
        foreach (Transform child in this.transform)
        {
            child.gameObject.SetActive(true);
        }
    }

    private void Start()
    {
        stateMachine.ChangeState(new ShadowEnemyIdleState());
    }

    protected override void Update()
    {
        print(this.transform.GetComponent<CapsuleCollider>());

        base.Update();
        print(stateMachine.currentState);
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
