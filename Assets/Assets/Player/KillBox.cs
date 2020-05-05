using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillBox : MonoBehaviour
{
    private void OnTriggerEnter(Collider hitInfo)
    {
        if (hitInfo.CompareTag("Player"))
        {
            hitInfo.GetComponent<PlayerInsanity>().Damage(50000000);
        }

        if (hitInfo.CompareTag("Enemy"))
        {
            Destroy(hitInfo.gameObject);
        }
    }
}
