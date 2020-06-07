using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MylingScream : MonoBehaviour
{
    [SerializeField] private MylingAI _myling;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            print("meme");
            GlobalState.state.AudioManager.MylingDiesAudio(_myling.transform.position);
        }
    }
}
