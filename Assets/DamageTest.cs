using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTest : MonoBehaviour
{
    [Header("PROTOTYPING ONLY")]

    public float damage;
    public EnemyHealth enemy;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            enemy.IncrementHealth(-damage);
        }
    }
}
