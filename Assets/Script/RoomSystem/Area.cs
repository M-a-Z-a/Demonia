using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Room;

public class Area : MonoBehaviour
{
    public static Area activeArea { get; protected set; }
    List<Room> rooms;

    private void Awake()
    {
        activeArea = this;
    }
    private void Start()
    {
        rooms = new(gameObject.GetComponentsInChildren<Room>(true));
        Debug.Log($"room count: {rooms.Count}");
        if (rooms.Count > 0)
        { 
            foreach (Room r in rooms)
            { r.Unload(); }
            rooms[0].Activate(); 
        }
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

}
