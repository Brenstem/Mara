using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LockonFunctionality : MonoBehaviour {
    [Header("Information")]
    [SerializeField] LayerMask enemyMask;
    [TagSelector] public string lockonTag;

    [Header("Stats")]
    [SerializeField] private float _trackRadius;
    [SerializeField] private float _lockedOnRadius;
    [SerializeField, Range(0.0f, 90.0f)] float _angle;

    [Header("Debug")]
    [SerializeField] List<GameObject> targetList;

    /* === SCRIPT EXCLUSIVES === */
    private Collider[] _cols;
    private float _closestDot;
    private Collider _closestTarget;
    private Transform _pointOfInterest;
    private PlayerInput _playerInput;
    private MovementController _movementController;


    private void Awake()
    {
        _playerInput = new PlayerInput();
        targetList = new List<GameObject>();
        _movementController = GetComponent<MovementController>();
        _playerInput.PlayerControls.Test.performed += LockOnInput;
    }

    private void LockOnInput(InputAction.CallbackContext c)
    {
        if (_pointOfInterest != null)
        {
            _movementController.pointOfInterest = _pointOfInterest;
            _movementController.ToggleLockon();
        }
        else if (_movementController.isLockedOn)
        {
            _movementController.DisableLockon();
        }
    }

    private void OnEnable() { _playerInput.Enable(); }

    private void OnDisable() { _playerInput.Disable(); }

    void TargetRecognition()
    {
        _cols = Physics.OverlapSphere(transform.position, _trackRadius, enemyMask);
        _closestTarget = null;

        foreach (Collider collider in _cols)
        {
            if (collider.tag == lockonTag)
            {
                /* === DOT PRODUCT INFO === */
                Vector3 characterToCollider = (collider.transform.position - transform.position).normalized;
                characterToCollider.y = 0;
                Vector3 forward = Camera.main.transform.forward;
                forward.y = 0;

                float dot = Vector3.Dot(characterToCollider, forward);

                if (dot >= Mathf.Abs(Mathf.Cos(_angle * Mathf.Deg2Rad)))
                {
                    if (!_closestTarget)
                    { // null check
                        _closestTarget = collider;
                        _closestDot = dot;
                    }
                    else
                    {
                        float closestDist = Vector3.Distance(_closestTarget.transform.position, transform.position);
                        float colDist = Vector3.Distance(collider.transform.position, transform.position);
                        if (dot > _closestDot && closestDist > colDist)
                        {
                            _closestTarget = collider;
                            _closestDot = dot;
                        }
                    }
                }
            }
        }
        if (_closestTarget)
        {
            _pointOfInterest = _closestTarget.transform;
        }
        else
        {
            if (_movementController.isLockedOn)
            {
                if (Object.ReferenceEquals(_pointOfInterest, null) || Vector3.Distance(transform.position, _pointOfInterest.position) > _lockedOnRadius)
                {
                    _pointOfInterest = null;
                    _movementController.DisableLockon();
                }
            }
            else
            {
                _pointOfInterest = null;
            }
        }
    }

    private void Update()
    {
        TargetRecognition();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _trackRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _lockedOnRadius);

        if (_closestTarget != null)
        {
            Gizmos.DrawIcon(_closestTarget.transform.position, "lockon");
        }
    }
}