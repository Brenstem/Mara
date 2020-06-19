using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMenuManagerTest : MonoBehaviour
{
    [SerializeField] private float _fadeTime;
    [SerializeField] private GameObject _canvas;

    private float _timer;
    private PlayerRevamp _playerRevamp;
    private bool _playing = SceneData.gameStarted;

    private void Awake()
    {
        _playerRevamp = GlobalState.state.Player;

        if (SceneData.gameStarted)
        {
            _canvas.SetActive(false);
            _canvas.GetComponent<CanvasGroup>().alpha = 0;
        }
    }

    public void ToggleStartGame()
    {
        StopAllCoroutines();

        _timer = 0f;

        if (_playing) // Fade in
        {
            _playing = false;

            _canvas.SetActive(true);

            GlobalState.state.LockCursor = false;

            StartCoroutine(FadeIn());
        }
        else
        {
            _playerRevamp.cameraAnimator.SetTrigger("GameStarted");

            GlobalState.state.LockCursor = true;
            GlobalState.state.GameStarted = true;

            _playing = true;
            Time.timeScale = 1;
            StartCoroutine(FadeOut());
        }
    }

    private IEnumerator FadeOut()
    {
        if (_canvas.GetComponent<CanvasGroup>().alpha > 0f)
        {
            _timer += Time.fixedDeltaTime;
            _canvas.GetComponent<CanvasGroup>().alpha = 1 - _timer / _fadeTime;
            yield return new WaitForFixedUpdate();
            StartCoroutine(FadeOut());
        }
        else
        {
            _canvas.SetActive(false);
            GlobalState.state.Player.EnabledControls = true;
        }
    }

    private IEnumerator FadeIn()
    {
        if (_canvas.GetComponent<CanvasGroup>().alpha < 1f)
        {
            _timer += Time.fixedDeltaTime;
            _canvas.GetComponent<CanvasGroup>().alpha = _timer / _fadeTime;

            yield return new WaitForFixedUpdate();
            StartCoroutine(FadeIn());
        }
        else
        {
            GlobalState.state.Player.EnabledControls = false;
            Time.timeScale = 0;
        }
    }
}
