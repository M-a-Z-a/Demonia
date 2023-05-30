using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class RoomSelector_Window : EditorWindow
{
    List<Area> _areas;
    List<Room> _rooms = new List<Room>();

    int tselect = 0;
    string[] toptions = { "Entity", "Stats", "Attributes" };

    [MenuItem("Window/Room Selector")]
    public static void ShowWindow()
    {
        GetWindow<RoomSelector_Window>("Room Selector");
    }


    private void OnGUI()
    {
        if (GUILayout.Button("Update Rooms"))
        {
            UpdateRooms();
        }

        if (_rooms.Count == 0) 
        { Debug.Log("Update Rooms?"); }
        else
        { 
            foreach (Room room in _rooms) 
            { RoomWrapper(room); }
        }
    }

    void RoomWrapper(Room room)
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("SELECT", GUILayout.Width(64)))
        { Selection.activeObject = room.gameObject; }
        GUILayout.Label($"{room.roomName}");
        EditorGUILayout.EndHorizontal();
    }

    void UpdateRooms()
    {
        Scene scene;
        _areas = new();
        _rooms = new();
        Area area;
        for (int i = 0; i < SceneManager.loadedSceneCount; i++)
        {
            scene = SceneManager.GetSceneAt(i);
            GameObject[] roots = scene.GetRootGameObjects();
            foreach (GameObject root in roots)
            {
                if (area = root.GetComponent<Area>())
                {
                    _areas.Add(area);
                    _rooms.AddRange(root.GetComponentsInChildren<Room>());
                }
            }
        }
        Debug.Log($"Fetced {_rooms.Count} rooms in {_areas.Count} areas");
    }

    private void OnSelectionChange()
    {
        //GameObject obj = Selection.activeGameObject;
        Repaint();
    }

}
