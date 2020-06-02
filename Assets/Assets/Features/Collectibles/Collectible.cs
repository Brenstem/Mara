using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    private bool _destroyOnPickup = true;
    private float _incrementAmount = 1;
    private PlayerInsanity _playerInsanity;
    private Collider _playerCollider;
    private SphereCollider _mySphereCollider;
    private Rigidbody _myRigidbody;


    private void Awake()
    {
        _playerCollider = GlobalState.state.Player.GetComponent<Collider>();
        _mySphereCollider = GetComponent<SphereCollider>();
        _myRigidbody = GetComponent<Rigidbody>();

        Physics.IgnoreCollision(_mySphereCollider, _playerCollider, true);

        Spawned(Vector3.zero, 0f);
    }

    public void Spawned(Vector3 dropDirection, float dropForce)
    {
        _myRigidbody.AddForce(dropDirection.normalized * dropForce);
    }

    public void SetIncrementAmount(float amount)
    {
        _incrementAmount = amount;
    }

    private void OnTriggerEnter(Collider hitInfo)
    {
        if (hitInfo.CompareTag("Player"))
        {
            GlobalState.state.AudioManager.CollectibleAudio(this.transform.position);
            _playerInsanity = hitInfo.GetComponent<PlayerInsanity>();
            
            _playerInsanity.MaxHealth += _incrementAmount;

            if (_destroyOnPickup)
            {
                Destroy(this.gameObject);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        ActivateCollectible();
    }

    private void ActivateCollectible()
    {
        _myRigidbody.isKinematic = true;
        _myRigidbody.useGravity = false;

        _mySphereCollider.enabled = false;
        GetComponent<FloatingObjectScript>().enabled = true;

        Physics.IgnoreCollision(_mySphereCollider, _playerCollider, false);
    }
}
