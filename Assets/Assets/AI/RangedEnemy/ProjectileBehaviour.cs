using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
//using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    [SerializeField] public float _speed;
    [SerializeField] private float _lifeSpan;

    public float Speed { get { return _speed; } }

    private Rigidbody _rb;
    private Timer lifespanTimer;
    private Vector3 _playerPos;
    Vector3 destination;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        lifespanTimer = new Timer(_lifeSpan);
        _playerPos = GlobalState.state.Player.transform.position;
    }

    private void Start()
    {
        Vector3 offset = new Vector3(0, 1.2f, 0);
        Vector3 direction = (_playerPos + offset) - this.transform.position;

        GlobalState.state.AudioManager.RangedEnemyFireAudio(this.transform.position);

        direction = direction.normalized;

        direction.x = transform.forward.x;
        direction.z = transform.forward.z;

        destination = (direction.normalized) * _speed;

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
        if (!hitInfo.CompareTag("Player"))
        {
            StartCoroutine(DestroyProjectile());
        }
    }

    IEnumerator DestroyProjectile()
    {
        yield return StartCoroutine(WaitFor.Frames(3));
        GlobalState.state.AudioManager.RangedProjectileHit(this.transform.position);
        Destroy(this.gameObject);
    }
}
