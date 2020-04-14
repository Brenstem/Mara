using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// DW och Dennis
public class Hitbox : MonoBehaviour {
    private HitboxGroup _parent;
    [HideInInspector] public Collider[] isHit;
    [SerializeField] private LayerMask _targetLayerMask;

    [Header("Hitbox stats")]
    public int priority;
    public float damageValue;
    [SerializeField] private Vector3 _size;
    [SerializeField] private Vector3 _offset;

    private void Awake()  {
        _parent = GetComponentInParent<HitboxGroup>();
    }

    void Update() {
        //Lägger in objekt som är i hitboxen i arrayn
        isHit = Physics.OverlapBox(transform.position + _offset, _size * 0.5f, transform.rotation, _targetLayerMask);

        foreach (Collider enemy in isHit) {
            if (isHit.Length != 0 && !_parent._alreadyHit.Contains(enemy.gameObject)) {
                _parent.AddHitbox(this);
            }
        }
    }

    private void OnDrawGizmos() {
        Gizmos.matrix = Matrix4x4.TRS(transform.position + _offset, transform.rotation, transform.localScale);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, _size);
    }

}
