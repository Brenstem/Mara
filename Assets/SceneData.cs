using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneData : MonoBehaviour
{
    public static SceneData _sceneData;
    public static bool gameStarted;
    public static int lightIndex;

    public static float horizontalSensitivity = 45f;
    public static float verticalSensitivity = 45f;
    public static float gamepadMultiplier = 5f;

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
