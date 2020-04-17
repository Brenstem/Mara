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

    [SerializeField] public List<HitboxGroup> hitboxGroups;

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
        print(stateMachine.currentState);
    }

    public void EndAnim()
    {
        _animationOver = true;
        print("EndAnim");
    }

    public void Attack(GameObject target, bool autoaim)
    {
        Vector3 offset = new Vector3(0, 0.5f, 0);
        Vector3 direction; // Direction of enemy
        bool hit; // Cast a ray with a length of stoppingdistance from player towards enemy

        foreach (HitboxGroup group in hitboxGroups)
        {
            group.enabled = true;
        }

        if (target && autoaim) // If there is target swing
        {
            direction = target.transform.position - transform.position;
            hit = Physics.Raycast(transform.position + offset, direction, stoppingDistance, enemyLayer);

            // Check if target is in range
            if (hit = Physics.Raycast(transform.position + offset, target.transform.position, stoppingDistance, enemyLayer)) // If target is in attack range face target and swing
            {
                FaceEnemy(target);
                _control.enabled = true;
            }
            else
            {
                _control.enabled = false;
                FaceEnemy(target);

                if (!hit) // If raycast missed move towards enemy
                {
                    characterController.Move(new Vector3(direction.normalized.x, 0, direction.normalized.z) * _dashAttackSpeed * Time.deltaTime);
                }
            }
        }
    }

    // Character looks at enemy
    private void FaceEnemy(GameObject target)
    {
        Vector3 _direction = (target.transform.position - transform.position);
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(_direction.x, 0, _direction.z));
        GlobalState.state.Player.GetComponent<Transform>().rotation = lookRotation;
    }
}

public class IdleAttackState : State<CombatController>
{
    public override void EnterState(CombatController owner) { }

    public override void ExitState(CombatController owner) { }

    public override void UpdateState(CombatController owner)
    {
        owner._animationOver = false;


        if (Input.GetMouseButtonDown(0))
        {
            owner.stateMachine.ChangeState(new FirstAttackState());
        }
    }
}

public class FirstAttackState : State<CombatController>
{
    private bool _doubleCombo;
    private GameObject _target;

    public override void EnterState(CombatController owner)
    {
        owner.anim.SetBool("DoubleCombo", false); // Reset the double combo animation bool upon entering state
        owner.anim.SetTrigger("Attack"); // Set animation trigger for first attack
        _target = owner.TargetFinder.FindTarget(); // Find target to pass to attack function in first frame of attack
    }

    public override void ExitState(CombatController owner)
    {
        if (_doubleCombo)
        {
            owner.anim.SetBool("DoubleCombo", true); // Set animation bool
            _doubleCombo = false; // Reset member variable
        }

        owner._control.enabled = true; // Enable movement controls disabled by attack function
    }

    public override void UpdateState(CombatController owner)
    {
        owner.Attack(_target, true); // Attack using target found in first frame
         
        if (!owner._animationOver && Input.GetMouseButtonDown(0)) // If the player presses mouse0 when animation is running set doublecombo to true
        {
            _doubleCombo = true;
        }

        if (owner._animationOver && !_doubleCombo) // If the animation has ended and doublecombo is not true go to idle
        {
            owner.stateMachine.ChangeState(new IdleAttackState());
        }
        else if (owner._animationOver) // If animation has ended and double combo is true go to second attack
        {
            owner.stateMachine.ChangeState(new SecondAttackState());
        }
    }
}

public class SecondAttackState : State<CombatController>
{
    private bool _tripleCombo;
    private GameObject _target;

    public override void EnterState(CombatController owner)
    {
        // Animation bool was set to true in last states exit

        owner.anim.SetBool("TripleCombo", false); // Reset triple combo animation bool upon entering state
        _target = owner.TargetFinder.FindTarget(); // Find target to pass to attack function in first frame of attack
        owner._animationOver = false;
    }

    public override void ExitState(CombatController owner)
    {
        if (_tripleCombo)
        {
            owner.anim.SetBool("TripleCombo", true);
            _tripleCombo = false;
        }

        owner._control.enabled = true;
    }

    public override void UpdateState(CombatController owner)
    {
        owner.Attack(_target, true);

        if (!owner._animationOver && Input.GetMouseButtonDown(0))
        {
            Debug.Log("triple combo");
            _tripleCombo = true;
        }

        if (owner._animationOver && !_tripleCombo)
        {
            owner.stateMachine.ChangeState(new IdleAttackState());
        }
        else if (owner._animationOver)
        {
            owner.stateMachine.ChangeState(new ThirdAttackState());
        }
    }
}

public class ThirdAttackState : State<CombatController>
{
    private GameObject _target;

    public override void EnterState(CombatController owner)
    {
        _target = owner.TargetFinder.FindTarget();
        owner._animationOver = false;
    }

    public override void ExitState(CombatController owner)
    {
        owner._attack = false;
        owner._control.enabled = true;
    }

    public override void UpdateState(CombatController owner)
    {
        owner.Attack(_target, false);

        if (owner._animationOver)
        {
            owner.stateMachine.ChangeState(new IdleAttackState());
        }
    }
}


