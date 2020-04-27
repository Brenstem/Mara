using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField]
    private float incrementAmount;

    [SerializeField]
    private bool destroyOnPickup;

    private PlayerInsanity _playerInsanity;
    private void OnTriggerEnter(Collider hitInfo)
    {
        if (hitInfo.CompareTag("Player"))
        {
            GlobalState.state.AudioManager.CollectibleAudio(this.transform.position);
            _playerInsanity = hitInfo.GetComponent<PlayerInsanity>();
            
            _playerInsanity.IncrementMaxInsanity(incrementAmount);

            if (destroyOnPickup)
            {
                Destroy(this.gameObject);
            }
        }
    }
}
