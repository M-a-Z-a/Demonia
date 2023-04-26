using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


public class ObjectEditorExtension : EditorWindow
{
    AreaContainer[] areas;

    GameObject[] selection = new GameObject[0];
    Transform[] transforms = new Transform[0];
    int sCount { get => selection.Length; }

    [MenuItem("Window/Object Editor")]
    public static void ShowWindow()
    {
        GetWindow<ObjectEditorExtension>("Object Editor").OnSelectionChange();
    }

    private void OnGUI()
    {
        if (sCount == 0)
        { EditorGUILayout.LabelField("Select object(s)..."); return; }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Center selection to view"))
        {
            Vector3 pos = Vector2.zero;
            foreach (Transform t in transforms)
            {
                pos += t.position;
            }
            pos /= sCount;
            pos.y = 0;
            Camera sceneCam = SceneView.currentDrawingSceneView.camera;
            //Vector3 campos = sceneCam.ScreenToWorldPoint(sceneCam.pixelWidth/2, sceneCam.pixelHeight);

            foreach (Transform t in transforms)
            { 
                t.position = pos + t.position.Add(-pos.x, -pos.y); 
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Get Rooms"))
        { GetRooms(); }
        EditorGUILayout.EndHorizontal();
    }

    // targetGameObject.transform.position = Camera.main.ScreenToWorldPoint( Vector3(Screen.width/2, Screen.height/2, Camera.main.nearClipPlane) )

    void GetRooms()
    {
        GetAreas();
        int rcount = 0;
        for (int i = 0; i < areas.Length; i++)
        { areas[i].rooms = areas[i].area.GetComponentsInChildren<Room>(); rcount += areas[i].rooms.Length; }
        Debug.Log($"Found {rcount} Rooms in {areas.Length} Areas");
    }
    void GetAreas()
    { 
        Area[] _areas = GameObject.FindObjectsOfType<Area>();
        areas = new AreaContainer[_areas.Length];
        for (int i = 0; i < _areas.Length; i++)
        { areas[i] = new AreaContainer(_areas[i]); }
    }

    private void OnSelectionChange()
    {
        selection = Selection.gameObjects;
        transforms = Selection.transforms;
        Repaint();
    }

    public struct AreaContainer
    {
        public Area area;
        public Room[] rooms;
        public AreaContainer(Area area, params Room[] rooms)
        { this.area = area; this.rooms = rooms; }
    }

}
