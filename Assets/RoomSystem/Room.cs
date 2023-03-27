using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public enum RoomState { Disabled = 0, Enabled = 1, Active = 2 };
    public static Room activeRoom { get; protected set; }

    public static Dictionary<string, int> conditionValues;
    [SerializeField] protected List<Room> connectedRooms;
    [SerializeField] protected Rect _roomBounds;
    protected Rect _roomWorldBounds;

    public bool firstLoad = true;

    protected RoomState _roomState = RoomState.Disabled;
    public bool isActive { get => _roomState == RoomState.Active; }
    public bool isEnabled { get => _roomState >= RoomState.Enabled; }
    public RoomState roomState { get => _roomState; }
    public Rect roomLocalBounds { get => _roomBounds; }
    public Rect roomBounds { get => _roomWorldBounds; }

    protected Transform objects;
    protected List<Entity> entities = new();



    protected virtual void OnDrawGizmos()
    {
        Vector3 rposmin = transform.position + (Vector3)_roomBounds.min;
        Vector3 rposmax = transform.position + (Vector3)_roomBounds.max;
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(rposmin, new Vector3(_roomBounds.width, 0, 0));
        Gizmos.DrawRay(rposmin, new Vector3(0, _roomBounds.height, 0));
        Gizmos.DrawRay(rposmax, new Vector3(-_roomBounds.width, 0, 0));
        Gizmos.DrawRay(rposmax, new Vector3(0, -_roomBounds.height, 0));
    }

    protected virtual void Awake()
    {
        GetRoomWorldBounds();
        FetchEntities();
    }
    protected virtual void Start()
    {
        if (activeRoom == null)
        { 
            activeRoom = this;
            CameraControl.instance.bounds = roomBounds;
        }
    }

    protected virtual void GetRoomWorldBounds()
    { _roomWorldBounds = new Rect(_roomBounds.position + (Vector2)transform.position, _roomBounds.size); }

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

    public void LoadAdjacentRooms()
    {

    }

    public virtual void SetState(RoomState state)
    {
        switch (state)
        {
            case RoomState.Active:
                Activate();
                break;
            case RoomState.Enabled:
                Deactivate();
                break;
            default: //RoomState.Disabled;
                Unload();
                break;
        }
    }
    public virtual bool Activate()
    {
        if (_roomState == RoomState.Disabled) return false;
        _roomState = RoomState.Active;
        firstLoad = false;
        activeRoom = this;
        return true;
    }
    public virtual bool Deactivate()
    {
        if (_roomState == RoomState.Disabled) return false;
        _roomState = RoomState.Enabled;
        return true;
    }

    public virtual bool Load()
    {
        if (_roomState == RoomState.Disabled)
        { _roomState = RoomState.Enabled; return true; }
        return false;
    }

    public virtual bool Unload()
    {
        Deactivate();
        if (_roomState == RoomState.Disabled) return false;
        _roomState = RoomState.Disabled;
        return true;
    }


    
}
