using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class CombatController : MonoBehaviour
{
    public float _dashAttackSpeed;
    public LayerMask enemyLayer;
    public float stoppingDistance;

    private bool _temp;
    private PlayerInput _playerInput;

    public HitboxGroup hitboxGroup;

    [HideInInspector] public bool _attack;
    [HideInInspector] public bool _animationOver;
    [HideInInspector] public MovementController _control;
    [HideInInspector] public StateMachine<CombatController> stateMachine;
    [HideInInspector] public Animator anim;
    [HideInInspector] public TargetFinder TargetFinder;
    [HideInInspector] public CharacterController characterController;

    private void OnEnable() { _playerInput.Enable(); }

    private void OnDisable() { _playerInput.Disable(); }

    private void Awake()
    {
        _playerInput = new PlayerInput();
        _playerInput.PlayerControls.Attack.performed += ctx => _attack = true;

        stateMachine = new StateMachine<CombatController>(this);

        anim = GetComponent<Animator>();
        characterController = GetComponentInParent<CharacterController>();
        _control = GetComponentInParent<MovementController>();
        TargetFinder = GetComponent<TargetFinder>();

        stateMachine.ChangeState(new IdleAttackState());
    }

    void Update()
    {
        stateMachine.Update();
        print("Animation is over: " + _animationOver);
        print(stateMachine.currentState);
        
    }

    public void EndAnim()
    {
        print("End animation");
        _animationOver = true;
    }
}


public class IdleAttackState : State<CombatController>
{
    public override void EnterState(CombatController owner) {  }

    public override void ExitState(CombatController owner) {  }

    public override void UpdateState(CombatController owner)
    {
        owner._animationOver = false;

        if (owner._attack)
        {
            if (GlobalState.state.Player.GetComponent<MovementController>().isLockedOn)
            {
                // owner.stateMachine.ChangeState(new LockedAttackState());
            }
            else 
            {
                owner.stateMachine.ChangeState(new AttackState());
            }
        }
    }
}

public class AttackState : State<CombatController>
{
    private GameObject _target;
    private Vector3 _direction;
    private Vector3 _offset = new Vector3(0, 0.5f, 0);
    private bool hit;

    public override void EnterState(CombatController owner)
    {
        Attack(owner);
        _target = owner.TargetFinder.FindTarget(); // Active target
    }

    public override void ExitState(CombatController owner) 
    {
        owner._animationOver = false;
        owner._attack = false;
        Debug.Log("Exit State: " + owner._animationOver);
    }

    // Pulls player toward enemy "AutoAim"
    public override void UpdateState(CombatController owner)
    {
        if (!_target) // If there is no target swing at air
        {
            Debug.Log("No target");
            owner.stateMachine.ChangeState(new IdleAttackState());
        }
        else if (owner._animationOver) // If attack animation ended go to idlestate
        {
            owner._control.enabled = true;
            owner.stateMachine.ChangeState(new IdleAttackState());
        }
        else if (hit = Physics.Raycast(owner.transform.position + _offset, _target.transform.position, owner.stoppingDistance, owner.enemyLayer)) // If target is in attack range face target and swing
        {
            Debug.Log("Enemy in range");
            FaceEnemy(owner);
            owner.stateMachine.ChangeState(new IdleAttackState());
        } 
        else
        {
            Debug.Log("Target: " + _target);

            owner._control.enabled = false;
            FaceEnemy(owner);

            Vector3 direction = _target.transform.position - owner.transform.position; // Direction of enemy

            hit = Physics.Raycast(owner.transform.position + _offset, direction, owner.stoppingDistance, owner.enemyLayer); // Cast a ray with a length of stoppingdistance from player towards enemy

            Debug.Log("Autoaiming...");

            if (!hit) // If raycast missed move towards enemy
            {
                Debug.Log("Moving towards enemy...");

                owner.characterController.Move(new Vector3(_direction.normalized.x, 0, _direction.normalized.z) * owner._dashAttackSpeed * Time.deltaTime);
            }
        }
    }

    // Character looks at enemy
    private void FaceEnemy(CombatController owner) 
    {
        _direction = (_target.transform.position - owner.transform.position);
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(_direction.x, 0, _direction.z));
        GlobalState.state.Player.GetComponent<Transform>().rotation = lookRotation;
    }

    // Run the attack animations and enable hitboxes
    public void Attack(CombatController owner)
    {
        owner.hitboxGroup.enabled = true;
        owner.anim.SetTrigger("Attack");
    }
}

public class LockedAttackState : State<CombatController>
{
    public override void EnterState(CombatController owner) 
    {
        owner.hitboxGroup.enabled = true;
        owner.anim.SetTrigger("Attack");
    }

    public override void ExitState(CombatController owner)
    {
        owner._animationOver = false;
        owner._attack = false;
    }

    public override void UpdateState(CombatController owner)
    {
        if (owner._animationOver)
        {
            owner.stateMachine.ChangeState(new IdleAttackState());
        }
    }
}