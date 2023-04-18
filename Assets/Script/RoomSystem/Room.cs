using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public enum RoomState { Disabled = 0, Enabled, Active };
    public static Room ActiveRoom { get; protected set; }

    [SerializeField] public List<Room> connectedRooms;
    [SerializeField] public Rect roomBounds;
    public Rect roomWorldBounds;

    public bool firstLoad = true;

    public RoomState roomState = RoomState.Disabled;
    
    public Transform objects;
    public List<Entity> entities = new();

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

    protected virtual void Awake()
    {
        GetRoomWorldBounds();
        FetchEntities();
        //FetchEntities();
    }
    protected virtual void Start()
    {
    }

    private void OnValidate()
    {
        GetRoomWorldBounds();
    }



    public virtual void SetEntityStates(bool state)
    {
        for (int i = 0; i < entities.Count; i++)
        { entities[i].gameObject.SetActive(state); }
    }

    protected virtual void GetRoomWorldBounds()
    { roomWorldBounds = new Rect(roomBounds.position + (Vector2)transform.position, roomBounds.size); }

    public virtual void GetObjectsContainer()
    {
        if (objects == null)
        { objects = transform.Find("Objects"); 
            if (objects == null)
            {
                objects = Instantiate(new GameObject("Objects"), Vector3.zero, Quaternion.identity, transform).transform;
            }
        }
    }

    public virtual void FetchEntities()
    {
        GetObjectsContainer();
        entities = new(objects.GetComponentsInChildren<Entity>());
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
