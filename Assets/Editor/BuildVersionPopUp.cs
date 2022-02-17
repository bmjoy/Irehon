using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BuildVersionPopUp : EditorWindow
{
    private string currentText;
    private Action buttonAction;

    private void OnEnable()
    {
        GetWindow((typeof(BuildVersionPopUp)));
    }

    public void SetButtonAction(Action action)
    {
        buttonAction = action;
    }

    private void OnGUI()
    {
        GUILayout.Space(20);

        EditorGUILayout.LabelField("Enter build version", EditorStyles.wordWrappedLabel);

        GUILayout.Space(20);

        currentText = GUILayout.TextField(currentText);

        if (GUILayout.Button("Build"))
        {
            if (currentText != "Enter build version")
            {
                PlayerSettings.bundleVersion = currentText;
                Debug.Log($"Building {Application.version}");
                buttonAction();
                Close();
            }
            else
                currentText = "Enter valid version";
        }
    }
}
