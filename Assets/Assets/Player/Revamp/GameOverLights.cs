using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverLights : MonoBehaviour
{
    [SerializeField] private float _timeUntilFadeAway = 1.0f;
    [SerializeField] private float _fadeDelay;
    private CanvasGroup _canvasGroup;
    private int currentIndex;
    private List<RawImage> lights;

    private void Awake()
    {
        lights = new List<RawImage>();

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            Debug.LogError("Unable to find CanvasGroup!", this);
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            lights.Add(transform.GetChild(i).GetComponent<RawImage>());
        }

        currentIndex = lights.Count - 1;
        Fade.onFadeExit += Light;
    }

    private void Light()
    {
        StartCoroutine(UpdateLight());
    }

    private IEnumerator UpdateLight()
    {
        if (currentIndex < 0)
        {
            foreach (RawImage item in lights)
            {
                var c = item.color;
                c.a = 1;
                item.color = c;
            }
            currentIndex = lights.Count - 1;
        }
        yield return new WaitForSecondsRealtime(_fadeDelay);
        DisableLight();
    }

    public void DisableLight()
    {
        if (currentIndex >= 0)
        {
            StartCoroutine(FadeEnumerator(1.0f));
        }
    }

    protected IEnumerator FadeEnumerator(float fadeTime)
    {
        float time = 0.0f;
        while (time / fadeTime < 1) // Fade in CavasGroup
        {
            yield return new WaitForFixedUpdate();
            time += Time.fixedDeltaTime;
            GetComponent<CanvasGroup>().alpha = Mathf.Lerp(0.0f, 1.0f, time / fadeTime);
        }

        var c = lights[currentIndex].color;
        c.a = 1;
        lights[currentIndex].color = c;

        time = 0.0f;
        while (time / fadeTime < 1) // Fade away single light
        {
            yield return new WaitForFixedUpdate();
            time += Time.fixedDeltaTime;
            c.a = Mathf.Lerp(1.0f, 0.0f, time / fadeTime);
            lights[currentIndex].color = c;
        }
        c.a = 0;
        lights[currentIndex].color = c;
        currentIndex -= 1;



        yield return new WaitForSecondsRealtime(_timeUntilFadeAway); // delay



        time = 0.0f;
        while (time / fadeTime < 1) // Fade out CavasGroup
        {
            yield return new WaitForFixedUpdate();
            time += Time.fixedDeltaTime;
            GetComponent<CanvasGroup>().alpha = Mathf.Lerp(1.0f, 0.0f, time / fadeTime);
        }

        transform.parent.GetComponent<Fade>().FadeToggle();
    }
}
