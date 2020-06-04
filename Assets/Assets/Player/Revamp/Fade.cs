using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fade : MonoBehaviour
{
    [SerializeField] private bool fadeTest;

    public delegate void OnFadeEnter();
    public static event OnFadeEnter onFadeEnter;

    public delegate void OnFadeExit();
    public static event OnFadeExit onFadeExit;

    [SerializeField] protected CanvasGroup _canvasGroup;
    [SerializeField, Range(0, 1)] protected float _alphaMax = 1.0f;
    [SerializeField] protected float _toBlackFadeTime = 1.0f;
    [SerializeField] protected float _toTransparentFadeTime = 1.0f;

    // KALLA DENHÄR NÄR DU SKA BÖRJA FADE
    public void FadeToggle()
    {
        if (Alpha == 0)
            FadeToTransparent();
        else
            FadeToBlack();
    }

    private float _alpha;
    protected float Alpha
    {
        get { return _canvasGroup.alpha; }
        set
        {
            if (value <= 0)
                _alpha = 0;
            else if (value >= _alphaMax)
                _alpha = _alphaMax;
            else
                _alpha = value;

            _canvasGroup.alpha = _alpha;
        }
    }

    public void FadeToBlack()
    {
        StartCoroutine(FadeEnumerator(_toTransparentFadeTime, -1));
    }
    
    public void FadeToTransparent()
    {
        StartCoroutine(FadeEnumerator(_toBlackFadeTime, 1));
    }

    protected IEnumerator FadeEnumerator(float fadeTime, int i = 1)
    {
        float time = 0.0f;
        int binVal = (1 + i) / 2; // 0 if i == -1 || 1 if i == 1
        Alpha = 1 - binVal;

        if (onFadeEnter != null && i == 1)
            onFadeEnter();

        while (time / fadeTime < 1)
        {
            yield return new WaitForFixedUpdate();
            time += Time.fixedDeltaTime;
            Alpha = (1 - binVal) + (time / fadeTime) * i;
            //Alpha = Mathf.Lerp(binVal, (1 - binVal) * _alphaMax, (time / fadeTime) * i);
        }
        Alpha = binVal;

        if (onFadeExit != null && i == 1)
            onFadeExit();
    }

    private void OnValidate()
    {
        if (fadeTest)
        {
            fadeTest = false;
            if (Alpha == 0)
                FadeToTransparent();
            else
                FadeToBlack();
        }
    }
}
