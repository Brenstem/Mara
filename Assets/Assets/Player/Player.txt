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

    [SerializeField] private GameObject hitEffect;

    public override void TakeDamage(HitboxValues hitbox, Entity attacker = null)
    {
        if (combatController.IsParrying)
        {
            // Parry logic
            combatController.SuccessfulParry();
        }
        else
        {
            EnableHitstun(hitbox.hitstunTime);
            playerInsanity.Damage(hitbox.damageValue);
            GlobalState.state.AudioManager.PlayerHurtAudio(this.transform.position);
        }
    }

    protected override void Awake()
    {
        base.Awake();
        input = new InputInfo();
        modifier = new HitboxModifier();
        _playerInput = new PlayerInput();
        _playerInput.PlayerControls.Move.performed += ctx => input.direction = ctx.ReadValue<Vector2>();
        _playerInput.PlayerControls.Jump.performed += ctx => input.jump = true;

        if (hitEffect != null)
            hitEffect.SetActive(false);
    }
    private void OnEnable() { _playerInput.PlayerControls.Enable(); }
    private void OnDisable() { _playerInput.PlayerControls.Disable(); }


    public bool IsDying;
    public void Death()
    {
        IsDying = true;
        combatController.anim.SetTrigger("Death");
        DisableCombatController();
        DisableLockonFunctionality();
        DisableMovementController();
        combatController.enabled = false;
        movementController.enabled = false;
        GetComponent<CharacterController>().enabled = false;
    }

    public void ImpendingDoom()
    {
        print("DOOOOOOOOM");
    }

    public void ResetAnim()
    {
        combatController.anim.SetTrigger("Reset");
    }

    public void ResetCombatController()
    {
        combatController.ResetController();
    }

    public void ResetMovementController()
    {
        movementController.ResetController();
    }

    public void EndAnim()
    {
        combatController.EndAnim();
    }
    
    public void EnableHitstun(float duration)
    {
        if (duration > 0.0f && !IsDying)
        {
            if (hitEffect != null)
                hitEffect.SetActive(true);

            DisableCombatController();
            DisableMovementController();
            _hitstunTimer = new Timer(duration);
            _useHitstun = true;
            if (duration > 1.0f)
                combatController.anim.SetTrigger("HitstunHeavy");
            else
            {
                //combatController.anim.SetLayerWeight(1, 1.0f);
                combatController.anim.SetTrigger("HitstunLight");
            }
            combatController.anim.SetBool("InHitstun", true);

            // play animation
        }
    }

    public void DisableHitstun()
    {
        _useHitstun = false;

        if (hitEffect != null)
            hitEffect.SetActive(false);
        if (!IsDying)
        {
            EnableMovementController();
            EnableCombatController();
        }
        //combatController.anim.SetLayerWeight(1, 0.0f);
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

    public void EnableMovementController()
    {
        movementController.enabled = true;
        if (movementController.isLockedOn)
            movementController.stateMachine.ChangeState(new StrafeMovementState());
    }
    public void DisableMovementController() { movementController.enabled = false; }

    public void EnableLockonFunctionality() { lockonFunctionality.enabled = true; }
    public void DisableLockonFunctionality() { lockonFunctionality.enabled = false; }

    public override void KillThis()
    {
        throw new System.NotImplementedException();
    }
}
