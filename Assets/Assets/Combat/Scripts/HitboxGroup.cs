using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// DW
public class HitboxGroup : MonoBehaviour
{
    public delegate void OnEnableHitboxes(int id);
    public event OnEnableHitboxes onEnableHitboxes;

    public delegate void OnDisableHitboxes(int id);
    public event OnDisableHitboxes onDisableHitboxes;

    public HitboxEventHandler hitboxEventHandler;
    public LayerMask targetLayerMask;

    [HideInInspector] public List<GameObject> _alreadyHit;
    private List<Hitbox> _hitTimes;

    [SerializeField] private bool _eventLess;

    void Awake()
    {
        _alreadyHit = new List<GameObject>();
        _hitTimes = new List<Hitbox>();
        if (!_eventLess)
        {
            if (hitboxEventHandler == null)
            {
                Debug.LogWarning("HitboxEventHandler missing! Resorting to finding in parent...", this);
                hitboxEventHandler = GetComponentInParent<HitboxEventHandler>();
                if (hitboxEventHandler == null)
                {
                    Debug.LogError("Unable to find HitboxEventHandler in parent!", this);
                }
                else
                {
                    Debug.Log("HitboxEventHandler found. Please add this component as a reference after game session", this);
                }
            }
            hitboxEventHandler.onEnableHitboxes += EnableEvent;
            hitboxEventHandler.onDisableHitboxes += DisableEvent;
            hitboxEventHandler.onEndAnim += ResetList;
        }
        else
        {
            EnableEvent(0);
        }
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

    void LateUpdate()
    {
        if (_hitTimes.Count > 0)
        {
            int highestPriorityIndex = 0;
            for (int i = 1; i < _hitTimes.Count; i++)
            {
                if (_hitTimes[i].priority < _hitTimes[highestPriorityIndex].priority)
                {
                    highestPriorityIndex = i;
                }
            }

            foreach (Collider enemy in _hitTimes[highestPriorityIndex].isHit)
            {
                if (!_alreadyHit.Contains(enemy.gameObject))
                {
                    Hitbox hit = _hitTimes[highestPriorityIndex];
                    var entity = enemy.gameObject.GetComponent<Entity>();
                    if (entity == null)
                    {
                        Debug.LogWarning("Object derived from Entity class is missing! Resorting to find in children...", this);
                        entity = enemy.gameObject.GetComponentInChildren<Entity>();
                        if (entity == null)
                        {
                            Debug.LogError("Object derived from Entity class is missing from \"" + enemy.gameObject.name + "\"!", this);
                        }
                        else
                        {
                            entity.TakeDamage(hit);
                        }
                    }
                    else
                    {
                        entity.TakeDamage(hit);
                    }
                    _alreadyHit.Add(enemy.gameObject);
                }
            }

            _hitTimes.Clear();
        }
    }

    public void AddHitbox(Hitbox hitbox)
    {
        _hitTimes.Add(hitbox);
    }

    private void OnEnable()
    {
        if (!_eventLess)
        {
            hitboxEventHandler.onEnableHitboxes += EnableEvent;
            hitboxEventHandler.onDisableHitboxes += DisableEvent;
            hitboxEventHandler.onEndAnim += ResetList;
        }

        ResetList();
    }

    private void OnDisable()
    {
        if (!_eventLess)
        {
            hitboxEventHandler.onEnableHitboxes -= EnableEvent;
            hitboxEventHandler.onDisableHitboxes -= DisableEvent;
            hitboxEventHandler.onEndAnim -= ResetList;
        }
    }

    private void ResetList()
    {
        DisableEvent(0);
        _alreadyHit.Clear();
    }
}
