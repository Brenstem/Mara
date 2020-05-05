using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTesting : MonoBehaviour
{
    [SerializeField]
    float damageAmount;

    private PlayerRevamp player;

    void Start()
    {
        player = GlobalState.state.PlayerGameObject.GetComponent<PlayerRevamp>(); 
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            HitboxValues h = new HitboxValues()
            {
                damageValue = damageAmount
            };

            player.TakeDamage(damageAmount);
        }
    }
}
