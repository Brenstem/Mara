﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEventHandler : MonoBehaviour
{
    public delegate void OnAnimationOver();
    public static event OnAnimationOver onAnimationOver;

    public delegate void OnIASA();
    public static event OnIASA onIASA;

    public delegate void OnAttackStep();
    public static event OnAttackStep onAttackStep;

    public delegate void OnWalkCancel();
    public static event OnWalkCancel onWalkCancel;

    public delegate void OnAttackStepEnd();
    public static event OnAttackStepEnd onAttackStepEnd;

    public delegate void PlayerDeath();
    public static event PlayerDeath onPlayerDeath;

    public void EndAnim()
    {
        if (onAnimationOver != null)
        {
            onAnimationOver();
        }
    }

    public void IASA()
    {
        if (onIASA != null)
        {
            onIASA();
        }
    }

    public void AttackStep()
    {
        if (onAttackStep != null)
        {
            onAttackStep();
        }
    }

    public void AttackStepEnd()
    {
        if (onAttackStepEnd != null)
        {
            onAttackStepEnd();
        }
    }

    public void PlayerDead()
    {
        if (onPlayerDeath != null)
        {
            onPlayerDeath();
        }
    }

    public void WalkCancel()
    {
        if (onWalkCancel != null)
        {
            onWalkCancel();
        }
    }
}
