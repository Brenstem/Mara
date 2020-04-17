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

    [SerializeField] private HitboxEventHandler _hitboxEventHandler;
    public LayerMask targetLayerMask;

    [HideInInspector] public List<GameObject> _alreadyHit;
    private List<Hitbox> _hitTimes;
    
    void Awake() {
        _alreadyHit = new List<GameObject>();
        _hitTimes = new List<Hitbox>();
        // print(GlobalState.state.PlayerMesh);
        // print(GlobalState.state.PlayerMesh.GetComponent<HitboxEventHandler>());
        if (_hitboxEventHandler == null)
        {
            Debug.LogWarning("HitboxEventHandler missing! Resorting to finding in parent...", this);
            _hitboxEventHandler = GetComponentInParent<HitboxEventHandler>();
            if (_hitboxEventHandler == null)
            {
                Debug.LogError("Unable to find HitboxEventHandler in parent!", this);
            }
            else
            {
                Debug.Log("HitboxEventHandler found. Please add this component as a reference after game session", this);
            }
        }
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

            foreach (Collider enemy in _hitTimes[highestPriorityIndex].isHit)
            {
                if (!_alreadyHit.Contains(enemy.gameObject))
                {
                    Hitbox hit = _hitTimes[highestPriorityIndex];
                    if (targetLayerMask == GlobalState.state.PlayerMask)
                    {
                        if (hit.isParryable && GlobalState.state.PlayerMesh.GetComponent<CombatController>().IsParrying)
                        {
                            //dont do damage, msg player parry successful
                            print("Successful parry");
                            GlobalState.state.PlayerMesh.GetComponent<CombatController>().startTime = true;
                            Time.timeScale = 0.01f;
                        }
                        else
                        {
                            print("failed parry");
                            enemy.gameObject.GetComponent<PlayerInsanity>().IncrementInsanity(hit.damageValue);
                        }
                    }
                    else
                    {
                        var health = enemy.gameObject.GetComponent<EnemyHealth>();
                        if (health == null)
                        {
                            Debug.LogWarning("EnemyHealth missing!", this);
                        }
                        else
                        {
                            enemy.gameObject.GetComponent<EnemyHealth>().Damage(hit.damageValue);
                        }
                    }
                    _alreadyHit.Add(enemy.gameObject);
                }
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
