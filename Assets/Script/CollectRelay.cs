using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollectRelay : MonoBehaviour
{
    [SerializeField] UnityEvent<CollectRelay> onCollect;
    public Entity ent;
    public void Collect(Entity entity)
    { ent = entity; onCollect.Invoke(this); }
}
