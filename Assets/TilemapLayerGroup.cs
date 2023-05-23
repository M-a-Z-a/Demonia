using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapLayerGroup : MonoBehaviour
{
    [SerializeField]Room room;
    [SerializeField]string tmapNamePrefix = "";
    List<TilemapLayer> tmaps = new();


    private void OnValidate()
    {
        if (room == null)
        { GetRoom(); }
        if (room != null)
        { tmapNamePrefix = room.roomName; }
        GetLayers();
        RenameObject();
    }

    bool GetRoom()
    {
        Transform t = transform.parent;
        for (int i = 0; i < 50; i++)
        {
            room = t.GetComponent<Room>();
            if (t == transform.root || room != null) break;
            t = t.parent;
        }
        return room != null;
    }
    void GetLayers()
    { tmaps = new(GetComponentsInChildren<TilemapLayer>()); }
    void RenameObject()
    {
        gameObject.name = tmapNamePrefix == "" ? "Layers" : $"Layers({tmapNamePrefix})";
        for (int i = 0; i < tmaps.Count; i++)
        { tmaps[i].gameObject.name = $"{tmapNamePrefix}({tmaps[i].layerName})"; }
    }
    void RenameTMapObjects()
    {
        if (tmapNamePrefix == "") return;
        foreach (TilemapLayer tl in tmaps)
        {
            //tm.name = $"{tmapNamePrefix}.L{}";
        }
    }

}
