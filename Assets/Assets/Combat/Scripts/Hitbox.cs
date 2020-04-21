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
    public bool isParryable;
    [SerializeField] private Vector3 _size;
    [SerializeField] private Vector3 _offset;

    private void Awake()
    {
        _parent = GetComponentInParent<HitboxGroup>();
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

    private void OnDrawGizmos()
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
