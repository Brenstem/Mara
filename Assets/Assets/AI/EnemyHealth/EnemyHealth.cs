﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : EntityHealth
{   
    public override void Damage(HitboxValues hitbox)
    {
        CurrentHealth -= hitbox.damageValue;
    }
}
