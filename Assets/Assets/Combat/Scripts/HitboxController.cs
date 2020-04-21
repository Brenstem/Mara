using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// DW
[RequireComponent(typeof(Hitbox))]
public class HitboxController : MonoBehaviour {
    private HitboxGroup _parent;
    private Hitbox _hitbox;

    private void Awake()  {
        _parent = GetComponentInParent<HitboxGroup>();
        _hitbox = GetComponent<Hitbox>();
        _hitbox.enabled = false;
    }

    private void OnEnable()
    {
        _parent.onEnableHitboxes += Enable;
        _parent.onDisableHitboxes += Disable;
    }

    private void OnDisable()
    {
        _parent.onEnableHitboxes -= Enable;
        _parent.onDisableHitboxes -= Disable;
    }

    private void Enable(int id)
    {
        if (id < 1 || _hitbox.id == id) // If ID is less than or equal to 0; every hitbox is enabled
        {
            _hitbox.enabled = true;
        }
    }

    private void Disable(int id)
    {
        if (id < 1 || _hitbox.id == id)
        {
            _hitbox.enabled = false;
        }
    }
}
