using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterializeWalls : MonoBehaviour
{
    [SerializeField] private Material _mat1;
    [SerializeField] private Material _mat2;
    [SerializeField] private Material _mat3;
    [SerializeField] private GameObject _colliders;

    [Header("Shader")]
    [SerializeField] float shaderFadeMultiplier = 1f;

    private Timer _shaderTimer;
    private float _shaderFadeTime = 1;

    // Start is called before the first frame update
    void Start()
    {
        _mat1.SetFloat("Vector1_5443722F", 1);
        _mat2.SetFloat("Vector1_5443722F", 1);
        _mat3.SetFloat("Vector1_5443722F", 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (_shaderTimer != null)
        {
            _shaderFadeTime -= Time.deltaTime * shaderFadeMultiplier;
            Mathf.Clamp(_shaderFadeTime, -1, 1);
            _mat1.SetFloat("Vector1_5443722F", _shaderFadeTime);
            _mat2.SetFloat("Vector1_5443722F", _shaderFadeTime);
            _mat3.SetFloat("Vector1_5443722F", _shaderFadeTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            print("meme");
            _shaderTimer = new Timer(_shaderFadeTime);
            _colliders.SetActive(true);
        }
    }
}
