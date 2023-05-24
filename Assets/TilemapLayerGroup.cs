using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TilemapLayerGroup : MonoBehaviour
{
    [SerializeField] string tmapNamePrefix = "";
    [SerializeField] public Area area;
    [SerializeField] public Room room;
    public List<TilemapLayer> layers = new();


    private void OnValidate()
    {
        if (room == null || area == null)
        { FindConnections(); }
        if (room != null)
        { tmapNamePrefix = room.roomName; }
        GetLayers();
        RenameObject();
    }

    /*
    bool GetRoom()
    {
        Transform t = transform.parent;
        for (int i = 0; i < 50; i++)
        {
            _room = t.GetComponent<Room>();
            if (t == transform.root || _room != null) break;
            t = t.parent;
        }
        return _room != null;
    }
    */
    void RenameObject()
    {
        gameObject.name = tmapNamePrefix == "" ? "Layers" : $"Layers({tmapNamePrefix})";
        for (int i = 0; i < layers.Count; i++)
        {
            layers[i].GetTilemapComponents(true);
            layers[i].gameObject.name = $"#{layers[i].transform.GetSiblingIndex()}{(tmapNamePrefix != "" ? $"({tmapNamePrefix})" : "")}[{layers[i].layerName}]"; 
        }
    }


    public void GetLayers()
    { 
        layers = new(GetComponentsInChildren<TilemapLayer>()); 
        foreach (TilemapLayer tl in layers)
        { 
            tl.group = this;
            if (room != null) tl.room = room;
            if (area != null) tl.area = area;
        }
    }
    public void ResetConnections()
    {
        area = null;
        room = null;
    }
    public void FindConnections(int lookoutDepth = 20)
    {
        Transform pt = transform;

        int i;
        Room r = null;
        Area a = null;

        for (i = 0; i < lookoutDepth; i++)
        {
            r = pt.GetComponent<Room>();
            a = pt.GetComponent<Area>();

            if (!room && r)
            { room = r; }
            if (a)
            { area = a; break; }
            if (pt == transform.root)
            { break; }
            pt = pt.parent;
        }
    }

}


#if UNITY_EDITOR

[CustomEditor(typeof(TilemapLayerGroup))]
class TilemapLayerGroupEditor : Editor
{
    TilemapLayerGroup instance { get => (TilemapLayerGroup)target; }
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

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Get Layers"))
        { instance.GetLayers(); }
        EditorGUILayout.EndHorizontal();
    }
}

#endif
