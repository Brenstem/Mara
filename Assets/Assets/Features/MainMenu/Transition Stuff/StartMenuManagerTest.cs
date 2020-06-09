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
    }

    public void ToggleStartGame()
    {
        StopAllCoroutines();

        _timer = 0f;

        if (_playing) // Fade in
        {
            _playing = false;

            //print("before: " + _canvas.activeSelf);

            _canvas.SetActive(true);

            //print("after: " + _canvas.activeSelf);

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
            print("meme");
            GlobalState.state.Player.enabled = false;
            Time.timeScale = 0;
        }
    }
}
