using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMenuManagerTest : MonoBehaviour
{
    [SerializeField] private float _fadeSpeed;

    public void StartGame()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        if (this.GetComponent<CanvasGroup>().alpha > 0f)
        {
            this.GetComponent<CanvasGroup>().alpha -= Time.fixedDeltaTime * _fadeSpeed;
            yield return new WaitForFixedUpdate();
            StartCoroutine(FadeOut());
        }
    }
}
