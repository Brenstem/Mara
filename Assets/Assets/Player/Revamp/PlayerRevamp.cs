using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRevamp : Entity
{
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

    #region Stats
    [Header("Movement"), Space(20)]
    [SerializeField] private float _gravity = -9.82f;
    [SerializeField] private float _jumpHeight = 3f;

    public float maxSpeed = 5f;
    public float airSpeed = 5f;
    public float rotationSpeed = 15f;
    public float airRotationSpeed = 4f;
    public float rotationAngleUntilMove = 30;
    #endregion

    
    /* === COMPONENT REFERENCES === */
    private PlayerInput _playerInput;
    private Vector2 _input;
    public Vector2 Input { get { return _input; } }

    [HideInInspector] public Transform pointOfInterest;
    [HideInInspector] public StateMachine<PlayerRevamp> stateMachine;
    [HideInInspector] public CharacterController controller;

    /* === LOCK ON === */
    private bool _doSnapCamera;
    private bool _lockedOn;
    public bool IsLockedOn { get { return _lockedOn; } }



    public override void TakeDamage(HitboxValues hitbox, Entity attacker = null)
    {

    }

    private void OnEnable() { _playerInput.PlayerControls.Enable(); }
    private void OnDisable() { _playerInput.PlayerControls.Disable(); }

    private void Awake()
    {
        base.Awake();
        controller = GetComponent<CharacterController>();
        _playerInput = new PlayerInput();
        stateMachine = new StateMachine<PlayerRevamp>(this);
        stateMachine.ChangeState(new IdleState());

        /* === INPUT === */
        _playerInput.PlayerControls.Move.performed += performedInput => _input = performedInput.ReadValue<Vector2>();
    }

    private void Update()
    {
        GroundCheck();

        stateMachine.Update();

        print(stateMachine.currentState);

        SnapCamera();
    }

    private void GroundCheck()
    {
        _isGrounded = Physics.CheckSphere(_groundCheckPosition.position, _groundDistance, GlobalState.state.GroundMask);
        if (_isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f;
            //_hasDashed = false;
        }

        //playerAnimator.SetBool("Grounded", _isGrounded);
    }

    /// <summary>
    /// Snaps camera after returning to free look camera
    /// </summary>
    void SnapCamera()
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
            cameraAnimator.SetBool("lockedOn", _lockedOn);
            if (IsLockedOn)
            {
                _lockonCam.LookAt = pointOfInterest;
            }
            else
            {
                _doSnapCamera = true;
            }
        }
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
