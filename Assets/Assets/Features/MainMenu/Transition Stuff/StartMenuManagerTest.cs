using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMenuManagerTest : MonoBehaviour
{
    [SerializeField] private float _fadeTime;
    private float _timer;

    public void StartGame()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        if (this.GetComponent<CanvasGroup>().alpha > 0f)
        {
            _timer += Time.fixedDeltaTime;
            this.GetComponent<CanvasGroup>().alpha = 1 - _timer / _fadeTime;
            yield return new WaitForFixedUpdate();
            StartCoroutine(FadeOut());
        }
    }
}
