using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemyAI : BaseAIMovementController
{
    public override void TakeDamage(Hitbox hitbox)
    {
        throw new System.NotImplementedException();
    }

    public override void TakeDamage(float damage)
    {
        throw new System.NotImplementedException();
    }

    private void Start()
    {
        stateMachine.ChangeState(new BasicEnemyIdle());
    }
}

public class BasicEnemyIdle : BaseIdleState
{
}

public class BasicEnemyChase : BaseChasingState
{
}