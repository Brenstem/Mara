using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct HitboxValues
{
    public float damageValue;
    public float hitstunTime;
    public float hitstopTime;
    public bool parryable;
    public bool ignoreArmor;

    public static HitboxValues operator *(HitboxValues h, EntityModifier m)
    {
        h.damageValue *= m.DamageMultiplier;
        h.hitstunTime *= m.HitstunMultiplier;
        return h;
    }
}

// DW
public class Hitbox : MonoBehaviour
{
    private HitboxGroup _parent;
    [HideInInspector] public Collider[] isHit;

    [Header("Hitbox stats")]
    [Tooltip("Lower numbers are prioritized"), Range(0, 15)] public int priority;
    public int id;
    public LayerMask targetLayerMask;

    public HitboxValues hitboxValues;

    [Header("Cube Hitbox")]
    [SerializeField] private Vector3 _size;
    [SerializeField] private Vector3 _offset;
    [SerializeField] private Transform followPoint;

    [Header("Sphere Hitbox")]
    [SerializeField] private bool circleCollider;
    [SerializeField] private float circleRadius;

    private float originalDamageValue;
    private float originalHitStun;

    private void Awake()
    {
        originalDamageValue = hitboxValues.damageValue;
        originalHitStun = hitboxValues.hitstunTime;

        _parent = transform.parent.GetComponent<HitboxGroup>();
    }

    private void OnEnable() { } // TODO: Parryable indicator
    private void OnDisable() { }

    void FixedUpdate()
    {
        if (followPoint != null)
        {
            transform.position = followPoint.position + _offset;
            transform.rotation = followPoint.rotation;
        }

        LayerMask mask = targetLayerMask == 0 ? _parent.targetLayerMask : targetLayerMask;

        //Lägger in objekt som är i hitboxen i arrayn
        //isHit = Physics.OverlapBox(transform.position + _offset, _size * 0.5f, transform.rotation, _parent.targetLayerMask);
        if (circleCollider)
        {
            isHit = Physics.OverlapSphere(transform.TransformPoint(_offset), circleRadius, mask);
        }
        else
        {
            isHit = Physics.OverlapBox(transform.TransformPoint(_offset), _size * 0.5f, transform.rotation, mask);
        }


        foreach (Collider enemy in isHit)
        {
            if (isHit.Length != 0 && !_parent._alreadyHit.Contains(enemy.gameObject)) // optimera contains så att om den finns så sätts en bool så att den inte behöver kolla igenom hela tiden, utan det är en dynamic algorithm
            {
                _parent.AddHitbox(this);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (enabled)
        {
            //Gizmos.matrix = Matrix4x4.TRS(transform.position + _offset, transform.rotation, transform.localScale);

            if (followPoint == null)
                Gizmos.matrix = Matrix4x4.TRS(transform.TransformPoint(_offset), transform.rotation, transform.localScale);
            else
                Gizmos.matrix = Matrix4x4.TRS(followPoint.TransformPoint(_offset), followPoint.rotation, transform.localScale);

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

            if (circleCollider)
            {
                Gizmos.DrawWireSphere(Vector3.zero, circleRadius);
            }
            else
            {
                Gizmos.DrawWireCube(Vector3.zero, _size);
            }
        }
    }
}
