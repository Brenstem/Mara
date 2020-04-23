using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MimicAIScript : BaseAIMovementController
{
    //basicly lägg in attackstates här och ändra hur den gör skit
    public override void TakeDamage(Hitbox hitbox)
    {
        throw new System.NotImplementedException();
    }

    public override void TakeDamage(float damage)
    {
        throw new System.NotImplementedException();
    }
}
