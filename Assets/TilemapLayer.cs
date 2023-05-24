using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(TilemapRenderer), typeof(Tilemap))]
public class TilemapLayer : MonoBehaviour
{
    public string layerName = "";
    [SerializeField] public Room room = null;
    [SerializeField] public Area area = null;
    [SerializeField] public TilemapLayerGroup group = null;
    [SerializeField] public TilemapRenderer rend; 
    [SerializeField] public Tilemap tmap;

    public void GetTilemapComponents(bool add_components = false)
    {
        rend = GetComponent<TilemapRenderer>();
        tmap = GetComponent<Tilemap>();
        if (!add_components) return;
        if (rend == null) rend = gameObject.AddComponent<TilemapRenderer>();
        if (tmap == null) tmap = gameObject.AddComponent<Tilemap>();
    }

    private void Awake()
    {
        FindConnections();
    }

    public void ResetConnections()
    {
        room = null;
        area = null;
        group = null;
    }
    public void FindConnections(int lookoutDepth = 20)
    {
        Transform pt = transform;

        if (room != null && area != null && group != null) return;

        int i; 
        Room r = null;
        Area a = null;
        TilemapLayerGroup g = null;

        //Debug.Log($"Finding connections in {transform.name}...");
        for (i = 0; i < lookoutDepth; i++)
        {
            r = pt.GetComponent<Room>();
            a = pt.GetComponent<Area>();
            g = pt.GetComponent<TilemapLayerGroup>();

            if (!room && r)
            { room = r; }
            if (!group && g)
            { group = g; }
            if (a)
            { area = a; break; }
            if (pt == transform.root)
            { break; }
            pt = pt.parent;
        }
        //Debug.Log($"Results: Area/Room/Group = {(_area ? _area.name : "None")}/{(_room ? _room.name : "None")}/{(_group ? _group.name : "None")}");
    }

}
#if UNITY_EDITOR

[CustomEditor(typeof(TilemapLayer))]
class TilemapLayerEditor : Editor
{
    TilemapLayer instance { get => (TilemapLayer)target; }
    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();
        base.OnInspectorGUI();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Find connections"))
        { instance.FindConnections(); }
        if (GUILayout.Button("Reset connections"))
        { instance.ResetConnections(); }
        EditorGUILayout.EndHorizontal();
    }
}

#endif
