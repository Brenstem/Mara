using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Control : MonoBehaviour
{
    [SerializeField] private CharacterController controller;

    [SerializeField] private float speed = 12f;
    [SerializeField] private float gravity = -9.82f;
    [SerializeField] private float jumpHeight = 3f;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    private Vector3 velocity;
    private bool isGrounded;

    PlayerInput playerInput;
    Vector2 input;
    bool jumped;

    private void Awake() {
        playerInput = new PlayerInput();
        playerInput.PlayerControls.Move.performed += ctx => input = ctx.ReadValue<Vector2>();
        //playerInput.PlayerControls.Jump.performed += ctx => jumped = ctx.ReadValue<bool>();
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0) {
            velocity.y = -2f;
        }
        float x = input.x;
        float z = input.y;
        print(input);

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);
    
        if (jumped && isGrounded) {
            velocity.y = Mathf.Sqrt(jumpHeight) * -2f * gravity;
        }

        velocity.y += gravity * Time.deltaTime; //Gravity formula
        controller.Move(velocity * Time.deltaTime); // T^2
    }

    private void OnEnable() {
        playerInput.Enable();
    }

    private void OnDisable() {
        playerInput.Disable();
    }
}