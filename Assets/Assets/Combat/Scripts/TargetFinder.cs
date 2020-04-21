using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFinder : MonoBehaviour
{
    [SerializeField] public float trackRadius;
    [SerializeField] float _playerAngle;

    [SerializeField] LayerMask targets;



    public GameObject FindTarget()
    {
        Collider[] toHit = Physics.OverlapSphere(transform.position, trackRadius, targets);
        GameObject targetToHit = null;
        float temp = Mathf.Infinity;
        Vector3 playerRotation = transform.forward;

        foreach (Collider target in toHit)
        {
            if (target != null)
            {
                Vector2 input = GlobalState.state.Player.input.direction;
                /* INPUT DEPENDANT */
                /*if (input != Vector2.zero)
                {

                }
                else
                {
                    */
                    /* DISTANCE DEPENDANT */
                    if (temp > Vector3.Distance(transform.position, target.transform.position))
                    {
                        temp = Vector3.Distance(transform.position, target.transform.position);
                        Vector3 targetRotation = target.transform.position - transform.position;

                        if (_playerAngle * Mathf.Deg2Rad > Mathf.Acos((Vector3.Dot(playerRotation, targetRotation)) / (playerRotation.magnitude * targetRotation.magnitude)))
                        {
                            targetToHit = target.gameObject;
                        }
                    }
                //}
            }
        }
        return targetToHit;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, trackRadius);
    }
}
