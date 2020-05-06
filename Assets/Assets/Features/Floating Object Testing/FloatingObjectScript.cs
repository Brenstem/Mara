using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingObjectScript : MonoBehaviour
{
    [SerializeField] private float _speed = 2f;
    [SerializeField] private float _amount = 2f;

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

        transform.Translate(new Vector3(0f, Mathf.Sin(_time * _speed) * (_amount * _speed * 0.001f), 0f));
    }
}
