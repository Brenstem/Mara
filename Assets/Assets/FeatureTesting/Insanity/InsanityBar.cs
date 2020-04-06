using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InsanityBar : MonoBehaviour
{
    [SerializeField]
    Slider slider;

    public void SetInsanity(float insanityValue)
    {
        slider.value = insanityValue;
    }

    public void SetMaxInsanity(float insanityValue)
    {
        slider.maxValue = insanityValue;
    }
}
