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

    private bool _eventLess;
    [SerializeField] private bool _enabledByDefault;
    private Entity _parentEntity;

    void Awake() {
        _alreadyHit = new List<GameObject>();
        _hitTimes = new List<Hitbox>();

        _eventLess = GetComponentInChildren<HitboxController>() == null ? true : false;

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
        }
        else
        {
            EnableEvent(0);
        }

        _parentEntity = GetComponentInParent<Entity>();
        enabled = _enabledByDefault;
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
        if (!_eventLess)
            HitDetection();
    }

    private void HitDetection()
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
                if (enemy != null && !_alreadyHit.Contains(enemy.gameObject) && enemy)
                {
                    var targetEntity = enemy.gameObject.GetComponent<Entity>();
                    if (targetEntity != null && !targetEntity.invulerable) // intangible behavior atm, stöd för båda borde finnas! Man blir samt slagen om invun. tar slut medans man blir träffad
                    {
                        Hitbox hitbox = _hitTimes[highestPriorityIndex];
                        if (targetEntity == null)
                        {
                            Debug.LogWarning("Object derived from Entity class is missing! Resorting to find in children...", this);
                            targetEntity = enemy.gameObject.GetComponentInChildren<Entity>();
                            if (targetEntity == null)
                            {
                                Debug.LogError("Object derived from Entity class is missing from \"" + enemy.gameObject.name + "\"!", this);
                            }
                            else
                            {
                                TakeDamage(targetEntity, hitbox);
                            }
                        }
                        else
                        {
                            TakeDamage(targetEntity, hitbox);
                        }
                        _alreadyHit.Add(enemy.gameObject);
                    }
                }
            }

            _hitTimes.Clear();
        }
    }

    private void TakeDamage(Entity target, Hitbox hitbox)
    {
        if (_parentEntity != null && _parentEntity.modifier != null)
        {
            if (_parentEntity.modifier.IsModified)
                hitbox *= _parentEntity.modifier; // overloaded * operator
        }
        
        target.TakeDamage(hitbox);

        if (hitbox.hitstopTime > 0)
        {
            GlobalState.state.HitStop(hitbox.hitstopTime);
        }
    }



    public void AddHitbox(Hitbox hitbox)
    {
        _hitTimes.Add(hitbox);
        if (_eventLess)
        {
            HitDetection();
        }
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

    private void OnDisable() {
        if (!_eventLess)
        {
            hitboxEventHandler.onEnableHitboxes -= EnableEvent;
            hitboxEventHandler.onDisableHitboxes -= DisableEvent;
            hitboxEventHandler.onEndAnim -= ResetList;
        }
        DisableEvent(0);
    }

    private void ResetList()
    {
        _alreadyHit.Clear();
    }
}
