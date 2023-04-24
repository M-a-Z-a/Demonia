using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class Room : MonoBehaviour
{
    public enum RoomState { Disabled = 0, Enabled, Active };
    public static Room ActiveRoom { get; protected set; }

    [SerializeField] public List<Room> connectedRooms;
    [SerializeField] public Rect roomBounds;
    public Rect roomWorldBounds;

    public bool firstLoad = true;

    public RoomState roomState = RoomState.Disabled;
    
    public Transform entities, objects;
    public List<Entity> entList = new();

    bool firstInit = true;

    public void SetActiveRoom()
    { ActiveRoom = this; }

    protected virtual void OnDrawGizmos()
    {
        Vector3 rposmin = transform.position + (Vector3)roomBounds.min;
        Vector3 rposmax = transform.position + (Vector3)roomBounds.max;
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(rposmin, new Vector3(roomBounds.width, 0, 0));
        Gizmos.DrawRay(rposmin, new Vector3(0, roomBounds.height, 0));
        Gizmos.DrawRay(rposmax, new Vector3(-roomBounds.width, 0, 0));
        Gizmos.DrawRay(rposmax, new Vector3(0, -roomBounds.height, 0));

        Vector2 rpos = roomWorldBounds.center;
        Vector2 dist;
        foreach (Room r in connectedRooms)
        {
            dist = r.roomWorldBounds.center - roomWorldBounds.center;
            Utility.DrawGizmoArrow(rpos, dist*0.6f, 1f);
        }
    }
    protected virtual void OnDrawGizmosSelected()
    {
        Vector3 rposmin = transform.position + (Vector3)roomBounds.min - new Vector3(0.5f, 0.5f, 0f);
        Vector3 rposmax = transform.position + (Vector3)roomBounds.max + new Vector3(0.5f, 0.5f, 0f);
        Gizmos.color = Color.white;
        Gizmos.DrawRay(rposmin, new Vector3(roomBounds.width + 1f, 0, 0));
        Gizmos.DrawRay(rposmin, new Vector3(0, roomBounds.height + 1f, 0));
        Gizmos.DrawRay(rposmax, new Vector3(-roomBounds.width-1f, 0, 0));
        Gizmos.DrawRay(rposmax, new Vector3(0, -roomBounds.height-1f, 0));

        Vector2 rpos = roomWorldBounds.center;
        Vector2 dist;
        foreach (Room r in connectedRooms)
        {
            dist = r.roomWorldBounds.center - roomWorldBounds.center;
            Utility.DrawGizmoArrow(rpos, dist * 0.6f, 1f);
        }
    }

    protected virtual void Awake()
    {
        GetRoomWorldBounds();
        //FetchObjects();
    }
    protected virtual void Start()
    {
        //FetchContents();
        //FetchContents();
        Init();
    }

    private void OnValidate()
    {
        GetRoomWorldBounds();
    }

    public void Init()
    {
        if (!firstInit) return;
        //GetRoomWorldBounds();
        firstInit = false;
        FetchContents();
    }


    public virtual void SetEntityStates(bool state)
    {
        for (int i = 0; i < entList.Count; i++)
        { entList[i].gameObject.SetActive(state); }
    }

    protected virtual void GetRoomWorldBounds()
    { roomWorldBounds = new Rect(roomBounds.position + (Vector2)transform.position, roomBounds.size); }

    public virtual void FetchContainers()
    {
        if (entities == null)
        { entities = transform.Find("Entities"); 
            if (entities == null)
            {
                GameObject go = new GameObject("Entities");
                entities = go.transform; //Instantiate(new GameObject("Entities"), Vector3.zero, Quaternion.identity, transform).transform;
                entities.parent = transform;
            }
        }
        if (objects == null)
        {
            objects = transform.Find("Objects");
            if (objects == null)
            {
                GameObject go = new GameObject("Objects");
                objects = go.transform;//Instantiate(new GameObject("Entities"), Vector3.zero, Quaternion.identity, transform).transform;
                objects.parent = transform;
            }
        }
    }
    public virtual void FetchContents()
    {
        FetchContainers();
        Entity[] ents = entities.GetComponentsInChildren<Entity>();
        entList = new();
        foreach (Entity ent in ents)
        { 
            if (ent.HasParentEntity(stop_at: transform))
            { continue; }
            entList.Add(ent);
        }
    }


    public bool PointInRoom(Vector2 point)
    { return roomWorldBounds.Contains(point); }



    public virtual bool Activate()
    {
        roomState = RoomState.Active;
        firstLoad = false;
        if (ActiveRoom)
        {
            List<Room> rlist = new(connectedRooms);
            rlist.Add(this);
            ActiveRoom.UnloadAdjacentRooms(rlist);
            ActiveRoom.Deactivate();
        }
        ActiveRoom = this;
        
        Debug.Log($"Room: {gameObject.name} activated");
        Load();
        LoadAdjacentRooms();
        SetEntityStates(true);

        Vector3 ppos = Player.pTransform.position;
        ppos.x = Mathf.Clamp(ppos.x, roomWorldBounds.xMin + 2, roomWorldBounds.xMax - 2);
        ppos.y = Mathf.Clamp(ppos.y, roomWorldBounds.yMin + 2, roomWorldBounds.yMax - 2);
        GameManager.Checkpoint.position = ppos;

        return true;
    }

    public virtual bool Deactivate()
    {
        roomState = RoomState.Enabled;
        Debug.Log($"Room: {gameObject.name} deactivated");
        SetEntityStates(false);
        return true;
    }

    public virtual bool Load()
    {
        roomState = RoomState.Enabled;
        gameObject.SetActive(true);
        SetEntityStates(false);
        Debug.Log($"Room: {gameObject.name} loaded");
        return true;
    }

    public virtual bool Unload()
    {
        Debug.Log($"Room: {gameObject.name} unloaded");
        Deactivate();
        roomState = RoomState.Disabled;
        gameObject.SetActive(false);
        return true;
    }


    public void LoadAdjacentRooms()
    {
        for (int i = 0; i < connectedRooms.Count; i++)
        {
            if (connectedRooms[i] == ActiveRoom) continue;
            connectedRooms[i].Load();
        }
    }
    public void LoadAdjacentRooms(List<Room> exclude)
    {
        for (int i = 0; i < connectedRooms.Count; i++)
        {
            if (connectedRooms[i] == ActiveRoom || exclude.Contains(connectedRooms[i])) continue;
            connectedRooms[i].Load();
        }
    }

    public void UnloadAdjacentRooms()
    {
        for (int i = 0; i < connectedRooms.Count; i++)
        {
            if (connectedRooms[i] == ActiveRoom) continue;
            connectedRooms[i].Unload();
        }
    }
    public void UnloadAdjacentRooms(List<Room> exclude)
    {
        for (int i = 0; i < connectedRooms.Count; i++)
        {
            if (connectedRooms[i] == ActiveRoom || exclude.Contains(connectedRooms[i])) continue;
            connectedRooms[i].Unload();
        }
    }

}



#if UNITY_EDITOR
[CustomEditor(typeof(Room))]
public class RoomEditor : Editor
{
    Room instance;
    static Vector2 snapOffset = new Vector2(0.5f, 0.5f);
    static bool isOffset = true;
    static int clampX = 0, clampY = 0;
    static string[] s_clampX = { "Clamp X Left", "Clamp X Center", "Clamp X Right" }, 
        s_clampY = { "Clamp Y Bottom", "Clamp Y Center", "Clamp Y Top"};
    private void OnEnable()
    {
        instance = (Room)target;
    }


    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        DrawDefaultInspector();


        snapOffset = EditorGUILayout.Vector2Field("Snap offset", snapOffset);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(s_clampX[clampX]))
        { clampX = (clampX + 1) % s_clampX.Length; }
        if (GUILayout.Button(s_clampY[clampY]))
        { clampY = (clampY + 1) % s_clampY.Length; }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button(isOffset?"Nudge Offset":"Nudge Scale"))
        { isOffset = !isOffset; }
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("TL"))
        { Nudge(-0.5f, 0.5f); }
        if (GUILayout.Button("LEFT"))
        { Nudge(x: -0.5f); }
        if (GUILayout.Button("BL"))
        { Nudge(-0.5f, -0.5f); }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("UP"))
        { Nudge(y: 0.5f); }
        if (GUILayout.Button($"SNAP"))
        { Snap(); }
        if (GUILayout.Button("DOWN"))
        { Nudge(y: -0.5f); }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        if (GUILayout.Button("TR"))
        { Nudge(0.5f, 0.5f); }
        if (GUILayout.Button("RIGHT"))
        { Nudge(x: 0.5f); }
        if (GUILayout.Button("BR"))
        { Nudge(0.5f, -0.5f); }
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    void Snap()
    {
        if (isOffset)
        { 
            Vector2 p = instance.roomBounds.position; 
            instance.roomBounds.position = new Vector2(Mathf.Floor(p.x), Mathf.Floor(p.y))+snapOffset;
            SceneView.RepaintAll();
            return;
        }
        Vector2 s = instance.roomBounds.size;
        instance.roomBounds.size = new Vector2(Mathf.Floor(s.x), Mathf.Floor(s.y))+snapOffset;
    }
    void Nudge(float x = 0, float y = 0)
    {
        if (isOffset)
        {
            instance.roomBounds.position += new Vector2(x, y); 
            SceneView.RepaintAll();
            return;
        }
        if (clampX == 2) x = -x;
        if (clampY == 2) y = -y;
        Vector2 vec = new Vector2(clampX == 1 ? x * 2 : x, clampY == 1 ? y * 2 : y);
        Vector2 posComp = new Vector2(clampX > 0 ? -x : 0, clampY > 0 ? -y : 0);
        instance.roomBounds.size += vec;
        instance.roomBounds.position += posComp;
        SceneView.RepaintAll();
    }
}
#endif