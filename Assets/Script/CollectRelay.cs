using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollectRelay : MonoBehaviour
{
    string groupName = "mapData";
    [SerializeField] string saveID = "";
    [SerializeField] UnityEvent<CollectRelay> onCollect;
    [HideInInspector] public Entity ent;
    public void Collect(Entity entity)
    { 
        ent = entity; 
        onCollect.Invoke(this);
        if (saveID == "") return;
        SaveManager.SetBool(saveID, true);
        Player.SaveData();
    }

    private void OnEnable()
    {
        if (SaveManager.GetBool(saveID, out bool v))
        { if (v) gameObject.SetActive(false); }
    }

}
