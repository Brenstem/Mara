using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Control : MonoBehaviour
{
    [SerializeField] private Transform pointOfInterest;
    [SerializeField] public CharacterController controller;

    [SerializeField] public float speed = 12f;
    [SerializeField] private float gravity = -9.82f;
    [SerializeField] private float jumpHeight = 3f;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] public LayerMask groundMask;

    private Vector3 velocity;
    private bool isGrounded;

    PlayerInput playerInput;
    public Vector2 input;
    bool _lockon;
    bool lockon
    {
        get { return _lockon; }
        set
        {
            if (value)
                stateMachine.ChangeState(new StrafeMovementState());
            else
                stateMachine.ChangeState(new GeneralMovementState());
            _lockon = value;
        }
    }
    bool jumped;
    private void Awake() {
        playerInput = new PlayerInput();
        stateMachine = new StateMachine<Control>(this);
        lockon = false;
        Cursor.lockState = CursorLockMode.Locked;
        playerInput.PlayerControls.Move.performed += ctx => input = ctx.ReadValue<Vector2>();
        playerInput.PlayerControls.Test.performed += _ => lockon = !lockon;
        playerInput.PlayerControls.Jump.performed += ctx => jumped = true;
    }

    public StateMachine<Control> stateMachine;

    float zRotation;
    [SerializeField] public float rotationSpeed;
    [SerializeField, Range(-1.0f, 1.0f)] private float t;
    public float turnSpeed;
    // Update is called once per frame
    void Update()
    {

        /* === GROUND CHECK === */
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0) {
            velocity.y = -2f;
        }
        /* === \GROUND CHECK === */

        stateMachine.Update();

        if (jumped && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight) * -gravity;
        }

        velocity.y += gravity * Time.deltaTime; //Gravity formula
        controller.Move(velocity * Time.deltaTime); // T^2

        jumped = false;
    }

    private void OnEnable() {
        playerInput.Enable();
    }

    private void OnDisable() {
        playerInput.Disable();
    }
}

public class IdleMovementState : State<Control>
{
    public override void EnterState(Control owner)
    {

    }

    public override void ExitState(Control owner)
    {

    }

    float zRotation;
    public override void UpdateState(Control owner) {
        if (owner.input != Vector2.zero)
            owner.stateMachine.ChangeState(new GeneralMovementState());
    }
}

public class GeneralMovementState : State<Control>
{
    public override void EnterState(Control owner)
    {

    }

    public override void ExitState(Control owner)
    {

    }

    float zRotation;
    public override void UpdateState(Control owner)
    {
        float x = owner.input.x;
        float z = owner.input.y;
        if (owner.input == Vector2.zero)
            owner.stateMachine.ChangeState(new IdleMovementState());
        if (true) // start moving from standing still
        {
            Vector3 cameraForward = Camera.main.transform.forward;
            Vector3 movementDirection = new Vector3();
            movementDirection += Camera.main.transform.right * x;
            movementDirection += Camera.main.transform.forward * z;

            Vector3 newDirection = Vector3.RotateTowards(owner.transform.forward, movementDirection, owner.turnSpeed * Time.deltaTime, 0.0f);

            //Vector3 newDirection = Vector3.RotateTowards(owner.transform.forward, cameraForward, owner.turnSpeed * Time.deltaTime, 0.0f);

            Debug.DrawRay(owner.transform.position, newDirection, Color.red);
            
            /*
            float angle = Vector2.Angle(new Vector2(owner.transform.forward.x, owner.transform.forward.z), new Vector2(cameraDirection.x, cameraDirection.z));
            if (angle > 65)
            {
                owner.transform.Rotate(new Vector3(0, angle, 0));
            }
            else
            {
            }
            */
            owner.transform.rotation = Quaternion.LookRotation(newDirection);
            owner.transform.eulerAngles = new Vector3(0, owner.transform.eulerAngles.y, 0);
        }

        Vector3 move = owner.transform.forward;
        //zRotation += owner.rotationSpeed * x * Time.deltaTime; // smoothing för polish

        //owner.transform.eulerAngles = new Vector3(0, zRotation, 0);
        owner.controller.Move(move * owner.speed * Time.deltaTime);
    }
}

public class StrafeMovementState : State<Control>
{
    public override void EnterState(Control owner)
    {

    }

    public override void ExitState(Control owner)
    {

    }

    public override void UpdateState(Control owner)
    {
        /* ==== POINT OF INTEREST IS MOUSE POS ==== */
        Vector2 mousePos = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f, owner.groundMask))
        {
            owner.transform.LookAt(hit.point);
            owner.transform.eulerAngles = new Vector3(0, owner.transform.eulerAngles.y, 0);
        }
        /* ==== \POINT OF INTEREST IS MOUSE POS ==== */

        float x = owner.input.x;
        float z = owner.input.y;

        Vector3 move = owner.transform.forward * z;
        move += owner.transform.right * x;

        owner.controller.Move(move * owner.speed * Time.deltaTime);
    }
}