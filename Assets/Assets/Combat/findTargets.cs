using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class findTargets : MonoBehaviour
{
    [SerializeField] float _radius;
    LayerMask targets;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Collider[] toHit = Physics.OverlapSphere(transform.position, _radius, targets);
    }

    private GameObject FindTarget()
    {


        return targetToHit;
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, _radius);
    }
}
