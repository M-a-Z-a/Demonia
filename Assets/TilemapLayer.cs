using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TilemapLayer : MonoBehaviour
{

    [SerializeField] Room _room = null;
    [SerializeField] Area _area = null;
    [SerializeField] TilemapLayerGroup _group = null;

    public Room room { get => _room; }
    public Area area { get => _area; }
    public TilemapLayerGroup group { get => _group; }


    private void Awake()
    {
        FindConnections();
    }

    public void ResetConnections()
    {
        _room = null;
        _area = null;
        _group = null;
    }
    public void FindConnections(int lookoutDepth = 20)
    {
        Transform pt = transform;

        int i; 
        Room r = null;
        Area a = null;
        TilemapLayerGroup g = null;

        Debug.Log($"Finding connections in {transform.name}...");
        for (i = 0; i < lookoutDepth; i++)
        {
            r = pt.GetComponent<Room>();
            a = pt.GetComponent<Area>();
            g = pt.GetComponent<TilemapLayerGroup>();

            if (pt == transform.root)
            { break; }
            if (!_room && r)
            { _room = r; }
            if (!_group && g)
            { _group = g; }
            if (a)
            { _area = a; break; }
            pt = pt.parent;
        }
        Debug.Log($"Results: Area/Room/Group = {(_area ? _area.name : "None")}/{(_room ? _room.name : "None")}/{(_group ? _group.name : "None")}");
    }

}
#if UNITY_EDITOR

[CustomEditor(typeof(TilemapLayer))]
class TilemapLayerEditor : Editor
{
    TilemapLayer instance { get => (TilemapLayer)target; }
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        //base.OnInspectorGUI();

        if (GUILayout.Button("Find connections"))
        { instance.FindConnections(); }
        if (GUILayout.Button("Reset connections"))
        { instance.ResetConnections(); }
    }
}

#endif
