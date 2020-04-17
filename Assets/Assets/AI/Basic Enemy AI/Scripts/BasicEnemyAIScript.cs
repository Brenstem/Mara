using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BasicEnemyAIScript : BaseAIMovementController
{

    public class AttackinState : BaseAttackingState
    {
        public override void UpdateState(BaseAIMovementController owner)
        {
            Debug.Log("meme");
        }
    }
}

