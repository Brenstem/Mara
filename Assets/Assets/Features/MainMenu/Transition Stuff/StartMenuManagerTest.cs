using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMenuManagerTest : MonoBehaviour
{
    [SerializeField] private float _fadeTime;

    private float _timer;

    private bool _playing = SceneData.gameStarted;

    public void ToggleStartGame()
    {
        // StopAllCoroutines();

        if (_playing) // Fade in
        {
            _playing = false;
            Time.timeScale = 0;

            print("before: " + this.GetComponent<Canvas>().enabled);

            this.GetComponent<Canvas>().enabled = true;

            print("after: " + this.GetComponent<Canvas>().enabled);

            Time.timeScale = 0;
            GlobalState.state.Player.EnabledControls = false;
            GlobalState.state.LockCursor = false;

            StartCoroutine(FadeIn());
        }
        else
        {
            _playing = true;
            Time.timeScale = 1;
            StartCoroutine(FadeOut());
        }
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
        else
        {
            this.GetComponent<Canvas>().enabled = false;
            GlobalState.state.Player.EnabledControls = true;
        }
    }

    private IEnumerator FadeIn()
    {
        if (this.GetComponent<CanvasGroup>().alpha < 1f)
        {
            _timer += Time.fixedDeltaTime;
            this.GetComponent<CanvasGroup>().alpha = _timer / _fadeTime;
            yield return new WaitForFixedUpdate();
            StartCoroutine(FadeIn());
        }
    }
}
