using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    public abstract void TakeDamage(Hitbox hitbox);
    public abstract void TakeDamage(float damage);
    public bool invulerable;
}
