using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// DW
public class Hitbox : MonoBehaviour
{
    private HitboxGroup _parent;
    [HideInInspector] public Collider[] isHit;

    [Header("Hitbox stats")]
    [Tooltip("Lower numbers are prioritized"), Range(0, 15)] public int priority;
    public int id;
    public float damageValue;
    public float hitstunTime;
    public bool isParryable;
    [Tooltip("How many seconds to increase the weapons hitstun effect")]
    [SerializeField] private float hitStunIncrease;
    [Tooltip("Percentage multiplier for damage value")]
    [SerializeField] private float damageBuffMultiplier;
    [SerializeField] private Vector3 _size;
    [SerializeField] private Vector3 _offset;

    private float originalDamageValue;
    private float originalHitStun;

    private void Awake()
    {
        originalDamageValue = damageValue;
        originalHitStun = hitstunTime;

        PlayerInsanity.onPlayerDamageBuff += BuffDamage;
        PlayerInsanity.onResetDamageBuff += ResetDamage;
        PlayerInsanity.onIncreaseHitstun += IncreaseHitstun;
        PlayerInsanity.onHeightenedSenses += ResetHitstun;

        _parent = transform.parent.GetComponent<HitboxGroup>();
    }

    private void IncreaseHitstun()
    {
        if (gameObject.CompareTag("Player"))
        {
            hitstunTime += hitStunIncrease;
        }
    }

    private void BuffDamage()
    {
        if (gameObject.CompareTag("Player"))
        {
            damageValue *= damageBuffMultiplier;
        }
    }

    private void ResetDamage()
    {
        damageValue = originalDamageValue;
    }

    private void ResetHitstun()
    {
        hitstunTime = originalHitStun;
    }

    void Update()
    {
        //Lägger in objekt som är i hitboxen i arrayn
        isHit = Physics.OverlapBox(transform.position + _offset, _size * 0.5f, transform.rotation, _parent.targetLayerMask);

        foreach (Collider enemy in isHit)
        {
            if (isHit.Length != 0 && !_parent._alreadyHit.Contains(enemy.gameObject))
            { 
                _parent.AddHitbox(this);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (enabled)
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position + _offset, transform.rotation, transform.localScale);

            switch (priority)
            {
                case int n when (n == 0):
                    Gizmos.color = Color.red;
                    break;
                case int n when (n == 1):
                    Gizmos.color = Color.blue;
                    break;
                case int n when (n == 2):
                    Gizmos.color = Color.magenta;
                    break;
                case int n when (n >= 3):
                    Gizmos.color = Color.green;
                    break;
                default:
                    break;
            }

            Gizmos.DrawWireCube(Vector3.zero, _size);
        }
    }
}
