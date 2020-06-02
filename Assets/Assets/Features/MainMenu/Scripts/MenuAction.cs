using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class MenuAction : MonoBehaviour
{
    public enum MenuActionType
    {
        SceneChange,
        Any,
    }
    public MenuActionType type;
    public bool flag;

    public int sceneIndex;
    public void ChangeScene()
    {
        /*if(!File.Exists(Application.persistentDataPath + "/controls.data"))
        {
            OptionData d = new OptionData();
        }*/
        SceneManager.LoadScene(sceneIndex);
    }

    public void DropdownTest()
    {
        print("Selected option value: " + GetComponent<UnityEngine.UI.Dropdown>().value);
    }

    public void SetQuality(int index)
    {
        print(index);
        //QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetFullscreenMode(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        print(isFullscreen);
        //QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void SetLanguage(int language)
    {
        GlobalState.state.language = (GlobalState.LanguageEnum)language;
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
         Application.Quit();
        #endif
    }

    public void SetResolution(int index)
    {
        var rr = Screen.resolutions[index];
        Screen.SetResolution(rr.width, rr.height, Screen.fullScreenMode, rr.refreshRate);
    }
}

/*
[CustomEditor(typeof(MenuAction))]
public class MyScriptEditor : Editor
{
override public void OnInspectorGUI()
{
    var m = target as MenuAction;

    m.flag = GUILayout.Toggle(m.flag, "Flag");

    if (m.flag)
        m.sceneIndex = EditorGUILayout.IntSlider("I field:", m.sceneIndex, 1, 100);

}
}
*/
