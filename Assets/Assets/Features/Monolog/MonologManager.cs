using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MonologManager : MonoBehaviour
{
    public Text monologText;
    public GameObject monologBackground;

    private Queue<string> _textPerSlide;
    private Queue<float> _timeToDisplaySlide;

    void Start()
    {
        _textPerSlide = new Queue<string>();
        _timeToDisplaySlide = new Queue<float>();
    }

    public void StartMonolog(Monolog monolog)
    {
        //spela eventuell animation här
        monologBackground.SetActive(true);

        _textPerSlide.Clear();
        _timeToDisplaySlide.Clear();     

        for (int i = 0; i < monolog.monologText.Length; i++)
        {
            _textPerSlide.Enqueue(monolog.monologText[i].textPerSlide);
            _timeToDisplaySlide.Enqueue(monolog.monologText[i].timeToDisplaySlide);
        }

        StopAllCoroutines();
        StartCoroutine(DisplayNextSlide());
    }

    IEnumerator DisplayNextSlide()
    {
        if (_textPerSlide.Count > 0)
        {
            monologText.text = _textPerSlide.Dequeue();

            yield return new WaitForSeconds(_timeToDisplaySlide.Dequeue());
            //rekursion Pog
            StartCoroutine(DisplayNextSlide());
        }
        else
        {
            EndMonolog();
            yield return null;
        }
    }

    void EndMonolog()
    {
        //spela eventuell animation här
        monologBackground.SetActive(false);
    }
}
