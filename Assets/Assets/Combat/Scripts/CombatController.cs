using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class CombatController : MonoBehaviour
{
    public bool successfulParry;
    public float _dashAttackSpeed;
    public LayerMask enemyLayer;
    public float stoppingDistance;

    private bool _temp;
    private PlayerInput _playerInput;

    [SerializeField] public List<HitboxGroup> hitboxGroups;
    
    [HideInInspector] public bool _attack;
    [HideInInspector] public bool _animationOver;
    [HideInInspector] public bool _interruptable; // Interruptible As Soon As (IASA)
    [HideInInspector] public MovementController _control;
    [HideInInspector] public StateMachine<CombatController> stateMachine;
    [HideInInspector] public Animator anim;
    [HideInInspector] public TargetFinder TargetFinder;
    [HideInInspector] public CharacterController characterController;

    public bool IsParrying
    {
        get
        {
            if (stateMachine.currentState.GetType() == typeof(ParryState))
                return true;
            else
                return false;
        }
    }

    private void OnEnable() { _playerInput.Enable(); }

    private void OnDisable()
    {
        _playerInput.Disable();
        foreach (HitboxGroup group in hitboxGroups)
        {
            group.hitboxEventHandler.EndAnim();
            group.enabled = false;
        }
        stateMachine.ChangeState(new IdleAttackState());
    }

    private void Awake()
    {   
        _playerInput = new PlayerInput();
        _playerInput.PlayerControls.Attack.performed += ctx => _attack = true;

        stateMachine = new StateMachine<CombatController>(this);

        anim = GetComponent<Animator>();
        characterController = GetComponentInParent<CharacterController>();
        _control = GlobalState.state.Player.movementController;
        TargetFinder = GetComponent<TargetFinder>();

        stateMachine.ChangeState(new IdleAttackState());
    }

    void Update()
    {
        stateMachine.Update();
    }

    public void EndAnim()
    {
        _animationOver = true;
    }

    public void IASA()
    {
        _interruptable = true;
    }

    public void SuccessfulParry()
    {
        successfulParry = true;
    }

    public void Attack(GameObject target, bool autoaim)
    {
        Vector3 offset = new Vector3(0, 0.5f, 0);
        Vector3 direction; // Direction of enemy
        bool hit; // Cast a ray with a length of stoppingdistance from player towards enemy
        
        if (_interruptable && GlobalState.state.Player.input.direction != Vector2.zero)
        {
            _control.enabled = true;
        }
        else
        {
            if (target && autoaim && !_control.isLockedOn) // If there is target swing
            {
                _control.enabled = false;
                direction = target.transform.position - transform.position;
                hit = Physics.Raycast(transform.position + offset, direction, stoppingDistance, enemyLayer);


                if (hit) // If target is in attack range face target and swing
                {
                    FaceEnemy(target);
                    _control.enabled = true;
                }
                else
                {
                    FaceEnemy(target);

                    if (!hit) // If raycast missed move towards enemy
                    {
                        characterController.Move(new Vector3(direction.normalized.x, 0, direction.normalized.z) * _dashAttackSpeed * Time.deltaTime);
                    }
                }
            }
        }
    }

    public GameObject FindTarget()
    {
        GameObject _target = TargetFinder.FindTarget(); // Find target to pass to attack function in first frame of attack
        if (_target != null)
        {
            var t = GlobalState.state.Player.lockonFunctionality.Target;
            if (t != null)
            {
                if (Vector3.Distance(transform.position, t.position) <= TargetFinder.trackRadius)
                    _target = t.gameObject;
            }
        }
        return _target;
    }

    // Character looks at enemy
    private void FaceEnemy(GameObject target)
    {
        Vector3 _direction = (target.transform.position - transform.position);
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(_direction.x, 0, _direction.z));
        GlobalState.state.PlayerGameObject.GetComponent<Transform>().rotation = lookRotation;
    }
}

public class IdleAttackState : State<CombatController>
{
    public override void EnterState(CombatController owner) {
        owner.anim.SetBool("CombatIdle", true);
    }

    public override void ExitState(CombatController owner)
    {
        owner.anim.SetBool("CombatIdle", false);
    }

    public override void UpdateState(CombatController owner)
    {
        owner._animationOver = false;
        
        if (Input.GetMouseButtonDown(0))
        {
            owner.stateMachine.ChangeState(new FirstAttackState());
        }
        else if (Input.GetMouseButtonDown(1))
        {
            owner.stateMachine.ChangeState(new ParryState());
        }
    }
}

public class FirstAttackState : State<CombatController>
{
    private bool _doubleCombo;
    private GameObject _target;

    public override void EnterState(CombatController owner)
    {
        GlobalState.state.Player.ResetAnim();
        owner.anim.SetBool("DoubleCombo", false); // Reset the double combo animation bool upon entering state
        owner.anim.SetTrigger("Attack"); // Set animation trigger for first attack
        _target = owner.FindTarget();
        owner._animationOver = false;
        owner._interruptable = false;
        owner.hitboxGroups[0].enabled = true;
    }

    public override void ExitState(CombatController owner)
    {
        if (_doubleCombo)
        {
            owner.anim.SetBool("DoubleCombo", _doubleCombo); // Set animation bool
            _doubleCombo = false; // Reset member variable
        }
        owner.hitboxGroups[0].enabled = false;
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
        else if (owner._animationOver || (owner._interruptable && _doubleCombo)) // If animation has ended and double combo is true go to second attack
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
        _target = owner.FindTarget(); // Find target to pass to attack function in first frame of attack
        owner._animationOver = false;
        owner._interruptable = false;
        if (owner.hitboxGroups.Count >= 2)
        {
            owner.hitboxGroups[1].enabled = true;
        }
        else
        {
            owner.hitboxGroups[owner.hitboxGroups.Count].enabled = true;
        }
    }

    public override void ExitState(CombatController owner)
    {
        if (_tripleCombo)
        {
            owner.anim.SetBool("TripleCombo", _tripleCombo);
            _tripleCombo = false;
        }

        if (owner.hitboxGroups.Count >= 2)
        {
            owner.hitboxGroups[1].enabled = false;
        }
        else
        {
            owner.hitboxGroups[owner.hitboxGroups.Count].enabled = false;
        }
        owner._control.enabled = true;
    }

    public override void UpdateState(CombatController owner)
    {
        owner.Attack(_target, true);

        if (!owner._animationOver && Input.GetMouseButtonDown(0))
        {
            _tripleCombo = true;
        }

        if (owner._animationOver && !_tripleCombo)
        {
            owner.stateMachine.ChangeState(new IdleAttackState());
        }
        else if (owner._animationOver || (owner._interruptable && _tripleCombo))
        {
            owner.stateMachine.ChangeState(new ThirdAttackState());
        }
    }
}

public class ThirdAttackState : State<CombatController>
{
    private bool _tripleCombo;
    private GameObject _target;

    public override void EnterState(CombatController owner)
    {
        _target = owner.FindTarget(); // Find target to pass to attack function in first frame of attack
        owner._animationOver = false;
        owner._interruptable = false;
        if (owner.hitboxGroups.Count >= 3)
        {
            owner.hitboxGroups[2].enabled = true;
        }
        else
        {
            owner.hitboxGroups[owner.hitboxGroups.Count].enabled = true;
        }
    }

    public override void ExitState(CombatController owner)
    {
        if (owner.hitboxGroups.Count >= 3)
        {
            owner.hitboxGroups[2].enabled = false;
        }
        else
        {
            owner.hitboxGroups[owner.hitboxGroups.Count].enabled = false;
        }
        owner._control.enabled = true;
    }

    public override void UpdateState(CombatController owner)
    {
        owner.Attack(_target, true);

        if (owner._animationOver)
        {
            owner.stateMachine.ChangeState(new IdleAttackState());
        }
    }
}

public class ParryState : State<CombatController>
{
    private Timer _timer;
    public override void EnterState(CombatController owner)
    {
        float parryTime = 1.0f;
        _timer = new Timer(parryTime);
        GlobalState.state.Player.DisableMovementController();
        owner.anim.SetTrigger("Parry");
    }

    public override void ExitState(CombatController owner)
    {
        owner.anim.SetBool("IsParrying", false);
        owner.successfulParry = false;
        GlobalState.state.Player.EnableMovementController();
        _timer.Reset();
    }

    public override void UpdateState(CombatController owner)
    {
        _timer.Time += Time.deltaTime;
        owner.anim.SetBool("IsParrying", true);
        if (_timer.Expired || owner.successfulParry) // Mathf.Lerp time on successful parry
        {
            //Lag men det gör man sen typ
            owner.stateMachine.ChangeState(new IdleAttackState());
        }

    }
}