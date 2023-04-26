using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static Room;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Area : MonoBehaviour
{
    public static Area activeArea { get; protected set; }
    List<Room> rooms;
    [SerializeField] Color ambientColor = Color.white;
    [SerializeField] float ambientIntensity = 1;
    Transform objects, entities;
    Light2D ambientLight;

    private void OnValidate()
    {
        if (ambientLight || TryGetComponent<Light2D>(out ambientLight))
        { UpdateAmbient(); }
    }

    private void OnDrawGizmos()
    {
        Vector2 rdist = new Vector2(100f, 100f);
        Vector2 tpos = transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(tpos, transform.right * rdist);
        Gizmos.DrawRay(tpos, -transform.right * rdist);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(tpos, transform.up * rdist);
        Gizmos.DrawRay(tpos, -transform.up * rdist);
    }

    private void Awake()
    {
        activeArea = this;
        ambientLight = GetComponent<Light2D>();
    }
    private void Start()
    {
        SortEntities();
        //rooms = new(gameObject.GetComponentsInChildren<Room>(true));
        Debug.Log($"room count: {rooms.Count}");
        if (rooms.Count > 0)
        { 
            foreach (Room r in rooms)
            { r.Init(); r.Unload(); }
            //rooms[0].Activate(); 
            Player.pTransform.position = GameManager.Checkpoint.position;
            if (FindPlayerRoom(out Room rm))
            { rm.Activate(); }
        }
        UpdateAmbient();
    }

    public void Update()
    {
        if (Player.instance != null)
        {
            if (ActiveRoom && !ActiveRoom.PointInRoom(Player.pTransform.position))
            {
                bool pfound = false;
                for (int i = 0; i < ActiveRoom.connectedRooms.Count; i++)
                {
                    if (ActiveRoom.connectedRooms[i].roomWorldBounds.Contains(Player.pTransform.position))
                    { ActiveRoom.connectedRooms[i].Activate(); pfound = true; break; }
                }
                Room proom = ActiveRoom;
                if (pfound == false)
                {
                    if (FindPlayerRoom(out proom))
                    {
                        proom.Activate();
                        pfound = true;
                    }
                }
                Debug.Log(pfound ? $"player in room: {proom.gameObject.name}" : "player lost?");
            }
        }
    }

    public static bool FindPlayerRoom(out Room room)
    {
        room = null;
        Vector2 ppos = Player.pTransform.position;
        for (int i = 0; i < activeArea.rooms.Count; i++)
        {
            if (activeArea.rooms[i].roomWorldBounds.Contains(ppos))
            { room = activeArea.rooms[i]; return true; }
        }
        return false;
    }

    public virtual void FetchContainers()
    {
        if (entities == null)
        {
            entities = transform.Find("AreaEntities");
            if (entities == null)
            {
                GameObject go = new GameObject("AreaEntities");
                entities = go.transform; //Instantiate(new GameObject("Entities"), Vector3.zero, Quaternion.identity, transform).transform;
                entities.parent = transform;
            }
        }
        if (objects == null)
        {
            objects = transform.Find("AreaObjects");
            if (objects == null)
            {
                GameObject go = new GameObject("AreaObjects");
                objects = go.transform;//Instantiate(new GameObject("Entities"), Vector3.zero, Quaternion.identity, transform).transform;
                objects.parent = transform;
            }
        }
    }

    void UpdateAmbient()
    {
        ambientLight.color = ambientColor;
        ambientLight.intensity = ambientIntensity;
        //RenderSettings.ambientLight = ambientColor;
        //RenderSettings.ambientIntensity = ambientIntensity;
    }

    public void SortEntities()
    {
        FetchContainers();
        rooms = new(GetComponentsInChildren<Room>());
        List<Entity> ents = new(GetComponentsInChildren<Entity>());
        Entity ent;
        int ent_count;
        foreach (Room r in rooms)
        {
            r.FetchContainers();
            ent_count = ents.Count;
            for (int i = ent_count-1; i >= 0; i--)
            {
                ent = ents[i];
                if (ent.HasParentEntity())
                { ents.RemoveAt(i); continue; }
                if (r.PointInRoom(ent.transform.position))
                {
                    ent.transform.parent = r.entities;
                    ents.RemoveAt(i);
                    continue;
                }
                //ent.transform.parent = entities;
            }
        }
        foreach (Entity e in ents)
        { e.transform.parent = entities; }
    }
    public void SortObjects()
    {
        rooms = new(GetComponentsInChildren<Room>());
        List<Transform> tlist = new();
        foreach (Room r in rooms)
        {
            r.FetchContainers();
            foreach (Transform t in r.objects)
            {
                if (!r.PointInRoom(t.position))
                { tlist.Add(t); }
            }
        }
        int t_count = tlist.Count;
        foreach (Room r in rooms)
        {
            for (int i = t_count - 1; i >= 0; i--)
            {
                if (r.PointInRoom(tlist[i].position))
                {
                    tlist[i].parent = r.objects;
                    tlist.RemoveAt(i);
                    continue;
                }
            }
        }
        foreach (Transform t in tlist)
        { t.parent = objects; }
    }
}



#if UNITY_EDITOR
[CustomEditor(typeof(Area))]
public class AreaEditor : Editor
{
    Area instance;
    private void OnEnable()
    {
        instance = (Area)target;
    }

    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        DrawDefaultInspector();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Sort Entities"))
        { instance.SortEntities(); }
        if (GUILayout.Button("Sort Objects"))
        { instance.SortObjects(); }
        EditorGUILayout.EndHorizontal();
    }
}
#endif