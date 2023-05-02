using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class RelayCollider : MonoBehaviour
{
    public Entity entity { get => _entity; }
    [SerializeField] Entity _entity;
    [SerializeField] List<string> relayTags = new();

    [SerializeField] UnityEvent<Collider2D> onTriggerEnter, onTriggerExit;

    public bool HasTag(string tag)
    { return relayTags.Contains(tag); }
    public int HasTags(params string[] tags)
    {
        int c = 0;
        foreach (string t in tags)
        { if (relayTags.Contains(tag)) c++; }
        return c;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    { 
        onTriggerEnter.Invoke(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        onTriggerExit.Invoke(collision);
    }

}
