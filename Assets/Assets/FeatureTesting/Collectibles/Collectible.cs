using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    [SerializeField]
    private float incrementAmount;

    private PlayerInsanity _playerInsanity;
    private void OnTriggerEnter(Collider hitInfo)
    {
        if (hitInfo.CompareTag("Player"))
        {
            _playerInsanity = hitInfo.GetComponent<PlayerInsanity>();

            _playerInsanity.IncrementMaxInsanity(incrementAmount);
        }
    }
}
