using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private float _lifeSpan;
    [SerializeField] private float _damage;

    private Rigidbody _rb;
    private Timer lifespanTimer;
    private Vector3 _playerPos;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        lifespanTimer = new Timer(_lifeSpan);
        _playerPos = GlobalState.state.Player.transform.position;
    }

    private void Start()
    {
        Vector3 _targetOffset = new Vector3(0, 1.2f, 0);
        Vector3 destination = (_playerPos + _targetOffset) - this.transform.position;
        destination = (destination.normalized) * _speed;
        _rb.velocity = destination;
    }

    private void Update()
    {
        lifespanTimer.Time += Time.deltaTime;

        if (lifespanTimer.Expired)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter(Collider hitInfo)
    {
        if (!hitInfo.CompareTag("Lockon"))
        {
            StartCoroutine(DestroyProjectile());
        }
    }

    IEnumerator DestroyProjectile()
    {
        yield return StartCoroutine(WaitFor.Frames(2));
        Destroy(this.gameObject);
    }
}
