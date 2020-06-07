using FMOD;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeScript : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private float _heightOffset = 1.75f;

    void Update()
    {
        Vector3 targetDirection = (GlobalState.state.Camera.transform.position + Vector3.up * _heightOffset) - this.transform.position;

        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, _moveSpeed * Time.deltaTime, 0);

        Quaternion rotation = Quaternion.LookRotation(newDirection);

        //if (Quaternion.Angle(transform.rotation, ))
        //restrict rotation to 85 degrees

        transform.rotation = rotation;

    }
}
