using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Monologue
{
    [System.Serializable]
    public struct MonologueText
    {
        public float timeToDisplaySlide;
        [Tooltip("SKRIV MAX MÄNGD TEXT PER SLIDE HÄR (alex eller whaterver)")]
        [TextArea(3, 10)]
        public string textPerSlide;
    }

    [SerializeField] public string name;
    public MonologueText[] monologueText;
}
