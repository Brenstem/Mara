using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class TrackingHealthBar : MonoBehaviour
{
    private Transform _cam;

    public RectTransform Transform
    {
        get
        {
            if(_transform == null)
                _transform = GetComponent<RectTransform>();
            return _transform;
        }
    }

    private RectTransform _transform;

    public Slider Slider
    {
        get
        {
            if (_slider == null)
                _slider = GetComponent<Slider>();
            return _slider;
        }
    }
    private Slider _slider;

    private void Awake()
    {
        _cam = Camera.main.transform;
    }

    private void LateUpdate()
    {
        transform.LookAt(transform.position + _cam.forward);
    }

    public void SetValue(float amount)
    {
        Slider.value = amount;
        print("value " + Slider.value);
    }

    public void SetMaxValue(float amount)
    {
        print("amount " + amount);
        Slider.maxValue = amount;
        Transform.sizeDelta = new Vector2(amount, 10);
    }
}
