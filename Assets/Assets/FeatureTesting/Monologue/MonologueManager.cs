using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MonologueManager : MonoBehaviour
{
    public TextMeshProUGUI monologueText;
    public GameObject monologueTextContainer;

    private Queue<string> _textPerSlide;
    private Queue<float> _timeToDisplaySlide;

    void Start()
    {
        _textPerSlide = new Queue<string>();
        _timeToDisplaySlide = new Queue<float>();
    }

    public void StartMonologue(Monologue monologue)
    {
        //spela eventuell animation här
        monologueTextContainer.SetActive(true);

        _textPerSlide.Clear();
        _timeToDisplaySlide.Clear();     

        for (int i = 0; i < monologue.monologueText.Length; i++)
        {
            _textPerSlide.Enqueue(monologue.monologueText[i].textPerSlide);
            _timeToDisplaySlide.Enqueue(monologue.monologueText[i].timeToDisplaySlide);
        }

        StopAllCoroutines();
        StartCoroutine(DisplayNextSlide());
    }

    IEnumerator DisplayNextSlide()
    {
        if (_textPerSlide.Count > 0)
        {
            monologueText.text = _textPerSlide.Dequeue();

            yield return new WaitForSeconds(_timeToDisplaySlide.Dequeue());
            //rekursion Pog
            StartCoroutine(DisplayNextSlide());
        }
        else
        {
            EndMonologue();
            yield return null;
        }
    }

    void EndMonologue()
    {
        //spela eventuell animation här
        monologueTextContainer.SetActive(false);
    }
}
