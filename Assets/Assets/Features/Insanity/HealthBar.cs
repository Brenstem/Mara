using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public abstract class HealthBar : MonoBehaviour
{
    private Slider _slider;
    public Slider Slider
    {
        get
        {
            if (!_slider)
                _slider = GetComponentInChildren<Slider>();

            return _slider;
        }
    }

    public virtual void SetValue(float amount)
    {
        Slider.value = amount;
    }

    public virtual void SetMaxValue(float amount)
    {
        Slider.maxValue = amount;
    }
}
