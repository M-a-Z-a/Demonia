using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineManager : MonoBehaviour
{
    static LineManager instance;

    private void Awake()
    {
        instance = this;

        float t;
        for (int i = 0; i < 5; i++)
        {
            t = (float)i / 4;
            //Debug.Log($"[{i}]SineVector({t}) => {Utility.SineVector(t)}");
        }
    }

    public static LineRenderer CreateLineRenderer()
    {
        GameObject go = new GameObject("temp_linerend");
        go.transform.SetParent(instance.transform);
        return go.AddComponent<LineRenderer>();
    }


}
