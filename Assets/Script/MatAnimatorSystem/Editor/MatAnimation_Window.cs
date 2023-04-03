using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MatAnimation_Window : EditorWindow
{

    [MenuItem("Window/MatAnimator/Material Animation")]
    public static void ShowWindow()
    {
        GetWindow<MatAnimation_Window>("Material Animation");
    }

    private void OnGUI()
    {

    }

}
