using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Entity
{
    public PlayerInsanity playerInsanity;
    public LockonFunctionality lockonFunctionality;

    public MovementController movementController;
    public CombatController combatController;

    public override void TakeDamage(Hitbox hitbox)
    {
        if (combatController.IsParrying)
        {
            print("Parry successful");
        }
        else
        {
            playerInsanity.IncrementInsanity(hitbox.damageValue);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
