using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class swordCollision : MonoBehaviour
{
    [SerializeField] float _sizeX;
    [SerializeField] float _sizeY;
    [SerializeField] float _sizeZ;
    [SerializeField] float _damageValue;

    private List<GameObject> _alreadyHit;

    [SerializeField] LayerMask _targetLayerMask;
    private void Awake()
    {
        _alreadyHit = new List<GameObject>();
    }

    void Update()
    {
        Collider[] isHit = Physics.OverlapBox(transform.position, new Vector3(_sizeX / 2, _sizeY / 2, _sizeZ), Quaternion.identity, _targetLayerMask);
        foreach(Collider enemy in isHit)
        {
            if (isHit.Length != 0 && !_alreadyHit.Contains(enemy.gameObject))
            {
                enemy.gameObject.GetComponent<EnemyHealth>().Damage(-_damageValue);
                _alreadyHit.Add(enemy.gameObject);
            }

        }
        
    }

    private void OnEnable()
    {
        ResetList();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, new Vector3(_sizeX, _sizeY, _sizeZ));
      
    }
    private void ResetList()
    {
        _alreadyHit.Clear();
    }
}
