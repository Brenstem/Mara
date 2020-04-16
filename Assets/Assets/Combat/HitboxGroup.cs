using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// DW och Dennis
public class HitboxGroup : MonoBehaviour
{
    public delegate void OnEnableHitboxes(int id);
    public event OnEnableHitboxes onEnableHitboxes;

    public delegate void OnDisableHitboxes(int id);
    public event OnDisableHitboxes onDisableHitboxes;

    private HitboxEventHandler _hitboxEventHandler;
    public List<GameObject> _alreadyHit;
    private List<Hitbox> _hitTimes;
    
    void Awake() {
        _alreadyHit = new List<GameObject>();
        _hitTimes = new List<Hitbox>();
        print(GlobalState.state.PlayerMesh);
        print(GlobalState.state.PlayerMesh.GetComponent<HitboxEventHandler>());
        _hitboxEventHandler = GlobalState.state.PlayerMesh.GetComponent<HitboxEventHandler>();
        _hitboxEventHandler.onEnableHitboxes += EnableEvent;
        _hitboxEventHandler.onDisableHitboxes += DisableEvent;
        _hitboxEventHandler.onEndAnim += ResetList;
    }

    private void EnableEvent(int id)
    {
        if (onEnableHitboxes != null)
            onEnableHitboxes(id);
        else
            Debug.LogWarning("No object is subscribed to the \"onEnableHitboxes\" event!", this);

    }

    private void DisableEvent(int id)
    {
        if (onDisableHitboxes != null)
            onDisableHitboxes(id);
        else
            Debug.LogWarning("No object is subscribed to the \"onDisableHitboxes\" event!", this);
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
                enemy.gameObject.GetComponent<EnemyHealth>().Damage(_hitTimes[highestPriorityIndex].damageValue);
                _alreadyHit.Add(enemy.gameObject);
            }

            _hitTimes.Clear();
        }
    }

    public void AddHitbox(Hitbox hitbox) {
        _hitTimes.Add(hitbox);
    }

    private void OnEnable()
    {
        _hitboxEventHandler.onEnableHitboxes += EnableEvent;
        _hitboxEventHandler.onDisableHitboxes += DisableEvent;
        _hitboxEventHandler.onEndAnim += ResetList;

        ResetList();
    }

    private void OnDisable() {
        _hitboxEventHandler.onEnableHitboxes -= EnableEvent;
        _hitboxEventHandler.onDisableHitboxes -= DisableEvent;
        _hitboxEventHandler.onEndAnim -= ResetList;
    }

    private void ResetList() {
        DisableEvent(0);
        _alreadyHit.Clear();
    }



    /*
    private void EnableChildren(int id) {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children) {
            child.gameObject.SetActive(true);
        }
    }

    private void DisableChildren(int id) {
        for (int i = 0; i < transform.childCount; i++) {
            transform.GetChild(i).gameObject.SetActive(false);
        }
        ResetList();
    }
    */

}
