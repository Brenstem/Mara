using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MylingAggro : MonoBehaviour
{
    [SerializeField] private MylingAI _myling;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            print("meme");
            _myling._aggroRange = 19f;
        }
    }
}
