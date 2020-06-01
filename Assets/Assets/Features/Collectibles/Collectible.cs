using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    private bool _destroyOnPickup = true;
    private float _incrementAmount = 1;
    private PlayerInsanity _playerInsanity;

    [SerializeField] private Vector3 _dropDirection;
    [SerializeField] private float _dropForce;


    private void Awake()
    {
        Spawned(_dropDirection, _dropForce);
    }

    private void Spawned(Vector3 dropDirection, float dropForce)
    {
        GetComponent<SphereCollider>().enabled = true;
        Physics.IgnoreLayerCollision(13, 11);
        Physics.IgnoreLayerCollision(13, 12);
        GetComponent<Rigidbody>().AddForce(dropDirection.normalized * dropForce);
        GetComponent<FloatingObjectScript>().enabled = false;
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
        print("meme");

        ActivateCollectible();
    }

    private void ActivateCollectible()
    {
        GetComponent<Rigidbody>().isKinematic = true;
        GetComponent<Rigidbody>().useGravity = false;

        GetComponent<SphereCollider>().enabled = false;

        Physics.IgnoreLayerCollision(13, 11, false);
        Physics.IgnoreLayerCollision(13, 12, false);

        GetComponent<FloatingObjectScript>().enabled = true;
    }
}
