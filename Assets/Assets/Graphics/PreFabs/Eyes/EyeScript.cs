using FMOD;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeScript : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;

    void Update()
    {
        Vector3 targetDirection = (GlobalState.state.Player.transform.position + Vector3.up * 1.75f) - this.transform.position;

        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, _moveSpeed * Time.deltaTime, 0);

        transform.rotation = Quaternion.LookRotation(newDirection);
    }
}
