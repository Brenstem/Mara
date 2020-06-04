/*
 _____                           _  ______                      _        
|  __ \                         | | | ___ \                    | |        
| |  \/ ___ _ __   ___ _ __ __ _| | | |_/ / __ _ _______   ___ | | ____ _
| | __ / _ \ '_ \ / _ \ '__/ _` | | | ___ \/ _` |_  / _ \ / _ \| |/ / _` |
| |_\ \  __/ | | |  __/ | | (_| | | | |_/ / (_| |/ / (_) | (_) |   < (_| |
 \____/\___|_| |_|\___|_|  \__,_|_| \____/ \__,_/___\___/ \___/|_|\_\__,_|
 
==***==                                                                                                                      
* In a lazy sunday day.
*/

using UnityEngine;
using System.Collections;

public class RotateAroundPivot : MonoBehaviour // source: https://forum.unity.com/threads/pivot-rotation.152215/#post-1043155
{
    public Vector3 Pivot;
    public Vector3 Magnitude;
    public bool DebugInfo = true;

    //it could be a Vector3 but its more user friendly
    public bool RotateX = false;
    public bool RotateY = true;
    public bool RotateZ = false;

    void FixedUpdate()
    {
        transform.position += (transform.rotation * Pivot);

        if (RotateX)
            transform.rotation *= Quaternion.AngleAxis(45 * Time.deltaTime * Magnitude.x, Vector3.right);
        if (RotateY)
            transform.rotation *= Quaternion.AngleAxis(45 * Time.deltaTime * Magnitude.y, Vector3.up);
        if (RotateZ)
            transform.rotation *= Quaternion.AngleAxis(45 * Time.deltaTime * Magnitude.z, Vector3.forward);

        transform.position -= (transform.rotation * Pivot);

    }

    private void OnDrawGizmosSelected()
    {
        if (DebugInfo)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawRay(transform.position, transform.rotation * Vector3.up);
            Gizmos.DrawRay(transform.position, transform.rotation * Vector3.right);
            Gizmos.DrawRay(transform.position, transform.rotation * Vector3.forward);

            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position + (transform.rotation * Pivot), transform.rotation * Vector3.up);
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position + (transform.rotation * Pivot), transform.rotation * Vector3.right);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position + (transform.rotation * Pivot), transform.rotation * Vector3.forward);
        }
    }
}