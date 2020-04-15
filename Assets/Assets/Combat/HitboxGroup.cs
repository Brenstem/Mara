using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// DW och Dennis
public class HitboxGroup : MonoBehaviour
{
    public List<GameObject> _alreadyHit;
    private List<Hitbox> _hitTimes;

    void Awake() {
        _alreadyHit = new List<GameObject>();
        _hitTimes = new List<Hitbox>();
    }

    private void EnableChildren() {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children) {
            child.gameObject.SetActive(true);
        }
    }

    private void DisableChildren() {
        for (int i = 0; i < transform.childCount; i++) {
            transform.GetChild(i).gameObject.SetActive(false);
        }
        ResetList();
    }

    void LateUpdate() {
        if (_hitTimes.Count > 0) {
            int highestPriorityIndex = 0;
            for (int i = 1; i < _hitTimes.Count; i++) {
                if (_hitTimes[i].priority < _hitTimes[highestPriorityIndex].priority) {
                    highestPriorityIndex = i;
                }
            }

            foreach (Collider enemy in _hitTimes[highestPriorityIndex].isHit) {
                enemy.gameObject.GetComponent<EnemyHealth>().Damage(-_hitTimes[highestPriorityIndex].damageValue);
                _alreadyHit.Add(enemy.gameObject);
            }

            _hitTimes.Clear();
        }
    }

    public void AddHitbox(Hitbox hitbox) {
        _hitTimes.Add(hitbox);
    }

    private void OnEnable() {
        HitboxHandler.onEnableHitboxes += EnableChildren;
        HitboxHandler.onDisableHitboxes += DisableChildren;
        ResetList();
    }

    private void OnDisable() {
        HitboxHandler.onEnableHitboxes -= EnableChildren;
        HitboxHandler.onDisableHitboxes -= DisableChildren;
        DisableChildren();
    }

    private void ResetList() {
        _alreadyHit.Clear();
    }
}
