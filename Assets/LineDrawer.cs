using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineDrawer : MonoBehaviour
{
    static LineDrawer instance;

    private void Awake()
    {
        instance = this;
    }

    public static LineRenderer CreateLineRenderer()
    {
        GameObject go = new GameObject("temp_linerend");
        go.transform.SetParent(instance.transform);
        return go.AddComponent<LineRenderer>();
    }

}
