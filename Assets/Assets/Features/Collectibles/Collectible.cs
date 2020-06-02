using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    private bool _destroyOnPickup = true;
    private float _incrementAmount = 1;
    private PlayerInsanity _playerInsanity;

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
}
