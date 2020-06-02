using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MimicAIScript : BaseAIMovementController
{
    public override void Parried()
    {
        Debug.LogWarning("Parried implementation missing", this);
    }

    public override void KillThis()
    {
        throw new System.NotImplementedException();
    }

    //basicly lägg in attackstates här och ändra hur den gör skit
    public override void TakeDamage(HitboxValues hitbox, Entity attacker)
    {
        throw new System.NotImplementedException();
    }
}
