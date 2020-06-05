using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingObjectScript : MonoBehaviour
{
    [SerializeField] private float _speed = 2f;
    [SerializeField] private float _amount = 2f;
    [SerializeField] private Vector3 _direction = new Vector3(0, 1, 0);
    private float _time;
    private float _startPos;

    void Start()
    {
        _startPos = Random.Range(0f, 360f);
        _time += _startPos;
    }

    void FixedUpdate()
    {
        _time += Time.fixedDeltaTime % 360;

        transform.Translate(_direction.normalized * Mathf.Sin(_time * _speed) * (_amount * _speed * 0.001f));
    }
}
