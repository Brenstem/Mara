using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRevamp : Entity
{
    #region Inspector
    /* === INSPECTOR VARIABLES === */
    [Header("References")]
    public Animator playerAnimator;
    public Animator cameraAnimator;
    [SerializeField] private Transform _groundCheckPosition;
    [SerializeField] private Cinemachine.CinemachineFreeLook _freeLookCam;
    [SerializeField] private Cinemachine.CinemachineFreeLook _lockonCam;

    [Header("Ground Check")]
    [SerializeField] private float _groundDistance = 0.4f;

    private Vector3 _velocity;
    private bool _isGrounded;
    public bool IsGrounded { get { return _isGrounded; } }

    [Header("Movement"), Space(20)]
    public float maxSpeed = 5f;
    public float rotationSpeed = 15f;
    public float rotationAngleUntilMove = 30;

    [Header("Jump")]
    [SerializeField] private float _gravity = -9.82f;
    [SerializeField] private float _jumpHeight = 3f;
    public float airRotationSpeed = 4f;
    public float airSpeed = 5f;
    private bool _hasJumped;

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
    #endregion

    /* === COMPONENT REFERENCES === */
    private PlayerInput _playerInput;

    [HideInInspector] public Transform pointOfInterest;
    [HideInInspector] public StateMachine<PlayerRevamp> stateMachine;
    [HideInInspector] public CharacterController controller;

    /* === INPUT === */
    private Vector2 _input;
    public Vector2 Input { get { return _input; } }

    [HideInInspector] public bool dashPerformed;
    [HideInInspector] public bool jumpPerformed;
    [HideInInspector] public bool useGravity = true;

    /* === LOCK ON === */
    private bool _doSnapCamera;
    private bool _lockedOn;

    public bool IsLockedOn { get { return _lockedOn; } }

    private void OnEnable() { _playerInput.PlayerControls.Enable(); }
    private void OnDisable() { _playerInput.PlayerControls.Disable(); }

    private new void Awake()
    {
        base.Awake();
        controller = GetComponent<CharacterController>();
        _playerInput = new PlayerInput();
        stateMachine = new StateMachine<PlayerRevamp>(this);
        stateMachine.ChangeState(new IdleState());

        // Dash
        _dashCooldownTimer = new Timer(_dashCooldownTime);
        _dashCooldownTimer.Time += _dashCooldownTime;

        /* === INPUT === */
        _playerInput.PlayerControls.Move.performed += performedInput => _input = performedInput.ReadValue<Vector2>();
        _playerInput.PlayerControls.Dash.performed += performedInput => dashPerformed = true;
        _playerInput.PlayerControls.Jump.performed += performedInput => jumpPerformed = true;
    }

    private void Update()
    {
        GroundCheck();

        stateMachine.Update();

        print(stateMachine.currentState);

        SnapCamera();

        Gravity();

        _dashCooldownTimer.Time += Time.deltaTime;
    }

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
        if (_hasJumped && IsGrounded) 
        {
            _hasJumped = true;
            _velocity.y = Mathf.Sqrt(_jumpHeight) * -_gravity;
            GlobalState.state.Player.EndAnim();
            playerAnimator.SetTrigger("Jump");
            GlobalState.state.AudioManager.PlayerJumpAudio(this.transform.position);
        }
    }

    public override void TakeDamage(HitboxValues hitbox, Entity attacker = null)
    {

    }

    private void GroundCheck()
    {
        _isGrounded = Physics.CheckSphere(_groundCheckPosition.position, _groundDistance, GlobalState.state.GroundMask);
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
            _airDashes = 0;
            _hasJumped = false;
        }

        //playerAnimator.SetBool("Grounded", _isGrounded);
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

    public void EnableLockon()
    {
        _lockedOn = true;
        _doSnapCamera = true;
        _lockonCam.LookAt = transform;
        cameraAnimator.SetBool("lockedOn", _lockedOn);
        _lockonCam.LookAt = pointOfInterest;
    }
    
    public void DisableLockon()
    {
        _lockedOn = false;
        _doSnapCamera = true;
        cameraAnimator.SetBool("lockedOn", _lockedOn);
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
            cameraAnimator.SetBool("lockedOn", IsLockedOn);
            playerAnimator.SetBool("lockedOn", IsLockedOn);

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
        Vector3 _direction = (target.position - transform.position);
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(_direction.x, 0, _direction.z));
        GlobalState.state.PlayerGameObject.GetComponent<Transform>().rotation = lookRotation;
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

        if (owner.dashPerformed)
        {
            owner.Dash();
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

        if (owner.dashPerformed)
        {
            owner.Dash();
        }
    }
}

public class JumpState : State<PlayerRevamp>
{
    // timer
    public override void EnterState(PlayerRevamp owner)
    {

    }

    public override void ExitState(PlayerRevamp owner)
    {

    }

    public override void UpdateState(PlayerRevamp owner)
    {

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
            if (owner.dashPerformed)
            {
                owner.Dash();
            }
            else if (owner.IsLockedOn)
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

