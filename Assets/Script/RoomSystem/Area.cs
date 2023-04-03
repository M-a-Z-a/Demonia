using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        rooms = new();
        foreach (Transform t in transform)
        {
            if (t.TryGetComponent<Room>(out Room r))
            { rooms.Add(r); }
        }
    }

    public static bool FindPlayerRoom(out Room room)
    {
        room = null;
        Vector2 ppos = Player.pTransform.position;
        foreach (Room r in activeArea.rooms)
        {
            if (r.roomBounds.Contains(ppos))
            { room = r; return true; }
        }
        return false;
    }
}
