using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset;
    [SerializeField] private Vector3 targetOffset;
    [SerializeField] private Vector3 localOffset;
    [SerializeField] private float angle;
    [SerializeField] private float radius;
    [SerializeField] private float sensitivity;
    PlayerInput playerInput;
    Vector2 input;

    private void Awake() {
        playerInput = new PlayerInput();
        playerInput.PlayerControls.Look.performed += ctx => input = ctx.ReadValue<Vector2>();
    }

    float angleX;
    float angleY;

    // Update is called once per frame
    void Update() {
        angleX += input.x * sensitivity;
        angleY += input.y * sensitivity;
        input = Vector2.zero;
        print(input);
        //transform.position = objectToFollow.transform.position - objectToFollow.transform.forward + offset;
        float cameraX = target.position.x + (radius * Mathf.Cos(Mathf.Deg2Rad * angleX));
        float cameraY = 0;//target.position.y + (radius * Mathf.Tan(Mathf.Deg2Rad * angleY));
        float cameraZ = target.position.z + (radius * Mathf.Sin(Mathf.Deg2Rad * angleX));

        transform.position = new Vector3(cameraX, cameraY, cameraZ) + offset;
        transform.localPosition += localOffset;
        var t = target;
        t.position += targetOffset;
        transform.LookAt(t);
    }

    private void OnEnable() {
        playerInput.Enable();
    }

    private void OnDisable() {
        playerInput.Disable();
    }
}
