using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif


public class Room : MonoBehaviour
{
    public static UnityEvent<Room, Room> onAnyRoomActivated = new();

    public enum RoomState { Disabled = 0, Enabled, Active };
    public static Room ActiveRoom { get; protected set; }

    public string roomName { get => _roomName; protected set => UpdateName(value); }
    [SerializeField] string _roomName = "";
    [SerializeField] public List<Room> connectedRooms = new();
    //[SerializeField] public List<RoomConnection> roomConnections;
    [SerializeField] public Rect roomBounds;

    [SerializeField] bool overrideAreaAmbient = false;
    [SerializeField] Color roomAmbientColor = Color.black;
    [SerializeField] float roomAmbientIntensity = 1f;
    [SerializeField] bool darkRoom = false;
    public bool isDarkRoom { get => darkRoom; }

    [SerializeField] bool overrideAreaMusic = false;
    [SerializeField] AudioClip roomMusic;
    [SerializeField] float roomMusicVolume = 1f;

    public Rect roomWorldBounds;
    public bool firstLoad = true;

    public RoomState roomState = RoomState.Disabled;
    
    public Transform entities, objects;
    public List<Entity> entList = new();

    bool firstInit = true;

    

    public void GetAmbientLight(out Color light_color, out float light_intensity)
    {
        if (!overrideAreaAmbient && transform.parent.TryGetComponent<Area>(out Area area))
        { area.GetAmbientLight(out light_color, out light_intensity); return; }
        light_color = roomAmbientColor; light_intensity = roomAmbientIntensity;
    }

    public void SetActiveRoom()
    { ActiveRoom = this; }

#if UNITY_EDITOR
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

        Handles.Label(new Vector3(rposmin.x + 1, rposmax.y + 1.5f, 0), $"{name}");

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
#endif

    protected virtual void Awake()
    {
        GetRoomWorldBounds();
    }
    protected virtual void Start()
    {
        Init();
    }

    private void OnValidate()
    {
        UpdateName();
        GetRoomWorldBounds();
    }

    
    void UpdateName(string n = null)
    { 
        if (n != null)
        { _roomName = n; }
        if (_roomName != "")
        { gameObject.name = $"Room({_roomName})"; }
    }

    public void Init()
    {
        if (!firstInit) return;
        //GetRoomWorldBounds();
        firstInit = false;
        FetchContents();
    }

    public static int ConnectRooms(Room a, Room b, bool a2b_enabled = true, bool b2a_enabled = true)
    {
        int rcon = 0;
        if (!a.connectedRooms.Contains(b)) { a.connectedRooms.Add(b); rcon += 1; }
        if (!b.connectedRooms.Contains(a)) { b.connectedRooms.Add(a); rcon += 2; }
        return rcon;
    }
    public static int DisconnectRooms(Room a, Room b)
    {
        int rcon = 0;
        if (a.connectedRooms.Remove(b)) rcon += 1;
        if (b.connectedRooms.Remove(a)) rcon += 2;
        return rcon;
    }

    public virtual void SetEntityStates(bool state)
    {
        for (int i = 0; i < entList.Count; i++)
        { entList[i].gameObject.SetActive(state); }
    }
    public virtual void SetObjectStates(bool state)
    {
        objects.gameObject.SetActive(state);
    }

    public virtual void GetRoomWorldBounds()
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
        Room laroom = ActiveRoom;
        ActiveRoom = this;
        
        //Debug.Log($"Room: {gameObject.name} activated");
        Load();
        LoadAdjacentRooms();
        SetObjectStates(true);
        SetEntityStates(true);

        if (overrideAreaAmbient)
        { Area.activeArea.SetAmbientLight(roomAmbientColor, roomAmbientIntensity); }
        else
        { Area.activeArea.ResetAmbientLight(); }
        if (overrideAreaMusic && roomMusic != null)
        { Area.activeArea.SetMusic(roomMusic, roomMusicVolume); }
        else
        { Area.activeArea.ResetMusic(); }

        Vector3 ppos = Player.pTransform.position;
        ppos.x = Mathf.Clamp(ppos.x, roomWorldBounds.xMin + 2, roomWorldBounds.xMax - 2);
        ppos.y = Mathf.Clamp(ppos.y, roomWorldBounds.yMin + 2, roomWorldBounds.yMax - 2);
        GameManager.Checkpoint.position = ppos;

        onAnyRoomActivated.Invoke(ActiveRoom, laroom);

        return true;
    }

    public virtual bool Deactivate()
    {
        roomState = RoomState.Enabled;
        //Debug.Log($"Room: {gameObject.name} deactivated");
        SetObjectStates(false);
        SetEntityStates(false);
        return true;
    }

    public virtual bool Load()
    {
        roomState = RoomState.Enabled;
        gameObject.SetActive(true);
        SetObjectStates(false);
        SetEntityStates(false);
        //Debug.Log($"Room: {gameObject.name} loaded");
        return true;
    }

    public virtual bool Unload()
    {
        //Debug.Log($"Room: {gameObject.name} unloaded");
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

    [System.Serializable]
    public class RoomConnection
    {
        public Room room;
        public bool enabled;
        public RoomConnection(Room room, bool enabled = true)
        { this.room = room; this.enabled = enabled; }
    }

}



#if UNITY_EDITOR
[CustomEditor(typeof(Room))]
[CanEditMultipleObjects]
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
        //EditorGUILayout.BeginHorizontal();
        //EditorGUILayout.LabelField("Rename:", GUILayout.Width(64));
        //instance.name = EditorGUILayout.DelayedTextField(instance.name);
        //EditorGUILayout.EndHorizontal();

        base.OnInspectorGUI();
        //DrawDefaultInspector();

        int s_count;
        if ((s_count = GetSelection(out Room[] s_rooms)) > 1)
        {
            List<string> rnames = new();
            foreach (Room r in s_rooms)
            { rnames.Add(r.name); }
            EditorGUILayout.LabelField($"Selected rooms [{s_count}]:");
            EditorGUILayout.LabelField($"{string.Join(',', rnames)}", EditorStyles.toolbarTextField);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Connect"))
            { 
                int rcon = ConnectRooms(s_rooms);
                Debug.Log($"New Room connections: {rcon}");
                SceneView.RepaintAll();
            }
            if (GUILayout.Button("Disconnect"))
            { 
                int rcon = DisconnectRooms(s_rooms);
                Debug.Log($"Removed Room connections: {rcon}");
                SceneView.RepaintAll();
            }
            EditorGUILayout.EndHorizontal();
        }


        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Area tools", EditorStyles.boldLabel);

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

    int ConnectRooms(Room[] rooms) 
    {
        int rconnections = 0;
        List<Room> rms = new(rooms);
        for (int i = rms.Count-1; i >= 0; i--)
        {
            if (i == 0) break;
            for (int r = i-1; r >= 0; r--)
            {
                int rcon = Room.ConnectRooms(rms[i], rms[r]);
                switch (rcon)
                {
                    case 1: case 2: rconnections++; break;
                    case 3: rconnections += 2; break;
                }
            }
            rms.RemoveAt(i);
        }
        return rconnections;
    }
    int DisconnectRooms(Room[] rooms)
    {
        int rconnections = 0;
        List<Room> rms = new(rooms);
        for (int i = rms.Count - 1; i >= 0; i--)
        {
            if (i == 0) break;
            for (int r = i - 1; r >= 0; r--)
            {
                int rcon = Room.DisconnectRooms(rms[i], rms[r]); switch (rcon)
                {
                    case 1: case 2: rconnections++; break;
                    case 3: rconnections += 2; break;
                }
            }
            rms.RemoveAt(i);
        }
        return rconnections;
    }


    int GetSelection(out Room[] rooms_out)
    {
        rooms_out = Selection.GetFiltered<Room>(SelectionMode.TopLevel);
        return rooms_out.Length;
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
        instance.GetRoomWorldBounds();
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
        instance.GetRoomWorldBounds();
        SceneView.RepaintAll();
    }
}
#endif