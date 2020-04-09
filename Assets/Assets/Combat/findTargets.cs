using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindTargets : MonoBehaviour
{
    [SerializeField] float _trackRadius;
    [SerializeField] float _playerAngle;

    [SerializeField] LayerMask targets;

   

    public GameObject FindTarget()
    {
        Collider[] toHit = Physics.OverlapSphere(transform.position, _trackRadius, targets);
        GameObject targetToHit = null;
        float temp = Mathf.Infinity;
        Vector3 playerRotation = transform.forward;

        foreach (Collider target in toHit)
        {
            if (temp > Vector3.Distance(transform.position, target.transform.position))
            {
                temp = Vector3.Distance(transform.position, target.transform.position);
                Vector3 targetRotation = target.transform.position - transform.position;

                if ( _playerAngle*Mathf.Deg2Rad > Mathf.Acos((Vector3.Dot(playerRotation, targetRotation)) / (playerRotation.magnitude * targetRotation.magnitude)))
                {
                    //print("Hello");
                    targetToHit = target.gameObject;
                }
            }
        }
        return targetToHit;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, _trackRadius);
    }
}

//Mathf.Acos((Vector3.Dot(playerRotation, targetRotation)) / (playerRotation.magnitude* targetRotation.magnitude))
//Vector3.Angle(transform.forward, new Vector3(target.transform.rotation.x, 0, target.transform.rotation.z))
