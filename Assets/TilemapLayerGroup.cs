using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapLayerGroup : MonoBehaviour
{

    public string tmapNamePrefix = "";
    List<TilemapLayer> tmaps;


    private void OnValidate()
    {

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
