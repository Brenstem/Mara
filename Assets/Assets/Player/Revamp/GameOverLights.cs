using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverLights : MonoBehaviour
{
    [SerializeField] private float _timeUntilFadeAway = 1.0f;
    [SerializeField] private float _fadeDelay;
    private CanvasGroup _canvasGroup;
    private List<RawImage> lights;

    private Canvas _canvas;
    private Canvas Canvas
    {
        get
        {
            if (_canvas == null)
                _canvas = GetComponentInParent<Canvas>();
            return _canvas;
        }
    }

    private void Awake()
    {
        lights = new List<RawImage>();

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            Debug.LogError("Unable to find CanvasGroup!", this);
        }

        if (SceneData.lightIndex == 0)
            SceneData.lightIndex = lights.Count - 1;

        for (int i = 0; i < transform.childCount; i++)
        {
            lights.Add(transform.GetChild(i).GetComponent<RawImage>());

            if (SceneData.lightIndex < i)
            {
                var c = lights[i].color;
                c.a = 0;
                lights[i].color = c;
            }
        }


        Canvas.planeDistance = 0.14f;

        Fade.onFadeExit += Light;
    }

    private void Light()
    {
        StartCoroutine(UpdateLight());
    }

    private IEnumerator UpdateLight()
    {
        if (SceneData.lightIndex < 0)
        {
            foreach (RawImage item in lights)
            {
                var c = item.color;
                c.a = 1;
                item.color = c;
            }
            SceneData.lightIndex = lights.Count - 1;
        }
        yield return new WaitForSecondsRealtime(_fadeDelay);
        DisableLight();
    }

    public void DisableLight()
    {
        if (SceneData.lightIndex >= 0)
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

        var c = lights[SceneData.lightIndex].color;
        c.a = 1;
        lights[SceneData.lightIndex].color = c;
        
        time = 0.0f;
        float d = Canvas.planeDistance;
        while (time / fadeTime < 1) // Fade away single light
        {
            yield return new WaitForFixedUpdate();
            time += Time.fixedDeltaTime;
            c.a = Mathf.Lerp(1.0f, 0.0f, time / fadeTime);
            lights[SceneData.lightIndex].color = c;
        }
        c.a = 0;
        lights[SceneData.lightIndex].color = c;
        SceneData.lightIndex -= 1;



        yield return new WaitForSecondsRealtime(_timeUntilFadeAway); // delay



        time = 0.0f;
        while (time / fadeTime < 1) // Fade out CavasGroup
        {
            yield return new WaitForFixedUpdate();
            time += Time.fixedDeltaTime;
            GetComponent<CanvasGroup>().alpha = Mathf.Lerp(1.0f, 0.0f, time / fadeTime);
        }


        //transform.parent.GetComponent<Fade>().FadeToggle();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        Fade.onFadeExit -= Light;
    }
}
