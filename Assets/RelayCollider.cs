using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class RelayCollider : MonoBehaviour
{
    public enum CollState { Disabled = 0, Enabled = 1 }
    //CollState _colliderState = CollState.Enabled;
    public CollState colliderState { get => GetCollState(); set => SetCollState(value); }
    public Entity entity { get => _entity; }
    [SerializeField] Entity _entity;
    [SerializeField] List<string> relayTags = new();
    Collider2D coll;
    public Collider2D Collider { get => coll; }
    [SerializeField] UnityEvent<RelayCollider, Collider2D> onTriggerEnter, onTriggerExit;

    Dictionary<string, ColliderState> collStates = new Dictionary<string, ColliderState>();

    public delegate bool LoadCollStateDelegate(ColliderState state);
    //public delegate bool SaveCollStateDelegate(string state, Collider2D coll);
    LoadCollStateDelegate LoadCollState;
    //SaveCollStateDelegate SaveCollState;

    private void Start()
    {
        coll = GetComponent<Collider2D>();
        SetTypeDelegate();
        SaveColliderDefaultState();
    }

    public bool HasTag(string tag)
    { return relayTags.Contains(tag); }
    public int HasTags(params string[] tags)
    {
        int c = 0;
        foreach (string t in tags)
        { if (relayTags.Contains(tag)) c++; }
        return c;
    }

    CollState GetCollState()
    {
        if (coll.enabled) return CollState.Enabled;
        return CollState.Disabled;
    }
    void SetCollState(CollState state)
    {
        switch (state)
        {
            case CollState.Enabled:
                coll.enabled = true; break;
            case CollState.Disabled:
                coll.enabled = false; break;
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    { 
        onTriggerEnter.Invoke(this, collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        onTriggerExit.Invoke(this, collision);
    }

    public bool LoadColliderState(string state)
    {
        if (collStates.TryGetValue(state, out ColliderState cstate))
        {
            LoadCollState(cstate);
            return true;
        }
        return false;
    }
    void SaveColliderDefaultState()
    { collStates["default"] = FromCollider2D(coll); }
    public bool SaveColliderState(string state, Collider2D coll)
    {
        if (state == "default") return false;
        if (collStates.TryGetValue(state, out ColliderState cstate))
        {
            collStates[state] = FromCollider2D(coll);
            return true;
        }
        collStates.Add(state, FromCollider2D(coll));
        return true;
    }

    public class ColliderState
    {
        public Rect rect = new Rect(Vector2.zero, Vector2.zero);
        public Vector3 localPosition = Vector3.zero;
        public float radius = 0f;
        public ColliderState(Rect rect, Vector3 local_position)
        {
            this.rect = rect;
            this.localPosition = local_position;
        }
    }

    public static ColliderState FromCollider2D(Collider2D collider)
    {
        Debug.Log($"FromCollider2D colliderType{collider.GetType().ToString()}");
        switch (collider.GetType().ToString())
        {
            case "UnityEngine.BoxCollider2D":
                return FromCollider2D(collider as BoxCollider2D);
            case "UnityEngine.CapsuleCollider2D":
                return FromCollider2D(collider as CapsuleCollider2D);
            case "UnityEngine.CircleCollider2D":
                return FromCollider2D(collider as CircleCollider2D);
        }
        return null;
    }
    public static ColliderState FromCollider2D(BoxCollider2D collider)
    {
        var collstate = new ColliderState(new Rect(collider.offset, collider.size), collider.transform.localPosition);
        collstate.radius = collider.edgeRadius;
        return collstate;
    }
    public static ColliderState FromCollider2D(CapsuleCollider2D collider)
    {
        return new ColliderState(new Rect(collider.offset, collider.size), collider.transform.localPosition);
    }
    public static ColliderState FromCollider2D(CircleCollider2D collider)
    {
        var collstate = new ColliderState(new Rect(collider.offset, collider.radius * 2 * Vector2.one), collider.transform.localPosition);
        collstate.radius = collider.radius;
        return collstate;
    }

    /*
    bool SaveColliderState_BoxCollider(string state, Collider2D coll)
    {
        if (state == "default") return false;
        if (collStates.ContainsKey(state))
        { collStates[state] = FromCollider2D(coll as BoxCollider2D); return true; }
        collStates.Add(state, FromCollider2D(coll as BoxCollider2D));
        return true;
    }
    bool SaveColliderState_CapsuleCollider(string state, Collider2D coll)
    {
        if (state == "default") return false;
        if (collStates.ContainsKey(state))
        { collStates[state] = FromCollider2D(coll as CapsuleCollider2D); return true; }
        collStates.Add(state, FromCollider2D(coll as CapsuleCollider2D));
        return true;
    }
    bool SaveColliderState_CircleCollider(string state, Collider2D coll)
    {
        if (state == "default") return false;
        if (collStates.ContainsKey(state))
        { collStates[state] = FromCollider2D(coll as BoxCollider2D); return true; }
        collStates.Add(state, FromCollider2D(coll as BoxCollider2D));
        return true;
    }
    */

    bool LoadColliderState_BoxCollider(ColliderState cstate)
    {
        coll.offset = cstate.rect.position;
        ((BoxCollider2D)coll).size = cstate.rect.size;
        ((BoxCollider2D)coll).edgeRadius = cstate.radius;
        transform.localPosition = cstate.localPosition;
        return true;
    }

    bool LoadColliderState_CapsuleCollider(ColliderState cstate)
    {
        coll.offset = cstate.rect.position;
        ((CapsuleCollider2D)coll).size = cstate.rect.size;
        transform.localPosition = cstate.localPosition;
        return true;
    }
    bool LoadColliderState_CircleCollider(ColliderState cstate)
    {
        coll.offset = cstate.rect.position;
        ((CircleCollider2D)coll).radius = cstate.radius;
        transform.localPosition = cstate.localPosition;
        return true;
    }

    void SetTypeDelegate()
    {
        Debug.Log($"SetTypeDelegate{coll.GetType().ToString()}");
        switch (coll.GetType().ToString())
        {
            case "UnityEngine.BoxCollider2D":
                LoadCollState = LoadColliderState_BoxCollider;
                //SaveCollState = SaveColliderState_BoxCollider;
                break;
            case "UnityEngine.CapsuleCollider2D":
                LoadCollState = LoadColliderState_CapsuleCollider;
                //SaveCollState = SaveColliderState_CapsuleCollider;
                break;
            case "UnityEngine.CircleCollider2D":
                LoadCollState = LoadColliderState_CircleCollider;
                //SaveCollState = SaveColliderState_CircleCollider; 
                break;
        }
    }
}

