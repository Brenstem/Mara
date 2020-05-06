using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// alert kan man gå in i när man nyligen har tagit skada inom en viss tid samt när fienden har aggro på en och man är skadad eller whatever
public class PlayerRevamp : Entity
{
    #region Inspector
    /* === DEBUG === */
    [Header("Debug")]
    [SerializeField] private bool _lockCursor = true;

    /* === INSPECTOR VARIABLES === */
    [Header("References and logic")]
    public Animator playerAnimator;
    public Animator cameraAnimator;
    [SerializeField] private Transform _groundCheckPosition;
    [SerializeField] private Cinemachine.CinemachineFreeLook _freeLookCam;
    [SerializeField] private Cinemachine.CinemachineFreeLook _lockonCam;
    [SerializeField] private TargetFinder _targetFinder;
    [SerializeField, Range(1, 20)] private int inputBufferSize = 1;

    [Header("Ground Check")]
    [SerializeField] private float _groundDistance = 0.4f;

    private Vector3 _velocity;
    private bool _isGrounded;
    public bool IsGrounded { get { return _isGrounded; } }

    [Header("Movement"), Space(20)]
    private float _originalMaxSpeed;
    public float maxSpeed = 5f;
    public float rotationSpeed = 15f;
    public float rotationAngleUntilMove = 30;

    [Header("Jump")]
    [SerializeField] private float _gravity = -9.82f;
    [SerializeField] private float _jumpHeight = 3f;
    public float airRotationSpeed = 4f;
    public float airSpeed = 5f;

    [Header("Dash Properties")]
    [SerializeField] private float _dashCooldownTime = 0.25f;
    [SerializeField] private int airDashAmount;
    public float dashTime = 0.25f;
    public float dashLag = 0.15f;
    public float dashSpeed = 10.0f;
    public float dashAnimationNumerator = 5;
    private Timer _dashCooldownTimer;
    private int _airDashes;
    public bool invulerableWhenDashing;

    [Header("Combat")]
    public float heavyHitstunThreshold = 1.0f;
    public float parryDuration;
    public float parryLag; // borde vara tied till animationen, samt tror inte att cooldown behövs med tanke på att det ska vara få active frames och mycket lag

    [Header("Light Attack 1")]
    public HitboxGroup light1HitboxGroup;
    public float light1StepSpeed;

    [Header("Light Attack 2")]
    public HitboxGroup light2HitboxGroup;
    public float light2StepSpeed;

    [Header("Heavy Attack")]
    public HitboxGroup heavyHitboxGroup;
    public float heavyStepSpeed;

    [Header("Insanity events")]
    [SerializeField] float _moveSpeedBuffMultiplier = 1.1f;
    [SerializeField] float _moveSpeedDebuffMultiplier = 0.9f;

    #endregion

    /* === COMPONENT REFERENCES === */
    private PlayerInput _playerInput;

    [HideInInspector] public Transform pointOfInterest;
    [HideInInspector] public StateMachine<PlayerRevamp> stateMachine;
    [HideInInspector] public CharacterController controller;

    /* === INPUT === */
    private Vector2 _input;
    public Vector2 Input { get { return _input; } }

    public enum InputType
    {
        Idle = -1,
        Dash = 0,
        Jump = 1,
        AttackLight = 2,
        AttackHeavy = 3,
        Parry = 4,
    }

    [HideInInspector] public CircularBuffer<InputType> inputBuffer;
    [HideInInspector] public bool dashPerformed;
    [HideInInspector] public bool jumpPerformed;
    [HideInInspector] public bool useGravity = true;

    /* === LOCK ON === */
    private bool _doSnapCamera;
    private bool _lockedOn;

    public bool IsLockedOn { get { return _lockedOn; } }

    private void OnEnable() { _playerInput.PlayerControls.Enable(); }
    private void OnDisable() { _playerInput.PlayerControls.Disable(); }

    /* === COMBAT === */
    [HideInInspector] public bool interruptable;
    [HideInInspector] public bool attackAnimationOver;
    [HideInInspector] public bool attackStep;
    [HideInInspector] public float hitstunDuration;
    [HideInInspector] public bool isParrying;

    private new void Awake()
    {
        base.Awake();

        if (_lockCursor)
            Cursor.lockState = CursorLockMode.Locked;

        _originalMaxSpeed = maxSpeed;

        controller = GetComponent<CharacterController>();
        modifier = new HitboxModifier();
        _playerInput = new PlayerInput();
        stateMachine = new StateMachine<PlayerRevamp>(this);
        stateMachine.ChangeState(new IdleState());

        inputBuffer = new CircularBuffer<InputType>(inputBufferSize);

        // Dash
        _dashCooldownTimer = new Timer(_dashCooldownTime);
        _dashCooldownTimer.Time += _dashCooldownTime;

        /* === INPUT === */
        _playerInput.PlayerControls.Move.performed += performedInput => _input = performedInput.ReadValue<Vector2>();
        _playerInput.PlayerControls.Dash.performed += performedInput => Action(InputType.Dash);
        _playerInput.PlayerControls.Jump.performed += performedInput => Action(InputType.Jump);
        _playerInput.PlayerControls.AttackLight.performed += performedInput => Action(InputType.AttackLight);
        _playerInput.PlayerControls.AttackHeavy.performed += performedInput => Action(InputType.AttackHeavy);
        _playerInput.PlayerControls.Parry.performed += performedInput => Action(InputType.Parry);

        /* === PLAYER ANIM EVENTS === */
        PlayerAnimationEventHandler.onAnimationOver += AnimationOver;
        PlayerAnimationEventHandler.onIASA += IASA;
        PlayerAnimationEventHandler.onAttackStep += AttackStep;
        PlayerAnimationEventHandler.onAttackStepEnd += AttackStepEnd;

        /* === INSANITY EVENTS === */
        PlayerInsanity.onSlow += Slow;
        PlayerInsanity.onIncreaseMovementSpeed += IncreaseMoveSpeed;
    }

    private void Update()
    {
        stateMachine.Update();

        playerAnimator.SetFloat("StrafeDirX", Input.x);
        playerAnimator.SetFloat("StrafeDirY", Input.y);

        Gravity();

        SnapCamera();

        _dashCooldownTimer.Time += Time.deltaTime;
    }

    private void FixedUpdate()
    {
        GroundCheck();

        Action(InputType.Idle);
    }

    [HideInInspector] public bool successfulParry;
    public override void TakeDamage(HitboxValues hitbox, Entity attacker = null)
    {
        if (isParrying)
        {
            successfulParry = true;
        }
        else
        {
            hitstunDuration = hitbox.hitstunTime;
            stateMachine.ChangeState(new HitstunState());
            health.Damage(hitbox);
        }
    }

    private void AttackStep() { attackStep = true; }
    private void AttackStepEnd() { attackStep = false; }
    private void IASA() { interruptable = true; }
    private void AnimationOver() {  attackAnimationOver = true; }
    private void Action(InputType type) { inputBuffer.Enqueue(type); }

    private void Gravity()
    {
        if (useGravity)
        {
            _velocity.y += _gravity * Time.deltaTime; //Gravity formula
        }
        else
        {
            _velocity.y = 0;
        }

        controller.Move(_velocity * Time.deltaTime); // T^2
    }

    private void GroundCheck()
    {
        _isGrounded = Physics.CheckSphere(_groundCheckPosition.position, _groundDistance, GlobalState.state.GroundMask);
        if (_isGrounded)
        {
            if (_velocity.y < 0)
                _velocity.y = -2f;
            _airDashes = 0;
            playerAnimator.SetBool("HasJumped", false);
        }
        playerAnimator.SetBool("IsGrounded", _isGrounded);
    }

    private void IncreaseMoveSpeed()
    {
        float currentInsanity = ((PlayerInsanity)health).GetInsanityPercentage();
        currentInsanity -= 75; // Slow starts at 75 insanity or higher
        if (currentInsanity > 0)
        {
            maxSpeed = _originalMaxSpeed;
            if (currentInsanity >= 10)
            {
                maxSpeed *= _moveSpeedBuffMultiplier;
            }
            else
            {
                maxSpeed *= 1 + currentInsanity / 100;
            }
        }
    }

    private void Slow()
    {
        float currentInsanity = ((PlayerInsanity)health).GetInsanityPercentage();
        if (currentInsanity - 50 > 0 && currentInsanity - 75 <= 0)
        {
            currentInsanity -= 50; // Slow starts at 50 insanity or higher
            maxSpeed = _originalMaxSpeed;
            if (currentInsanity >= 10)
            {
                maxSpeed *= _moveSpeedDebuffMultiplier;
            }
            else
            {
                maxSpeed *= 1 - currentInsanity / 100;
            }
        }
    }
    
    /// <summary>
    /// Snaps camera after returning to free look camera
    /// </summary>
    private void SnapCamera()
    {
        if (_doSnapCamera)
        {
            //var free = new Vector2(_freeLookCam.m_XAxis.Value, _freeLookCam.m_YAxis.Value);
            //var loc = new Vector2(_lockonCam.m_XAxis.Value, _lockonCam.m_YAxis.Value);
            _freeLookCam.m_XAxis.Value = transform.eulerAngles.y;
            _doSnapCamera = false;
        }
    }

    public void Dash()
    {
        if (_dashCooldownTimer.Expired && _airDashes < airDashAmount)
        {
            _airDashes++;
            _dashCooldownTimer.Reset();
            //GlobalState.state.Player.ResetAnim();
            stateMachine.ChangeState(new DashingState());
        }
    }

    public void Jump()
    {
        if (IsGrounded && !playerAnimator.GetBool("HasJumped"))
        {
            _velocity.y = Mathf.Sqrt(_jumpHeight) * -_gravity;
            playerAnimator.SetBool("HasJumped", true);
            playerAnimator.SetTrigger("Jump");
            GlobalState.state.AudioManager.PlayerJumpAudio(this.transform.position);
        }
    }

    public void EnableLockon()
    {
        _lockedOn = true;
        _doSnapCamera = true;
        _lockonCam.LookAt = transform;
        cameraAnimator.SetBool("LockedOn", _lockedOn);
        playerAnimator.SetBool("LockedOn", _lockedOn);
        _lockonCam.LookAt = pointOfInterest;
    }
    
    public void DisableLockon()
    {
        _lockedOn = false;
        _doSnapCamera = true;
        cameraAnimator.SetBool("LockedOn", _lockedOn);
        playerAnimator.SetBool("LockedOn", _lockedOn);
    }

    public void ToggleLockon()
    {

        if (!pointOfInterest)
        {
            Debug.LogWarning("Trying to toggle lockon without a point of interest!", this);
        }
        else
        {
            _lockedOn = !_lockedOn;
            cameraAnimator.SetBool("LockedOn", IsLockedOn);
            playerAnimator.SetBool("LockedOn", IsLockedOn);

            if (IsLockedOn)
            {
                _lockonCam.LookAt = pointOfInterest;

                FaceDirection(pointOfInterest);
            }
            else
            {
                _doSnapCamera = true;
            }
        }
    }
    /// <summary>
    /// Character looks towards direction
    /// </summary>
    public void FaceDirection(Transform target)
    {
        if (target != null)
        {
            Vector3 _direction = (target.position - transform.position);
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(_direction.x, 0, _direction.z));
            transform.rotation = lookRotation;
        }
    }

    public GameObject FindTarget()
    {
        GameObject _target = _targetFinder.FindTarget(); // Find target to pass to attack function in first frame of attack
        if (_target != null)
        {
            var t = GlobalState.state.Player.gameObject.GetComponent<LockonFunctionality>().Target;

            if (t != null)
            {
                if (Vector3.Distance(transform.position, t.position) <= _targetFinder.trackRadius)
                    _target = t.gameObject;
            }
        }
        return _target;
    }

    public override void KillThis()
    {
        stateMachine.ChangeState(new PlayerDeathState());
        playerAnimator.SetBool("Dead", true);
    }
}

public class IdleState : State<PlayerRevamp>
{
    public override void EnterState(PlayerRevamp owner)
    {

    }

    public override void ExitState(PlayerRevamp owner)
    {

    }

    public override void UpdateState(PlayerRevamp owner)
    {
        if (owner.Input != Vector2.zero)
        {
            owner.stateMachine.ChangeState(new MovementState());
        }

        owner.playerAnimator.SetFloat("Blend", owner.Input.magnitude);

        if (owner.IsLockedOn)
        {
            owner.stateMachine.ChangeState(new IdleAlertState());
        }
        bool inputFound = false;
        foreach (PlayerRevamp.InputType item in owner.inputBuffer)
        {
            switch (item)
            {
                case PlayerRevamp.InputType.Dash:
                    owner.Dash();
                    return;
                case PlayerRevamp.InputType.Jump:
                    owner.Jump();
                    Debug.Log("jump");
                    inputFound = true;
                    break;
                case PlayerRevamp.InputType.Parry:
                    if (owner.IsGrounded)
                    {
                        owner.stateMachine.ChangeState(new ParryState());
                        return;
                    }
                    break;
                case PlayerRevamp.InputType.AttackLight:
                    if (owner.IsGrounded)
                    {
                        owner.stateMachine.ChangeState(new LightAttackOneState());
                        return;
                    }
                    break;
                case PlayerRevamp.InputType.AttackHeavy:
                    if (owner.IsGrounded)
                    {
                        owner.stateMachine.ChangeState(new HeavyAttackState());
                        return;
                    }
                    break;
                default:
                    break;
            }
            if (inputFound)
            {
                break;
            }
        }
    }
}

public class IdleAlertState : State<PlayerRevamp>
{
    public override void EnterState(PlayerRevamp owner)
    {

    }

    public override void ExitState(PlayerRevamp owner)
    {

    }

    public override void UpdateState(PlayerRevamp owner)
    {
        if (owner.Input != Vector2.zero)
        {
            owner.stateMachine.ChangeState(new MovementState());
        }

        owner.playerAnimator.SetFloat("Blend", owner.Input.magnitude);

        if (!owner.IsLockedOn)
        {
            owner.stateMachine.ChangeState(new IdleState());
        }
        foreach (PlayerRevamp.InputType item in owner.inputBuffer)
        {
            switch (item)
            {
                case PlayerRevamp.InputType.Dash:
                    owner.Dash();
                    return;
                case PlayerRevamp.InputType.Jump:
                    owner.Jump();
                    return;
                case PlayerRevamp.InputType.Parry:
                    if (owner.IsGrounded)
                    {
                        owner.stateMachine.ChangeState(new ParryState());
                        return;
                    }
                    break;
                case PlayerRevamp.InputType.AttackLight:
                    if (owner.IsGrounded)
                    {
                        owner.stateMachine.ChangeState(new LightAttackOneState());
                        return;
                    }
                    break;
                case PlayerRevamp.InputType.AttackHeavy:
                    if (owner.IsGrounded)
                    {
                        owner.stateMachine.ChangeState(new HeavyAttackState());
                        return;
                    }
                    break;
            default:
                    break;
            }
        }
    }
}

public class MovementState : State<PlayerRevamp>
{
    private bool _isMoving;

    public override void EnterState(PlayerRevamp owner) { }
    public override void ExitState(PlayerRevamp owner)
    {
        _isMoving = false;
        //owner.playerAnimator.SetBool("Running", _isMoving);
    }

    public float movingThreshold = 0.09f;
    //private float completeTurnAroundAngleThreshold = 120;
    public override void UpdateState(PlayerRevamp owner)
    {
        if (owner.Input == Vector2.zero)
        { // Changes state to idle if player is not moving
            owner.stateMachine.ChangeState(new IdleState());
        }
        else
        {
            bool inputFound = false;
            foreach (PlayerRevamp.InputType item in owner.inputBuffer)
            {
                switch (item)
                {
                    case PlayerRevamp.InputType.Dash:
                        owner.Dash();
                        return;
                    case PlayerRevamp.InputType.Jump:
                        owner.Jump();
                        inputFound = true;
                        break;
                    case PlayerRevamp.InputType.Parry:
                        if (owner.IsGrounded)
                        {
                            owner.stateMachine.ChangeState(new ParryState());
                            return;
                        }
                        break;
                    case PlayerRevamp.InputType.AttackLight:
                        if (owner.IsGrounded)
                        {
                            owner.stateMachine.ChangeState(new LightAttackOneState());
                            return;
                        }
                        break;
                    case PlayerRevamp.InputType.AttackHeavy:
                        if (owner.IsGrounded)
                        {
                            owner.stateMachine.ChangeState(new HeavyAttackState());
                            return;
                        }
                        break;
                    default:
                        break;
                }
                if (inputFound)
                {
                    break;
                }
            }

            if (owner.IsLockedOn)
            {
                //PointOfInterestIsMousePos(owner);

                owner.transform.LookAt(owner.pointOfInterest);
                owner.transform.eulerAngles = new Vector3(0, owner.transform.eulerAngles.y, 0);

                float x = owner.Input.x;
                float z = owner.Input.y;

                Vector3 move = owner.transform.forward * z;
                move += owner.transform.right * x;

                owner.controller.Move(move * owner.maxSpeed * Time.deltaTime);
            }
            else if (owner.Input.magnitude >= movingThreshold)
            {
                Vector3 baseInputDirection = Camera.main.transform.right * owner.Input.normalized.x + Camera.main.transform.forward * owner.Input.normalized.y;

                // The angle between baseInputDirection and resultingDirection
                float angle = Vector2.Angle(new Vector2(owner.transform.forward.x, owner.transform.forward.z),
                                            new Vector2(baseInputDirection.x, baseInputDirection.z));

                float rotationSpeed = owner.rotationSpeed * (angle > 135 ? 5 : 1);
                if (!owner.IsGrounded)
                    rotationSpeed = owner.airRotationSpeed * (angle > 135 ? 2 : 1);

                Vector3 resultingDirection = Vector3.RotateTowards(owner.transform.forward, baseInputDirection, rotationSpeed * Time.deltaTime, 0.0f);

                owner.transform.rotation = Quaternion.LookRotation(resultingDirection);
                owner.transform.eulerAngles = new Vector3(0, owner.transform.eulerAngles.y, 0); // Limits rotation to the Y-axis
                Vector3 move = owner.transform.forward * owner.Input.magnitude;                 // Constant forward facing force
                owner.playerAnimator.SetFloat("Blend", owner.Input.magnitude > 1 ? 1 : owner.Input.magnitude);

                //owner.playerAnimator.SetBool("Running", true);

                if (angle < owner.rotationAngleUntilMove)
                {
                    _isMoving = true;
                }

                if (_isMoving)
                {
                    float analogSpeed = move.magnitude;
                    if (analogSpeed > 1)
                        analogSpeed = 1;

                    Vector3 speed = move.normalized * analogSpeed;

                    if (owner.IsGrounded)
                        speed *= owner.maxSpeed;
                    else
                        speed *= owner.airSpeed;

                    owner.controller.Move(speed * Time.deltaTime);
                }
            }
        }
    }
}

public class DashingState : State<PlayerRevamp>
{
    private Timer _timer;
    private Timer _lagTimer;
    private Vector3 _dashDirection;

    public override void EnterState(PlayerRevamp owner)
    {
        owner.useGravity = false;

        owner.playerAnimator.SetFloat("DashSpeed", owner.dashSpeed / owner.dashAnimationNumerator);

        if (owner.invulerableWhenDashing)
        {
            GlobalState.state.Player.invulerable = true;
        }

        _timer = new Timer(owner.dashTime);
        _lagTimer = new Timer(owner.dashLag);

        owner.playerAnimator.SetTrigger("Dash");
        owner.playerAnimator.SetBool("IsDashing", true);
        GlobalState.state.AudioManager.PlayerDodgeAudio(owner.transform.position);

        _dashDirection += Camera.main.transform.right * owner.Input.x;
        _dashDirection += Camera.main.transform.forward * owner.Input.y;
        if (_dashDirection == Vector3.zero)
            _dashDirection = Camera.main.transform.forward;
        _dashDirection.y = 0;

        if(!owner.IsLockedOn)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(_dashDirection.x, 0, _dashDirection.z));
            owner.transform.rotation = lookRotation;
        }
    }

    public override void ExitState(PlayerRevamp owner)
    {
        owner.useGravity = true;
        owner.dashPerformed = false;

        if (owner.invulerableWhenDashing)
        {
            GlobalState.state.Player.invulerable = false;
        }

        owner.playerAnimator.SetBool("IsDashing", false);
    }

    public override void UpdateState(PlayerRevamp owner)
    {
        if (_timer.Expired)
        {
            //owner.playerAnimator.SetTrigger("DashLag");
            _lagTimer.Time += Time.deltaTime;
            owner.useGravity = true;

            if (_lagTimer.Expired) // input buffer here?
            {
                owner.stateMachine.ChangeState(new IdleState());
            }
        }
        else
        {
            _timer.Time += Time.deltaTime;
            owner.controller.Move(_dashDirection.normalized * owner.dashSpeed * Time.deltaTime);

            if (owner.IsLockedOn)
            {
                owner.FaceDirection(owner.pointOfInterest);
            }
        }
    }
}

public class LightAttackOneState : State<PlayerRevamp>
{
    private bool _secondAttack;
    private GameObject _target;

    public override void EnterState(PlayerRevamp owner)
    {
        owner.interruptable = false;
        owner.attackAnimationOver = false;
        owner.light1HitboxGroup.enabled = true;
        owner.playerAnimator.SetBool("LightAttackTwo", false); // Reset the double combo animation bool upon entering state
        owner.playerAnimator.SetTrigger("AttackLight"); // Set animation trigger for first attack
        GlobalState.state.AudioManager.PlayerSwordSwingAudio(owner.transform.position);

        _target = owner.FindTarget();
    }

    public override void ExitState(PlayerRevamp owner)
    {
        owner.playerAnimator.SetBool("LightAttackTwo", _secondAttack); // Set animation bool
        owner.light1HitboxGroup.enabled = false;
        _secondAttack = false; // Reset member variable
        owner.interruptable = false;
        owner.attackAnimationOver = false;
    }

    public override void UpdateState(PlayerRevamp owner)
    {
        if (!owner.IsGrounded)
        {
            owner.stateMachine.ChangeState(new IdleState());
        }
        else
        {
            if (owner.attackStep)
            {
                if (_target != null)
                    owner.FaceDirection(_target.transform);
                owner.controller.Move(owner.transform.forward * owner.light1StepSpeed * Time.deltaTime);
            }
            if (owner.interruptable && owner.inputBuffer.Contains(PlayerRevamp.InputType.AttackLight))
            {
                _secondAttack = true;
                owner.stateMachine.ChangeState(new LightAttackTwoState());
            }
            else if (owner.interruptable && owner.Input != Vector2.zero)
            {
                owner.stateMachine.ChangeState(new IdleState());
            }
            else if (owner.attackAnimationOver)
            {
                owner.stateMachine.ChangeState(new IdleState());
            }
        }
    }
}

public class LightAttackTwoState : State<PlayerRevamp>
{
    private bool _secondAttack;
    private GameObject _target;

    public override void EnterState(PlayerRevamp owner)
    {
        owner.interruptable = false;
        owner.attackAnimationOver = false;
        owner.light2HitboxGroup.enabled = true;
        GlobalState.state.AudioManager.PlayerSwordSwingAudio(owner.transform.position);

        _target = owner.FindTarget();
    }

    public override void ExitState(PlayerRevamp owner)
    {
        owner.light2HitboxGroup.enabled = false;
        owner.interruptable = false;
        owner.attackAnimationOver = false;
        _secondAttack = false;
    }

    public override void UpdateState(PlayerRevamp owner)
    {
        if (!owner.IsGrounded)
        {
            owner.stateMachine.ChangeState(new IdleState());
        }
        else
        {
            if (owner.attackStep)
            {
                if (_target != null)
                    owner.FaceDirection(_target.transform);
                owner.controller.Move(owner.transform.forward * owner.light2StepSpeed * Time.deltaTime);
            }

            if (owner.interruptable && owner.inputBuffer.Contains(PlayerRevamp.InputType.AttackLight))
            {
                _secondAttack = true;
                owner.stateMachine.ChangeState(new LightAttackOneState());
            }
            else if (owner.interruptable && owner.Input != Vector2.zero)
            {
                owner.stateMachine.ChangeState(new IdleState());
            }
            else if (owner.attackAnimationOver)
            {
                owner.stateMachine.ChangeState(new IdleState());
            }
        }
    }
}

public class HeavyAttackState : State<PlayerRevamp>
{
    private GameObject _target;

    public override void EnterState(PlayerRevamp owner)
    {
        owner.interruptable = false;
        owner.attackAnimationOver = false;
        owner.heavyHitboxGroup.enabled = true;
        owner.playerAnimator.SetTrigger("AttackHeavy"); // Set animation trigger for first attack
        GlobalState.state.AudioManager.PlayerSwordSwingAudio(owner.transform.position);

        _target = owner.FindTarget();
    }

    public override void ExitState(PlayerRevamp owner)
    {
        owner.heavyHitboxGroup.enabled = false;
        owner.interruptable = false;
        owner.attackAnimationOver = false;
    }

    public override void UpdateState(PlayerRevamp owner)
    {
        if (!owner.IsGrounded)
        {
            owner.stateMachine.ChangeState(new IdleState());
        }
        else
        {
            if (owner.attackStep)
            {
                if (_target != null)
                    owner.FaceDirection(_target.transform);
                owner.controller.Move(owner.transform.forward * owner.heavyStepSpeed * Time.deltaTime);
            }

            if (owner.interruptable && owner.Input != Vector2.zero)
            {
                owner.stateMachine.ChangeState(new IdleState());
            }
            else if (owner.attackAnimationOver)
            {
                owner.stateMachine.ChangeState(new IdleState());
            }
        }
    }
}

public class HitstunState : State<PlayerRevamp>
{
    private Timer _hitstunTimer;
    public override void EnterState(PlayerRevamp owner)
    {
        _hitstunTimer = new Timer(owner.hitstunDuration);
        if (owner.hitstunDuration > owner.heavyHitstunThreshold) // heavy hitstun threshold
        {
            owner.playerAnimator.SetTrigger("HitstunHeavy");
        }
        else
        {
            owner.playerAnimator.SetTrigger("HitstunLight");
        }
        owner.playerAnimator.SetBool("InHitstun", true);
    }

    public override void ExitState(PlayerRevamp owner)
    {
        owner.playerAnimator.SetBool("InHitstun", false);
    }

    public override void UpdateState(PlayerRevamp owner)
    {
        _hitstunTimer += Time.deltaTime;
        if (_hitstunTimer.Expired)
        {
            owner.stateMachine.ChangeState(new IdleState());
        }
    }
}

public class ParryState : State<PlayerRevamp>
{
    private Timer _parryTimer;
    private Timer _parryLagTimer;

    public override void EnterState(PlayerRevamp owner)
    {
        _parryTimer = new Timer(owner.parryDuration);
        _parryLagTimer = new Timer(owner.parryLag);
        owner.playerAnimator.SetBool("IsParrying", true);
        owner.playerAnimator.SetTrigger("Parry");
    }

    public override void ExitState(PlayerRevamp owner)
    {
        owner.successfulParry = false;
        owner.attackAnimationOver = false;
        owner.isParrying = false;
        owner.playerAnimator.SetBool("IsParrying", false);
        owner.playerAnimator.SetBool("ParryLag", false);
        _parryTimer.Reset();
        _parryLagTimer.Reset();
    }

    public override void UpdateState(PlayerRevamp owner)
    {
        if (!owner.IsGrounded)
        {
            owner.stateMachine.ChangeState(new IdleState());
        }
        else
        {
            if (owner.attackAnimationOver)
            {
                owner.isParrying = true;
                _parryTimer.Time += Time.deltaTime;
                if (owner.successfulParry)
                {
                    owner.playerAnimator.SetTrigger("ParrySuccessful");
                    owner.stateMachine.ChangeState(new IdleState()); // successful parry state, no logic atm
                }
                else if (_parryTimer.Expired) // Mathf.Lerp time on successful parry
                {
                    owner.isParrying = false;
                    Debug.Log("expired");
                    owner.playerAnimator.SetBool("IsParrying", false);
                    owner.playerAnimator.SetBool("ParryLag", true);
                    _parryLagTimer.Time += Time.deltaTime;
                    if (_parryLagTimer.Expired)
                    {
                        owner.stateMachine.ChangeState(new IdleState());
                    }
                }
            }
        }
    }
}

public class PlayerDeathState : State<PlayerRevamp>
{
    public override void EnterState(PlayerRevamp owner)
    {
        owner.invulerable = true;
    }

    public override void ExitState(PlayerRevamp owner)
    {
        owner.invulerable = false;
        owner.playerAnimator.SetBool("Dead", false);
    }

    public override void UpdateState(PlayerRevamp owner) { }
}