using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct InputInfo
{
    public Vector2 direction;
    public bool jump;
}

public class Player : Entity
{
    public PlayerInsanity playerInsanity;
    public LockonFunctionality lockonFunctionality;

    public MovementController movementController;
    public CombatController combatController;

    public InputInfo input;

    private bool _useHitstun;
    private Timer _hitstunTimer;
    private PlayerInput _playerInput;

    public override void TakeDamage(Hitbox hitbox)
    {
        if (combatController.IsParrying)
        {
            print("Parry successful");
            // Parry logic
        }
        else
        {
            EnableHitstun(hitbox.hitstunTime);
            playerInsanity.IncrementInsanity(hitbox.damageValue);
        }
    }

    public override void TakeDamage(float damage)
    {
        playerInsanity.IncrementInsanity(damage);
    }

    private void Awake()
    {
        input = new InputInfo();
        modifier = new HitboxModifier();
        _playerInput = new PlayerInput();
        _playerInput.PlayerControls.Move.performed += ctx => input.direction = ctx.ReadValue<Vector2>();
        _playerInput.PlayerControls.Jump.performed += ctx => input.jump = true;

        PlayerInsanity.onImpendingDoom += ImpendingDoom;
    }
    private void OnEnable() { _playerInput.PlayerControls.Enable(); }
    private void OnDisable() { _playerInput.PlayerControls.Disable(); }

    public void ImpendingDoom()
    {
        print("DOOOOOOOOM");
    }

    public void ResetAnim()
    {
        combatController.EndAnim();
        combatController.anim.SetTrigger("Reset");
    }

    public void EndAnim()
    {
        combatController.EndAnim();
    }

    public void EnableHitstun(float duration)
    {
        if (duration > 0.0f)
        {
            DisableCombatController();
            DisableMovementController();
            _hitstunTimer = new Timer(duration);
            _useHitstun = true;
            combatController.anim.SetTrigger("Hitstun");
            combatController.anim.SetBool("InHitstun", true);

            // play animation
        }
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
