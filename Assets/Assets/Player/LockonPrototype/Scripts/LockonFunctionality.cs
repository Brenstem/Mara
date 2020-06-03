using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LockonFunctionality : MonoBehaviour
{
    [Header("Information")]
    [SerializeField] private LayerMask _enemyMask;
    [SerializeField] private LayerMask _groundMask;
    [TagSelector] public string lockonTag;
    [SerializeField] private Transform _lockonThreshold;

    [Header("Stats")]
    [SerializeField] private float _trackRadius;
    [SerializeField] private float _lockedOnRadius;
    [SerializeField, Range(0.0f, 180.0f)] float _fieldOfViewAngle;

    [Header("Debug")]
    [SerializeField] List<GameObject> targetList;

    /* === SCRIPT EXCLUSIVES === */
    private Collider[] _cols;
    private float _closestDot;
    private Collider _closestTarget;
    private Transform _pointOfInterest;
    private PlayerInput _playerInput;
    private PlayerRevamp _player;

    public Transform Target
    {
        get
        {
            if (!_pointOfInterest)
            {
                return null;
            }
            else
            {
                return _pointOfInterest;
            }
        }
    }

    private void Awake()
    {
        _playerInput = new PlayerInput();
        targetList = new List<GameObject>();
        //_movementController = GetComponent<MovementController>();
        _player = GetComponent<PlayerRevamp>();

        _playerInput.PlayerControls.Lockon.performed += LockOnInput;
    }

    private void LockOnInput(InputAction.CallbackContext c)
    {
        if (_pointOfInterest != null)
        {
            _player.pointOfInterest = _pointOfInterest;
            _player.ToggleLockon();
        }
        else if (_player.IsLockedOn)
        {
            _player.DisableLockon();
        }
    }

    private void OnEnable() { _playerInput.Enable(); }

    private void OnDisable()
    {
        _playerInput.Disable();
        //_movementController.DisableLockon();
    }
    
    void TargetRecognition()
    {
        if (!_player.IsLockedOn)
        {
            _cols = Physics.OverlapSphere(transform.position, _trackRadius, _enemyMask);
            float _smallestAngle = 180;
            _closestTarget = null;
            _pointOfInterest = null;
            foreach (Collider collider in _cols)
            {
                if (collider.tag == lockonTag)
                {
                    Vector3 direction = collider.transform.position - transform.position;
                    direction.y = 0;

                    Vector3 cForward = Camera.main.transform.forward;
                    cForward.y = 0;

                    float angle = Vector3.Angle(cForward, direction);
                    if (angle < _fieldOfViewAngle / 2) // Is object within the view-cone
                    {
                        RaycastHit firstHit;
                        if (Physics.Raycast(_lockonThreshold.position, direction.normalized, out firstHit)) // is the view of the object obstructed?
                        {
                            bool hit = false;
                            RaycastHit secondHit;
                            if (firstHit.collider == collider)
                            {
                                hit = true;
                            }
                            else if (Physics.Raycast(_lockonThreshold.position, direction.normalized, out secondHit, Mathf.Infinity, _enemyMask))
                            {
                                if (!secondHit.collider.CompareTag(lockonTag))
                                {
                                    // print(secondHit.collider.name);
                                    hit = true;
                                }
                            }

                            if (hit)
                            {
                                if (_closestTarget == null) // if it is the first col
                                {
                                    _closestTarget = collider;
                                    _smallestAngle = angle;
                                }
                                else
                                {
                                    if (angle < _smallestAngle)
                                    {
                                        _closestTarget = collider;
                                    }
                                }
                            }
                        }
                    }
                    #region Old dot product
                    /* === DOT PRODUCT INFO === */
                    /*
                    Vector3 characterToCollider = (collider.transform.position - transform.position).normalized;
                    characterToCollider.y = 0;
                    Vector3 forward = Camera.main.transform.forward;
                    forward.y = 0;
                    */
                    /*
    float dot = Vector3.Dot(characterToCollider, forward);
    if (dot >= Mathf.Abs(Mathf.Cos(_fieldOfViewAngle * Mathf.Deg2Rad)))
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
    */
                    #endregion
                }
            }
            if (_closestTarget != null)
            {
                _pointOfInterest = _closestTarget.transform;
            }
        }
        else
        {
            if (_pointOfInterest != null)
            {
                Vector3 direction = _pointOfInterest.transform.position - transform.position;
                //direction.y = 0;
                RaycastHit hit;
                Physics.Raycast(_lockonThreshold.position, direction.normalized, out hit, Vector3.Distance(_lockonThreshold.position, _pointOfInterest.position), _groundMask); // is the view of the object obstructed?
                if (hit.collider != null || Vector3.Distance(_lockonThreshold.position, _pointOfInterest.position) > _lockedOnRadius)
                {
                    _pointOfInterest = null;
                    _closestTarget = null;
                    _player.DisableLockon();
                }
            }
            else
            {
                _pointOfInterest = null;
                _closestTarget = null;
                _player.DisableLockon();
            }
        }
    }

    private void Update()
    {
        if (_pointOfInterest != null && !_pointOfInterest.CompareTag(lockonTag))
        {
            _player.DisableLockon();
        }
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