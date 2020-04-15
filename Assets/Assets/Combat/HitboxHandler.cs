using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// DW
public class HitboxHandler : MonoBehaviour
{
    public delegate void OnEnableHitboxes();
    public static event OnEnableHitboxes onEnableHitboxes;

    public delegate void OnDisableHitboxes();
    public static event OnEnableHitboxes onDisableHitboxes;

    public void EnableHitboxes() {
        onEnableHitboxes();
    }

    public void DisableHitboxes() {
        onDisableHitboxes();
    }
}
