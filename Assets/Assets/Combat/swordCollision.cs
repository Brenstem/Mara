using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class swordCollision : MonoBehaviour
{
    [SerializeField] float _sizeX;
    [SerializeField] float _sizeY;
    [SerializeField] float _sizeZ;
    [SerializeField] public float _damageValue;
    [SerializeField] public int _priority;

    [HideInInspector] public Collider[] isHit;

    HitboxGroup parent;

    [SerializeField] LayerMask _targetLayerMask;
    private void Awake()
    {
        parent = GetComponentInParent<HitboxGroup>();
    }

    void Update()
    {
        //Lägger in objekt som är i hitboxen i arrayn
        isHit = Physics.OverlapBox(transform.position, new Vector3(_sizeX / 2, _sizeY / 2, _sizeZ/2), Quaternion.identity, _targetLayerMask);

        foreach(Collider enemy in isHit)
        {
            if (isHit.Length != 0 && !parent._alreadyHit.Contains(enemy.gameObject))
            {
                parent.AddHitbox(this);
            }
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(_sizeX, _sizeY, _sizeZ));
      
    }

}
