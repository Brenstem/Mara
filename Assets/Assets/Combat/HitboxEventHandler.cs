using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// DW
public class HitboxEventHandler : MonoBehaviour
{
    public delegate void OnEnableHitboxes(int id);
    public event OnEnableHitboxes onEnableHitboxes;

    public delegate void OnDisableHitboxes(int id);
    public event OnEnableHitboxes onDisableHitboxes;

    public delegate void OnEndAnim();
    public event OnEndAnim onEndAnim;

    public void EnableHitboxes(int id)
    {
        if (onEnableHitboxes != null)
            onEnableHitboxes(id);
        else
            Debug.LogWarning("No object is subscribed to the \"onEnableHitboxes\" event!", this);
    }

    public void DisableHitboxes(int id)
    {
        if (onDisableHitboxes != null)
            onDisableHitboxes(id);
        else
            Debug.LogWarning("No object is subscribed to the \"onDisableHitboxes\" event!", this);
    }

    public void EndAnim()
    {
        if (onEndAnim != null)
            onEndAnim();
        else
            Debug.LogWarning("No object is subscribed to the \"onEndAnim\" event!", this);
    }
}
