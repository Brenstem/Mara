using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemyAI : BaseAIMovementController
{
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