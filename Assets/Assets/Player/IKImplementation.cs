using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKImplementation : MonoBehaviour
{
    [Range(0, 1f)]
    [SerializeField] private float _distanceToGround;

    [Range(0, 1f)]
    [SerializeField] private float _lookAtIKWeight;

    [Range(0, 3f)]
    [SerializeField] private float _lookAtVerticalOffset;

    [SerializeField] private LayerMask _collisionLayers;

    private Animator _anim;
    private LockonFunctionality _playerLockOn;
    private Transform _lookAtTarget = null;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _playerLockOn = GlobalState.state.Player.GetComponent<LockonFunctionality>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (_anim)
        {
            // Head
            _anim.SetLookAtWeight(_lookAtIKWeight);

            if (!GlobalState.state.Player.IsLockedOn)
            {
                _lookAtTarget = _playerLockOn.Target;

                if (_lookAtTarget)
                {
                    _anim.SetLookAtPosition(_lookAtTarget.position + new Vector3(0, _lookAtVerticalOffset, 0));
                }
                else
                {
                    _anim.SetLookAtPosition((GlobalState.state.Player.transform.position + new Vector3(0, _lookAtVerticalOffset, 0)) + GlobalState.state.Camera.transform.forward);
                }
            }
            else
            {
                _anim.SetLookAtPosition((GlobalState.state.Player.transform.position + new Vector3(0, _lookAtVerticalOffset, 0)) + GlobalState.state.Camera.transform.forward);
            }

            // Left foot
            _anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
            _anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);

            RaycastHit hit;
            Ray ray = new Ray(_anim.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up, Vector3.down);

            if (Physics.Raycast(ray, out hit, _distanceToGround + 1f, _collisionLayers))
            {
                Vector3 footPos = hit.point;
                footPos.y += _distanceToGround;

                _anim.SetIKPosition(AvatarIKGoal.LeftFoot, footPos);
                _anim.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(transform.forward, hit.normal));
            }

            // Right foot
            _anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
            _anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
            
            ray = new Ray(_anim.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up, Vector3.down);

            if (Physics.Raycast(ray, out hit, _distanceToGround + 1f, _collisionLayers))
            {
                Vector3 footPos = hit.point;
                footPos.y += _distanceToGround;

                _anim.SetIKPosition(AvatarIKGoal.RightFoot, footPos);
                _anim.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(transform.forward, hit.normal));    
            }
        }
    }
}
