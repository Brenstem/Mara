using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SisterScreamScript : MonoBehaviour
{
    [SerializeField] private GameObject _sister;

    private LayerMask _triggerDetectionLayers;

    private void Awake()
    {
        _triggerDetectionLayers = (_triggerDetectionLayers | 1 << GlobalState.state.Player.gameObject.gameObject.layer);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_triggerDetectionLayers == (_triggerDetectionLayers | 1 << other.gameObject.layer))
        {
            _sister.GetComponent<MylingAI>().stateMachine.ChangeState(new MylingAttackingState());
            Destroy(this.gameObject);
        }
    }
}
