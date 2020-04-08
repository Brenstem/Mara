using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour {
    #region Parameters
    /* === DEBUG === */
    [Header("Debug")]
    [SerializeField] private bool lockCursor = true;

    /* === STATS === */
    [Header("Character properties")]
    [SerializeField] private float _gravity = -9.82f;
    [SerializeField] private float _jumpHeight = 3f;

    public float maxSpeed = 12f;
    public float acceleration = 8f;
    public float deceleration = 2f;
    public float rotationSpeed;
    public float rotationAngleUntilMove = 30;

    public float dashTime = 0.25f;
    public float dashLag = 0.15f;
    public float dashSpeed = 10.0f;

    /* === HIDDEN REFERENCES === */
    [HideInInspector] public Camera mainCamera;
    [HideInInspector] public CharacterController controller;
    private Animator animator;

    /* === PUBLIC REFERENCES === */
    [Header("References")]
    [SerializeField] private Transform _groundCheckPosition;
    [SerializeField] private Cinemachine.CinemachineFreeLook _freeLookCam;
    [SerializeField] private Cinemachine.CinemachineFreeLook _lockonCam;
    public LayerMask groundMask;
    public LayerMask enemyMask;

    /* === INFORMATION === */
    [Header("Information")]
    [SerializeField] private float _groundDistance = 0.4f;
    #endregion

    #region Variables
    /* === SCRIPT EXCLUSIVES === */
    [Header("Placeholder lock on")]
    [SerializeField] private Vector3 _lockOnOffset;
    [SerializeField] private float _lockOnRadius;
    [SerializeField] private float _lockOnMaxDistance;
    private Vector3 _lockOnOrigin;
    private Vector3 _lockOnDirection;
    private float _lockOnCurrentHitDistance;
    private RaycastHit _lockOnCastHit;


    private bool _doSnapCamera;
    private bool _hasJumped;
    private bool _isGrounded;
    private Vector3 _velocity;
    private PlayerInput _playerInput;
    [HideInInspector] public Vector2 input;
    [HideInInspector] public Transform pointOfInterest;
    [HideInInspector] public StateMachine<PlayerController> stateMachine;

    private Vector3 _maxSpeedVec;
    public Vector3 maxSpeedVec {
        get {
            if (_maxSpeedVec == null || _maxSpeedVec == Vector3.zero)
                _maxSpeedVec = Vector3.Normalize(new Vector3(1, 0, 1)) * maxSpeed;
            return _maxSpeedVec;
        }
    }

    private bool _lockon;
    public bool lockon { // better solution is adviced, though it is functional
        get { return _lockon; }
        set {
            if (pointOfInterest != null) {
                if (value) {
                    animator.SetBool("lockedOn", true);
                    if (pointOfInterest != null)
                        _lockonCam.LookAt = pointOfInterest;

                    // BUG: overrides current state, resulting in deleted end lag
                    stateMachine.ChangeState(new StrafeMovementState());
                }
                else {
                    animator.SetBool("lockedOn", false);
                    _doSnapCamera = true;
                }
                _lockon = value;
            }
        }
    }
#endregion

    private void Awake() {
        // Debug
        if (lockCursor)
            Cursor.lockState = CursorLockMode.Locked;

        // Reference handling
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        // Declarations
        _playerInput = new PlayerInput();
        stateMachine = new StateMachine<PlayerController>(this);
        stateMachine.ChangeState(new IdleMovementState());
        lockon = false;

        // Input
        _playerInput.PlayerControls.Move.performed += ctx => input = ctx.ReadValue<Vector2>();
        _playerInput.PlayerControls.Test.performed += _ => lockon = !lockon;
        _playerInput.PlayerControls.Jump.performed += ctx => _hasJumped = true;
        _playerInput.PlayerControls.Dash.performed += Dash;
    }
    
    void Update() {
        GroundCheck();

        PlaceholderLockon();

        stateMachine.Update();

        Jump();

        SnapCamera();

        _hasJumped = false;
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Debug.DrawLine(_lockOnOrigin, _lockOnOrigin + _lockOnDirection * _lockOnCurrentHitDistance);
        Gizmos.DrawWireSphere(_lockOnOrigin + _lockOnDirection * _lockOnCurrentHitDistance, _lockOnRadius);
    }

    private void OnEnable() { _playerInput.Enable(); }

    private void OnDisable() { _playerInput.Disable(); }


    /// <summary>
    /// Snaps camera after returning to free look camera
    /// </summary>
    void SnapCamera() {
        if (_doSnapCamera) {
            var free = new Vector2(_freeLookCam.m_XAxis.Value, _freeLookCam.m_YAxis.Value);
            var loc = new Vector2(_lockonCam.m_XAxis.Value, _lockonCam.m_YAxis.Value);
            _freeLookCam.m_XAxis.Value = transform.eulerAngles.y;
            _doSnapCamera = false;
        }
    }

    void GroundCheck() {
        _isGrounded = Physics.CheckSphere(_groundCheckPosition.position, _groundDistance, groundMask);
        if (_isGrounded && _velocity.y < 0) {
            _velocity.y = -2f;
        }
    }

    void Jump() {
        if (_hasJumped && _isGrounded) {
            _velocity.y = Mathf.Sqrt(_jumpHeight) * -_gravity;
        }

        _velocity.y += _gravity * Time.deltaTime; //Gravity formula
        controller.Move(_velocity * Time.deltaTime); // T^2
    }

    /* === PLACEHOLDERS === */
    private void Dash(InputAction.CallbackContext c) { // Placeholder
        stateMachine.ChangeState(new DashMovementState());
    }

    void PlaceholderLockon() {
        if (!lockon) {
            _lockOnOrigin = transform.position + _lockOnOffset;
            _lockOnDirection = Camera.main.transform.forward;
            if (_lockOnDirection.y < 0)
                _lockOnDirection.y = 0;

            if (Physics.SphereCast(_lockOnOrigin, _lockOnRadius, _lockOnDirection, out _lockOnCastHit, _lockOnMaxDistance, enemyMask)) {
                pointOfInterest = _lockOnCastHit.transform;
                _lockOnCurrentHitDistance = _lockOnCastHit.distance;
            }
            else {
                _lockOnCurrentHitDistance = _lockOnMaxDistance;
                pointOfInterest = null;
            }
        }
    }
}

public class IdleMovementState : State<PlayerController> {
    public override void EnterState(PlayerController owner) { }
    public override void ExitState(PlayerController owner) { }
    
    public override void UpdateState(PlayerController owner) {
        if (owner.input != Vector2.zero)
            owner.stateMachine.ChangeState(new GeneralMovementState());
    }
}

public class GeneralMovementState : State<PlayerController> {
    private bool _isMoving;

    public override void EnterState(PlayerController owner) { }
    public override void ExitState(PlayerController owner) {
        _isMoving = false;
    }

    //private float completeTurnAroundAngleThreshold = 120;
    public override void UpdateState(PlayerController owner) {
        if (owner.input == Vector2.zero) { // Changes state to idle if player is not moving
            owner.stateMachine.ChangeState(new IdleMovementState());
        }
        else {
            Vector3 baseInputDirection = Camera.main.transform.right * owner.input.normalized.x + Camera.main.transform.forward * owner.input.normalized.y;
            Vector3 resultingDirection = Vector3.RotateTowards(owner.transform.forward, baseInputDirection, owner.rotationSpeed * Time.deltaTime, 0.0f);

            // The angle between baseInputDirection and resultingDirection
            float angle = Vector2.Angle(new Vector2(owner.transform.forward.x, owner.transform.forward.z),
                                        new Vector2(baseInputDirection.x, baseInputDirection.z));

            owner.transform.rotation = Quaternion.LookRotation(resultingDirection);
            owner.transform.eulerAngles = new Vector3(0, owner.transform.eulerAngles.y, 0); // Limits rotation to the Y-axis
            Vector3 move = owner.transform.forward * owner.input.magnitude;                 // Constant forward facing force

            if (angle < owner.rotationAngleUntilMove) {
                _isMoving = true;
            }

            if (_isMoving) {
                owner.controller.Move(move * owner.maxSpeed * Time.deltaTime);
            }
        }
    }
}

public class StrafeMovementState : State<PlayerController> {
    public override void EnterState(PlayerController owner) { }
    public override void ExitState(PlayerController owner) { }

    public override void UpdateState(PlayerController owner) {
        if (owner.pointOfInterest != null) {
            //PointOfInterestIsMousePos(owner);

            owner.transform.LookAt(owner.pointOfInterest);
            owner.transform.eulerAngles = new Vector3(0, owner.transform.eulerAngles.y, 0);

            float x = owner.input.x;
            float z = owner.input.y;

            Vector3 move = owner.transform.forward * z;
            move += owner.transform.right * x;

            owner.controller.Move(move * owner.maxSpeed * Time.deltaTime);
        }
        else {
            owner.stateMachine.ChangeState(new GeneralMovementState());
        }
    }

    // Placeholder meant for debug
    void PointOfInterestIsMousePos(PlayerController owner) {
        Vector2 mousePos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f, owner.groundMask)) {
            //owner.pointOfInterest = hit.transform;
            owner.transform.LookAt(hit.point);
            owner.transform.eulerAngles = new Vector3(0, owner.transform.eulerAngles.y, 0);
        }
    }
}

public class DashMovementState : State<PlayerController> {
    private Timer _timer;
    private Timer _lagTimer;
    private Vector3 _dashDirection;

    public override void ExitState(PlayerController owner) { }

    public override void EnterState(PlayerController owner) {
        _timer = new Timer(owner.dashTime);
        _lagTimer = new Timer(owner.dashLag);

        _dashDirection += Camera.main.transform.right * owner.input.x;
        _dashDirection += Camera.main.transform.forward * owner.input.y;
        if (_dashDirection == Vector3.zero)
            _dashDirection = Camera.main.transform.forward;
        _dashDirection.y = 0;

        if (!owner.lockon) {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(_dashDirection.x, 0, _dashDirection.z));
            owner.transform.rotation = lookRotation;
        }
    }
    public override void UpdateState(PlayerController owner) {
        if (_timer.Expired()) {
            _lagTimer.Time += Time.deltaTime;
            if (_lagTimer.Expired()) {
                if (owner.lockon)
                    owner.stateMachine.ChangeState(new StrafeMovementState());
                else
                    owner.stateMachine.ChangeState(new IdleMovementState());
            }
        }
        else {
            _timer.Time += Time.deltaTime;
            owner.controller.Move(_dashDirection * owner.dashSpeed * Time.deltaTime);
        }
    }
}