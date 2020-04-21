using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
{
    public PlayerInsanity playerInsanity;
    public LockonFunctionality lockonFunctionality;

    public MovementController movementController;
    public CombatController combatController;


    private bool _useHitstun;
    private Timer _hitstunTimer;

    public override void TakeDamage(Hitbox hitbox)
    {
        if (combatController.IsParrying)
        {
            print("Parry successful");
            // Parry logic
        }
        else
        {
            // hitstun?
            EnableHitstun(0.3f);
            playerInsanity.IncrementInsanity(hitbox.damageValue);
        }
    }

    public override void TakeDamage(float damage)
    {
        playerInsanity.IncrementInsanity(damage);
    }

    private void Awake()
    {
        PlayerInsanity.OnImpendingDoom += ImpendingDoom;
    }

    public void ImpendingDoom()
    {
        print("DOOOOOOOOM");
    }

    public void EnableHitstun(float duration)
    {
        DisableCombatController();
        DisableMovementController();
        _hitstunTimer = new Timer(duration);
        _useHitstun = true;
        combatController.anim.SetTrigger("Hitstun");
        combatController.anim.SetBool("InHitstun", true);
        // play animation
    }

    public void DisableHitstun()
    {
        _useHitstun = false;
        EnableMovementController();
        EnableCombatController();
        combatController.anim.SetBool("InHitstun", false);
    }

    private void Update()
    {
        if (_useHitstun)
        {
            _hitstunTimer.Time += Time.deltaTime;
            if (_hitstunTimer.Expired)
            {
                _hitstunTimer.Reset();
                DisableHitstun();
            }
        }
    }

    public void EnableCombatController() { combatController.enabled = true; }
    public void DisableCombatController() { combatController.enabled = false; }

    public void EnableMovementController() { movementController.enabled = true; }
    public void DisableMovementController() { movementController.enabled = false; }

    public void EnableLockonFunctionality() { lockonFunctionality.enabled = true; }
    public void DisableLockonFunctionality() { lockonFunctionality.enabled = false; }
}
