using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;

public class CombatController : MonoBehaviour
{
    public float parryDuration;
    public float parryLag; // borde vara tied till animationen, samt tror inte att cooldown behövs med tanke på att det ska vara få active frames och mycket lag

    [HideInInspector] public bool successfulParry;
    public float succSpeed;
    public float stoppingDistance;
    public LayerMask enemyLayer;

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
            return anim.GetBool("IsParrying");
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
        //_playerInput.PlayerControls.Attack.performed += ctx => _attack = true;

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

    public void ResetController()
    {
        _animationOver = true;
        stateMachine.ChangeState(new IdleAttackState());
    }


    [SerializeField] private bool succFunctionality;
    public void Attack(GameObject target, bool autoaim)
    {
        Vector3 offset = new Vector3(0, 0.5f, 0);
        Vector3 direction; // Direction of enemy
        bool hit; // Cast a ray with a length of stoppingdistance from player towards enemy

        if (succFunctionality && !_interruptable)
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
                        characterController.Move(new Vector3(direction.normalized.x, 0, direction.normalized.z) * succSpeed * Time.deltaTime);
                    }
                }
            }
        }
        else
        {
            _control.enabled = true;
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
    public void FaceEnemy(GameObject target)
    {
        Vector3 _direction = (target.transform.position - transform.position);
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(_direction.x, 0, _direction.z));
        GlobalState.state.PlayerGameObject.GetComponent<Transform>().rotation = lookRotation;
    }
}

public class IdleAttackState : State<CombatController>
{
    public override void EnterState(CombatController owner)
    {
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
        GlobalState.state.AudioManager.PlayerSwordSwingAudio(owner.transform.position);
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
        //owner._control.enabled = true; // Enable movement controls disabled by attack function
    }

    public override void UpdateState(CombatController owner)
    {
        owner.Attack(_target, true);
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
        GlobalState.state.AudioManager.PlayerSwordSwingAudio(owner.transform.position);
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
        //owner._control.enabled = true;
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
        GlobalState.state.AudioManager.PlayerSwordSwingAudio(owner.transform.position);
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
        //owner._control.enabled = true;
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
    private Timer _parryTimer;
    private Timer _parryLagTimer;

    public override void EnterState(CombatController owner)
    {
        _parryTimer = new Timer(owner.parryDuration);
        _parryLagTimer = new Timer(owner.parryLag);
        GlobalState.state.Player.DisableMovementController();
        owner.anim.SetBool("IsParrying", true);
        owner.anim.SetTrigger("Parry");
    }

    public override void ExitState(CombatController owner)
    {
        owner.successfulParry = false;
        GlobalState.state.Player.EnableMovementController();
        _parryTimer.Reset();
        _parryLagTimer.Reset();
    }

    public override void UpdateState(CombatController owner)
    {
        _parryTimer.Time += Time.deltaTime;
        if (_parryTimer.Expired) // Mathf.Lerp time on successful parry
        {
            Debug.Log("expired");
            owner.anim.SetBool("IsParrying", false);
            _parryLagTimer.Time += Time.deltaTime;
            if (_parryLagTimer.Expired)
            {
                owner.anim.SetTrigger("ParryOver");
                owner.stateMachine.ChangeState(new IdleAttackState());
            }
        }
        else if (owner.successfulParry)
        {
            Debug.Log("Successful parry");
            owner.anim.SetTrigger("ParrySuccessful");
            owner.anim.SetBool("IsParrying", false);
            owner.stateMachine.ChangeState(new IdleAttackState()); // successful parry state, no logic atm
        }
    }
}