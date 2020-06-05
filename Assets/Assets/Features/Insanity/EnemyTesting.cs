using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTesting : MonoBehaviour
{ 
    [SerializeField] float damageAmount = 1;

    private PlayerRevamp player;

    void Start()
    {
        player = GlobalState.state.Player.gameObject.GetComponent<PlayerRevamp>(); 
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            player.TakeDamage(damageAmount);
        }
    }
}
