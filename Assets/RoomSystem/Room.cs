using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    static Room _activeRoom;
    public static Room activeRoom { get => _activeRoom; }
    
    public enum RoomState { Disabled = 0, Enabled = 1, Active = 2 };

    public static Dictionary<string, int> conditionValues;
    [SerializeField] List<Room> connectedRooms;
    [SerializeField] List<ConnectionCondition> conditions;
    [SerializeField] Rect _roomBounds;
    Rect _roomWorldBounds;

    public bool firstLoad = true;
    
    RoomState _roomState = RoomState.Disabled;
    public bool isActive { get => _roomState == RoomState.Active; }
    public bool isEnabled { get => _roomState >= RoomState.Enabled; }
    public RoomState roomState { get => _roomState; }
    public Rect roomLocalBounds { get => _roomBounds; }
    public Rect roomBounds { get => _roomWorldBounds; }

    Transform objects;
    List<Entity> entities = new();



    private void OnDrawGizmos()
    {
        Vector3 rposmin = transform.position + (Vector3)_roomBounds.min;
        Vector3 rposmax = transform.position + (Vector3)_roomBounds.max;
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(rposmin, new Vector3(_roomBounds.width, 0, 0));
        Gizmos.DrawRay(rposmin, new Vector3(0, _roomBounds.height, 0));
        Gizmos.DrawRay(rposmax, new Vector3(-_roomBounds.width, 0, 0));
        Gizmos.DrawRay(rposmax, new Vector3(0, -_roomBounds.height, 0));
    }

    private void Start()
    {
        if (_activeRoom == null)
        { _activeRoom = this; }
        GetRoomWorldBounds();
        FetchEntities();
        CameraControl.instance.bounds = roomBounds;
    }

    void GetRoomWorldBounds()
    { _roomWorldBounds = new Rect(_roomBounds.position + (Vector2)transform.position, _roomBounds.size); }

    public void GetObjectsContainer()
    {
        if (objects == null)
        { objects = transform.Find("Objects"); 
            if (objects == null)
            {
                objects = Instantiate(new GameObject("Objects"), Vector3.zero, Quaternion.identity, transform).transform;
            }
        }
    }

    public void FetchEntities()
    {
        GetObjectsContainer();
        entities = new(objects.GetComponentsInChildren<Entity>());
    }


    public void SetActive(bool state)
    {
        if (state) Activate();
        else Deactivate();
    }
    public bool Activate()
    {
        if (_roomState == RoomState.Disabled) return false;
        _roomState = RoomState.Active;
        firstLoad = false;
        _activeRoom = this;
        return true;
    }
    public bool Deactivate()
    {
        if (_roomState == RoomState.Disabled) return false;
        _roomState = RoomState.Enabled;
        return true;
    }

    public bool Load()
    {
        if (_roomState == RoomState.Disabled && TestConditions())
        { _roomState = RoomState.Enabled; return true; }
        return false;
    }
    public bool Unload()
    {
        if (_roomState == RoomState.Disabled) return false;
        _roomState = RoomState.Disabled;
        return true;
    }

    public bool TestConditions()
    {
        int conds = 0;
        for (int i = 0; i < conditions.Count; i++)
        { if (conditions[i].TestCondition()) conds++; }
        return conds == conditions.Count;
    }

    [System.Serializable]
    public class ConnectionCondition
    { 
        public enum ConditionComparison { 
            NotEqual = 0,
            Equal = 1,
            BiggerThan = 2,
            BiggerThanOrEqual = 3,
            Smaller = 4,
            SmallerOrEqual = 5 
        }
        public string key;
        public ConditionComparison comparison;
        public int value; 

        public bool TestCondition()
        { 
            if (conditionValues.TryGetValue(key, out int v))
            { 
                switch (comparison)
                {
                    case ConditionComparison.NotEqual:
                        return value != v;
                    case ConditionComparison.Equal:
                        return value == v;
                    case ConditionComparison.BiggerThan:
                        return value > v;
                    case ConditionComparison.BiggerThanOrEqual:
                        return value >= v;
                    case ConditionComparison.Smaller:
                        return value < v;
                    case ConditionComparison.SmallerOrEqual:
                        return value <= v;
                }
            }
            return true;
        }
    }
}
