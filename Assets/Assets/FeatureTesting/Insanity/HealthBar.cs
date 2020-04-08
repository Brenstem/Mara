using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class HealthBar : MonoBehaviour
{   
    private Slider _slider;

    private void Awake()
    {
        _slider = GetComponent<Slider>();
    }

    public void SetValue(float amount)
    {
        _slider.value = amount;
    }

    public void SetMaxValue(float amount)
    {
        _slider.maxValue = amount;
    }
}
