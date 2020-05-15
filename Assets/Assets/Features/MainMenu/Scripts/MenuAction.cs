﻿using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public void SetFullscreenMode(int index)
    {
        Screen.fullScreenMode = (FullScreenMode)index;
        print(Screen.fullScreenMode);
        //QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
         Application.Quit();
        #endif
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
