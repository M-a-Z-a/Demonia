using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class RuleTileGeneratorSave_Window : EditorWindow
{
    RuleTileGenerator_Window rtgw;

    private void OnGUI()
    {
        if (!rtgw) 
        { GUILayout.Label("RuleTileGenerator_Window not valid"); return; }

        GUILayout.BeginHorizontal();
        GUILayout.Label("Save path:");
        string savepath = GUILayout.TextField("");
        GUILayout.EndHorizontal();

    }

    public void SetRTGW(RuleTileGenerator_Window rtg_window)
    {
        rtgw = rtg_window;
        Repaint();
    }

}
