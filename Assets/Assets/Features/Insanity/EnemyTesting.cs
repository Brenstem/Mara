using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTesting : Entity
{
    [SerializeField] float damageAmount;
    [SerializeField] HitboxGroup hitboxGroup;

    private PlayerRevamp player;
    
    public override void KillThis()
    {
        Debug.LogError("kill implementation is not implemented, how did you manage to kill this??", this);
    }

    public override void Parried()
    {
        GetComponent<Animator>().SetTrigger("Parried");
        hitboxGroup.DisableEvent();
    }

    public override void TakeDamage(HitboxValues hitbox, Entity attacker = null)
    {
        print("oof, owie jag tog damage!!");
    }

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
