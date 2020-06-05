using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneData : MonoBehaviour
{
    public static bool gameStarted;
    public static int lightIndex;
    public static SceneData _sceneData;

    void Awake()
    {
        if (_sceneData != null && _sceneData != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            DontDestroyOnLoad(this);
            _sceneData = this;
        }
    }
}
