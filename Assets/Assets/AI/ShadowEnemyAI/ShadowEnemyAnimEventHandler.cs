using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShadowEnemyAnimEventHandler : MonoBehaviour
{
    public void DestroyThis()
    {
        Destroy(this.transform.parent.gameObject);
    }
}
