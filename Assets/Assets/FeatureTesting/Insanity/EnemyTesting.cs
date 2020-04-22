using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTesting : MonoBehaviour
{
    [SerializeField]
    float damageAmount;

    private Player player;

    void Start()
    {
        player = GlobalState.state.PlayerGameObject.GetComponent<Player>(); 
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            player.TakeDamage(damageAmount);
        }
    }
}
