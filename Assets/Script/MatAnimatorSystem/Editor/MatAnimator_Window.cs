using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MatAnimator_Window : EditorWindow
{

    [MenuItem("Window/MatAnimator/Material Animator")]
    public static void ShowWindow()
    {
        GetWindow<MatAnimator_Window>("Material Animator");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Open Editor"))
        { GetWindow<MatAnimation_Window>("Material Animation"); }
    }

}
