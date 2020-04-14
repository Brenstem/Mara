using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LockonFunctionality : MonoBehaviour {
    [SerializeField] LayerMask enemyMask;
    [TagSelector] public string lockonTag;

    [SerializeField] float _trackRadius;
    [SerializeField, Range(0.0f, 90.0f)] float _angle;

    [Header("Debug")]
    [SerializeField] List<GameObject> targetList;

    PlayerInput _playerInput;
    public Image image;


    private void Awake() {
        _playerInput = new PlayerInput();
        targetList = new List<GameObject>();
        _playerInput.PlayerControls.Test.performed += e2;
        //characterToCollider = transform.position;
    }

    private void e2(InputAction.CallbackContext c) {
        if (pointOfInterest != null) {
            GetComponent<PlayerController>().pointOfInterest = pointOfInterest;
            GetComponent<PlayerController>().ToggleLockon();
        }
    }

    private void OnEnable() { _playerInput.Enable(); }

    private void OnDisable() { _playerInput.Disable(); }

    [SerializeField] Collider[] cols;

    Transform pointOfInterest;
    Collider closestTarget;
    float closestDot;
    void Raycasting1() {
        cols = Physics.OverlapSphere(transform.position, _trackRadius, enemyMask);
        if (cols.Length == 0)
            image.color = new Color(1, 1, 1, 0);
        closestTarget = null;
        Vector3 characterToCollider;
        float dot;
        foreach (Collider collider in cols) {
            if (collider.tag == lockonTag) {
                characterToCollider = (collider.transform.position - transform.position).normalized;
                characterToCollider.y = 0;
                Vector3 forward = Camera.main.transform.forward;
                forward.y = 0;
                dot = Vector3.Dot(characterToCollider, forward);
                print(dot);
                print(Mathf.Cos(_angle * Mathf.Deg2Rad));
                if (dot >= Mathf.Abs(Mathf.Cos(_angle * Mathf.Deg2Rad))) {
                    if (!closestTarget) {
                        closestTarget = collider;
                        closestDot = dot;
                    }
                    else {
                        if (dot > closestDot) {
                            if (Vector3.Distance(closestTarget.transform.position, transform.position) 
                                > Vector3.Distance(collider.transform.position, transform.position)) {
                                closestTarget = collider;
                                closestDot = dot;
                            }
                        }
                    }
                }
            }
        }
        if (closestTarget) {
            pointOfInterest = closestTarget.transform;
        }
            /*
            if (closestTarget) {
                image.color = new Color(1, 1, 1, 1);
                image.transform.position = Camera.main.WorldToScreenPoint(closestTarget.transform.position);
                pointOfInterest = closestTarget.transform;
            }
            else {
                image.color = new Color(1, 1, 1, 0);
            }
            */
        }
    private void Update() {
        Raycasting1();
    }
    
    private void OnDrawGizmos() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _trackRadius);

        Gizmos.color = Color.blue;
        if (closestTarget != null) {
            Gizmos.DrawIcon(closestTarget.transform.position, "lockon");
            //Gizmos.DrawCube(closestTarget.transform.position, Vector3.one * 1.5f);
        }
    }
}