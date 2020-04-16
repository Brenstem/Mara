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
        print(_animationOver);
    }

    public void EndAnim()
    {
        _animationOver = true;
        print("EndAnim");
    }

    public void Attack(GameObject target)
    {
        Vector3 offset = new Vector3(0, 0.5f, 0);
        Vector3 direction; // Direction of enemy
        bool hit; // Cast a ray with a length of stoppingdistance from player towards enemy

        foreach (HitboxGroup group in hitboxGroups)
        {
            group.enabled = true;
        }

        if (!target) // If there is no target swing at air
        {
            // wait for animation and go to idle
            if (_animationOver)
            {
                stateMachine.ChangeState(new IdleAttackState());
            }
        }
        else
        {
            direction = target.transform.position - transform.position;
            hit = Physics.Raycast(transform.position + offset, direction, stoppingDistance, enemyLayer);

            // Check if target is in range
            if (hit = Physics.Raycast(transform.position + offset, target.transform.position, stoppingDistance, enemyLayer)) // If target is in attack range face target and swing
            {
                FaceEnemy(target);
                _control.enabled = true;
                stateMachine.ChangeState(new IdleAttackState());
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
public class AttackState : State<CombatController>
{
    private GameObject _target;
    private Vector3 _direction;
    private Vector3 _offset = new Vector3(0, 0.5f, 0);
    private bool hit;

    public override void EnterState(CombatController owner)
    {
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
}

public class IdleAttackState : State<CombatController>
{
    public override void EnterState(CombatController owner) { }

    public override void ExitState(CombatController owner) { }

    public override void UpdateState(CombatController owner)
    {
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
        owner.anim.SetBool("DoubleCombo", false);
        owner.anim.SetTrigger("Attack");
        _target = owner.TargetFinder.FindTarget();
    }

    public override void ExitState(CombatController owner)
    {
        if (_doubleCombo)
        {
            owner.anim.SetBool("DoubleCombo", true);
            _doubleCombo = false;
        }

        owner._animationOver = false;
        owner._attack = false;
        owner._control.enabled = true;
    }

    public override void UpdateState(CombatController owner)
    {
        owner.Attack(_target);

        if (!owner._animationOver && Input.GetMouseButtonDown(0))
        {
            _doubleCombo = true;
        }

        if (owner._animationOver && !_doubleCombo)
        {
            Debug.Log("go back to idle from first attack");
            owner.stateMachine.ChangeState(new IdleAttackState());
        }
        else if (owner._animationOver)
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
        _target = owner.TargetFinder.FindTarget();
        owner.anim.SetBool("TripleCombo", false);
    }

    public override void ExitState(CombatController owner)
    {
        if (_tripleCombo)
        {
            owner.anim.SetBool("TripleCombo", true);
            _tripleCombo = false;
        }

        owner._animationOver = false;
        owner._attack = false;
        owner._control.enabled = true;
    }

    public override void UpdateState(CombatController owner)
    {
        owner.Attack(_target);

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
    }

    public override void ExitState(CombatController owner)
    {
        owner._animationOver = false;
        owner._attack = false;
        owner._control.enabled = true;
    }

    public override void UpdateState(CombatController owner)
    {
        owner.Attack(_target);

        if (owner._animationOver)
        {
            owner.stateMachine.ChangeState(new IdleAttackState());
        }
    }
}