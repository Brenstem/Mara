using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class TrackingHealthBar : MonoBehaviour
{
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

    public void SetValue(float amount)
    {
        Slider.value = amount;
    }

    public void SetMaxValue(float amount)
    {
        Slider.maxValue = amount;
    }
}
