using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollectRelay : MonoBehaviour
{
    [SerializeField] UnityEvent<Entity> onCollect;   
    public void Collect(Entity entity)
    { onCollect.Invoke(entity); }
}
