using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitboxGroup : MonoBehaviour
{
    public List<GameObject> _alreadyHit;
    private List<swordCollision> _hitTimes;

    void Awake()
    {
        _alreadyHit = new List<GameObject>();
        _hitTimes = new List<swordCollision>();
    }

    void LateUpdate()
    {
        if (_hitTimes.Count > 0)
        {
            int highestPriorityIndex = 0;
            for (int i = 1; i < _hitTimes.Count; i++)
            {
                if(_hitTimes[i]._priority < _hitTimes[highestPriorityIndex]._priority)
                {
                    highestPriorityIndex = i;
                }
            }

            foreach (Collider enemy in _hitTimes[highestPriorityIndex].isHit)
            {
                enemy.gameObject.GetComponent<EnemyHealth>().IncrementHealth(-_hitTimes[highestPriorityIndex]._damageValue);
                _alreadyHit.Add(enemy.gameObject);
            }

            _hitTimes.Clear();
        }
    }

    public void AddHitbox(swordCollision hitbox)
    {
        _hitTimes.Add(hitbox);
    }
    private void OnEnable()
    {
        ResetList();
    }

    private void ResetList()
    {
        _alreadyHit.Clear();
    }
}
