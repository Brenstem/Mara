using UnityEngine;
using System.Collections;

// DW
// Modified version of this script: https://gist.github.com/hurr1star/c1bdeebcf56a3d94d74bcabe6aec214f
public class FPSDisplay : MonoBehaviour {
    [SerializeField] private bool displayFpsCounter;
    [SerializeField] private int targetFPS;

    public static bool DisplayFPS;
    public static int TargetFPS {
        get { return Application.targetFrameRate; }
        set { Application.targetFrameRate = value; }
    }

    private float deltaTime = 0.0f;

    private void Start() {
        //QualitySettings.vSyncCount = 0;
        DisplayFPS = displayFpsCounter;
        TargetFPS = targetFPS;
    }

    void Update() {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    void OnGUI() {
        if (DisplayFPS) {
            int w = Screen.width, h = Screen.height;

            GUIStyle style = new GUIStyle();

            Rect rect = new Rect(0, 0, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 100;
            style.normal.textColor = new Color(0.0f, 0.0f, 0.5f, 1.0f);
            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;
            string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
            GUI.Label(rect, text, style);
        }
    }

    private void OnValidate() {
        DisplayFPS = displayFpsCounter;
        TargetFPS = targetFPS;
    }
}