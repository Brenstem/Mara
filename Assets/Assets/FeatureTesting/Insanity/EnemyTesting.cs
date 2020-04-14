using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTesting : MonoBehaviour
{
    [SerializeField]
    float damageAmount;

    private PlayerInsanity player;

    void Start()
    {
        player = GlobalState.state.Player.GetComponent<PlayerInsanity>(); 
    }

    void Update()
    { 
        if (Input.GetKeyDown(KeyCode.F))
        {
            player.IncrementInsanity(damageAmount);
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            player.SetMaxInsanity(120);
        }
    }
}
