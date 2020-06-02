using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class TrackingHealthBar : HealthBar
{
    [SerializeField] private bool _billBoarding;

    private Transform _cam;

    // Get transform component if null
    private RectTransform _transform;
    public RectTransform Transform
    {
        get
        {
            if(!_transform)
                _transform = GetComponent<RectTransform>();

            return _transform;
        }
    }

    private void Awake()
    {
        _cam = GlobalState.state.Camera.transform;
    }

    // Health bar billboarding
    private void LateUpdate()
    {
        if (_billBoarding)
        {
            transform.LookAt(transform.position + _cam.forward);
        }
    }

    public override void SetMaxValue(float amount)
    {
        Slider.maxValue = amount;
        Transform.sizeDelta = new Vector2(amount, 10);
    }
}
