using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealthBar : HealthBar
{
    public override void SetValue(float amount)
    {
        Slider.value = amount;
    }

    public override void SetMaxValue(float amount)
    {
        Slider.maxValue = amount;
    }
}
