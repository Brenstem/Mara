using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Control : MonoBehaviour {
    //public Camera cam;
    //public Cinemachine.CinemachineFreeLook cinemachine;

    private Animator animator;

    [SerializeField] public Transform pointOfInterest;
    [SerializeField] private Cinemachine.CinemachineFreeLook freeLookCam;
    [SerializeField] private Cinemachine.CinemachineFreeLook lockonCam;

    [SerializeField] public CharacterController controller;

    [SerializeField] public float maxSpeed = 12f;
    [SerializeField] public float acceleration = 8f;
    [SerializeField] public float deceleration = 2f;
    [SerializeField] private float gravity = -9.82f;
    [SerializeField] private float jumpHeight = 3f;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] public LayerMask groundMask, enemyMask;

    private Vector3 _maxSpeedVec;
    public Vector3 maxSpeedVec {
        get {
            if (_maxSpeedVec == null || _maxSpeedVec == Vector3.zero)
                _maxSpeedVec = Vector3.Normalize(new Vector3(1, 0, 1)) * maxSpeed;
            return _maxSpeedVec;
        }
    }

    [SerializeField] private Vector3 _velocity;
    /*
    public Vector3 velocity {
        get { return _velocity; }
        set {
            _velocity = value;
            if (Mathf.Abs(value.x) > maxSpeedVec.x)
                _velocity.x = maxSpeedVec.x * Mathf.Sign(value.x);
            if (Mathf.Abs(value.z) > maxSpeedVec.z)
                _velocity.z = maxSpeedVec.z * Mathf.Sign(value.z);
        }
    }
    */
    private bool isGrounded;

    PlayerInput playerInput;
    public Vector2 input;
    bool _lockon;
    public bool lockon {
        get { return _lockon; }
        set {
            if (pointOfInterest != null) {
                if (value) {
                    animator.SetBool("lockedOn", true);
                    if (pointOfInterest != null)
                        lockonCam.LookAt = pointOfInterest;
                    stateMachine.ChangeState(new StrafeMovementState());
                }
                else {
                    animator.SetBool("lockedOn", false);
                    doSnapCamera = true;
                }
                _lockon = value;
            }
        }
    }
    bool jumped;
    private void Awake() {
        playerInput = new PlayerInput();
        Cursor.lockState = CursorLockMode.Locked;
        animator = gameObject.GetComponent<Animator>();
        stateMachine = new StateMachine<Control>(this);
        stateMachine.ChangeState(new IdleMovementState());
        lockon = false;
        playerInput.PlayerControls.Move.performed += ctx => input = ctx.ReadValue<Vector2>();
        playerInput.PlayerControls.Test.performed += _ => lockon = !lockon;
        playerInput.PlayerControls.Jump.performed += ctx => jumped = true;
        playerInput.PlayerControls.Dash.performed += _ => Dash();
    }
    
    private void Dash() {
        stateMachine.ChangeState(new DashMovementState());
    }

    public StateMachine<Control> stateMachine;

    float zRotation;
    [SerializeField] public float rotationSpeed;
    [SerializeField, Range(-1.0f, 1.0f)] private float t;
    public float turnSpeed;
    public bool doSnapCamera;
    // Update is called once per frame
    void Update() {

        /* === GROUND CHECK === */
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && _velocity.y < 0) {
            _velocity.y = -2f;
        }
        /* === \GROUND CHECK === */

        stateMachine.Update();
        //print("Current state: " + stateMachine.currentState);
        if (jumped && isGrounded) {
            _velocity.y = Mathf.Sqrt(jumpHeight) * -gravity;
        }

        _velocity.y += gravity * Time.deltaTime; //Gravity formula
        controller.Move(_velocity * Time.deltaTime); // T^2

        if (doSnapCamera) {
            var free = new Vector2(freeLookCam.m_XAxis.Value, freeLookCam.m_YAxis.Value);
            var loc = new Vector2(lockonCam.m_XAxis.Value, lockonCam.m_YAxis.Value);
            freeLookCam.m_XAxis.Value = transform.eulerAngles.y;
            doSnapCamera = false;
        }
        jumped = false;
        e();
    }

    Vector3 origin;
    Vector3 direction;

    [SerializeField] Vector3 lockonOffset;
    [SerializeField] float lockOnRadius;
    [SerializeField] float currentHitDistance;
    [SerializeField] float maxDistance;
    RaycastHit hit;
    void e() {
        if (!lockon) {
            origin = transform.position + lockonOffset;
            direction = Camera.main.transform.forward;
            if (direction.y < 0)
                direction.y = 0;

            if (Physics.SphereCast(origin, lockOnRadius, direction, out hit, maxDistance, enemyMask)) {
                pointOfInterest = hit.transform;
                currentHitDistance = hit.distance;
            }
            else {
                currentHitDistance = maxDistance;
                pointOfInterest = null;
            }
        }
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Debug.DrawLine(origin, origin + direction * currentHitDistance);
        Gizmos.DrawWireSphere(origin + direction * currentHitDistance, lockOnRadius);
    }

    private void OnEnable() {
        playerInput.Enable();
    }

    private void OnDisable() {
        playerInput.Disable();
    }
}

public class IdleMovementState : State<Control> {
    public override void EnterState(Control owner) {

    }

    public override void ExitState(Control owner) {

    }

    float zRotation;
    public override void UpdateState(Control owner) {
        if (owner.input != Vector2.zero)
            owner.stateMachine.ChangeState(new GeneralMovementState());
        else {
            /*
            float delta = Mathf.Abs(new Vector2(owner.velocity.x, owner.velocity.z).magnitude) - owner.deceleration * Time.deltaTime;
            float deltaX = Mathf.Abs(owner.velocity.x) - owner.deceleration * Time.deltaTime;
            float deltaZ = Mathf.Abs(owner.velocity.z) - owner.deceleration * Time.deltaTime;
            Debug.Log(delta);
            Debug.Log(deltaX);
            Debug.Log(deltaZ);
            /*if (delta < 0) {
                owner.velocity = new Vector3(0, owner.velocity.y, 0);
            }
            else {
                */
            /*
            Vector3 vel = Vector3.zero;
            if (deltaX > 0) {
                vel = owner.velocity -= new Vector3(1, 0, 0) * owner.deceleration;
            }

            if (deltaZ > 0) {
                vel = owner.velocity -= new Vector3(0, 0, 1) * owner.deceleration;
            }
            vel.y = owner.velocity.y;
            Debug.Log(vel);
            owner.velocity = vel;
            //}
            */

        }
    }
}

public class GeneralMovementState : State<Control> {
    public override void EnterState(Control owner) { }
    public override void ExitState(Control owner) { }

    float turnAroundAngleThreshold = 120;
    public override void UpdateState(Control owner) {
        if (owner.input == Vector2.zero) { // Changes state to idle if player is not moving
            owner.stateMachine.ChangeState(new IdleMovementState());
        }
        else {
            Vector3 baseInputDirection = Camera.main.transform.right * owner.input.x + Camera.main.transform.forward * owner.input.y;
            Vector3 resultingDirection = Vector3.RotateTowards(owner.transform.forward, baseInputDirection, owner.turnSpeed * Time.deltaTime, 0.0f);

            // The angle between baseInputDirection and resultingDirection
            float angle = Vector2.Angle(new Vector2(owner.transform.forward.x, owner.transform.forward.z),
                                        new Vector2(baseInputDirection.x, baseInputDirection.z));
            Debug.Log("GeneralMovementState angle: " + angle); // Debug info

            if (angle > turnAroundAngleThreshold) {
                // new state, turnaround state
                // basically, loss of control och lite påbörjad momentum fram tills animationen är klar, då flippas det
                owner.transform.Rotate(new Vector3(0, angle, 0)); // placeholder
            }
            else {
                owner.transform.rotation = Quaternion.LookRotation(resultingDirection);
            }

            owner.transform.eulerAngles = new Vector3(0, owner.transform.eulerAngles.y, 0); // Limits rotation to the Y-axis
            Vector3 move = owner.transform.forward;                                         // Constant forward facing force
            //owner.velocity += move * owner.acceleration;
            owner.controller.Move(move * owner.maxSpeed * Time.deltaTime);
        }
    }
}

public class StrafeMovementState : State<Control> {
    public override void EnterState(Control owner) { }
    public override void ExitState(Control owner) { }

    public override void UpdateState(Control owner) {
        if (owner.pointOfInterest != null) {
            /* ==== POINT OF INTEREST IS MOUSE POS ==== */
            /*
            Vector2 mousePos = Input.mousePosition;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100f, owner.groundMask)) {
                //owner.pointOfInterest = hit.transform;
                owner.transform.LookAt(hit.point);
                owner.transform.eulerAngles = new Vector3(0, owner.transform.eulerAngles.y, 0);
            }
            /* ==== \POINT OF INTEREST IS MOUSE POS ==== */

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
}

public class DashMovementState : State<Control> {
    private Vector3 dashDirection;

    public override void EnterState(Control owner) {
        timer = new Timer(dashTime);
        lagTimer = new Timer(dashLag);

        dashDirection += Camera.main.transform.right * owner.input.x;
        dashDirection += Camera.main.transform.forward * owner.input.y;
        if (dashDirection == Vector3.zero)
            dashDirection = Camera.main.transform.forward;
        dashDirection.y = 0;

        if (!owner.lockon) {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(dashDirection.x, 0, dashDirection.z));
            owner.transform.rotation = lookRotation;
        }
    }

    public override void ExitState(Control owner) { }

    //både det och cooldown
    float dashTime = 0.25f;
    float dashLag = 0.15f;
    float dashSpeed = 10.0f;
    Timer timer, lagTimer;
    public override void UpdateState(Control owner) {
        if (timer.Expired()) {
            lagTimer.Time += Time.deltaTime;
            if (lagTimer.Expired()) {
                if (owner.lockon)
                    owner.stateMachine.ChangeState(new StrafeMovementState());
                else
                    owner.stateMachine.ChangeState(new IdleMovementState());
            }
        }
        else {
            timer.Time += Time.deltaTime;
            owner.controller.Move(dashDirection * dashSpeed * Time.deltaTime);
        }
    }
}