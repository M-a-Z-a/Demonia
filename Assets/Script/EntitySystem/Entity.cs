using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public enum EntityType { Entity, Object }

    static string _groundMaskDefaultName = "Ground", _platformMaskDefaultName = "Platform";

    public static int groundMaskDefault { get => Utility.GetLayerMaskByNames(_groundMaskDefaultName); }
    public static int platformMaskDefault { get => Utility.GetLayerMaskByNames(_platformMaskDefaultName); }


    public string entityName { get => _entityName; set => Rename(value); }
    [SerializeField] string _entityName = "";
    [SerializeField] EntityType _entityType = 0;
    [SerializeField] protected List<string> entityTags = new();
    public EntityType entityType { get => _entityType; }
    public List<string> EntityTags { get => entityTags; }
    public EntityStats entityStats { get => _entityStats; }
    protected EntityStats _entityStats;

    public static float gravity = -9.81f * 2;

    public Vector2 velocity { get => _velocity; set => _velocity = value; }
    protected Vector2 _velocity = Vector2.zero;


    protected virtual void Awake()
    {
        FetchEntityComponents();
    }
    protected virtual void Start()
    {

    }

    protected virtual void OnValidate()
    {
        Rename(_entityName);
    }

    public void Rename(string name)
    {
        _entityName = name??"";
        switch (entityType)
        {
            case EntityType.Entity:
                gameObject.name = $"Entity({(name==""?"<nameless>":name)})"; break;
            case EntityType.Object:
                gameObject.name = $"Object({(name==""?"<nameless>":name)})"; break;
        }
    }

    public void FetchEntityComponents()
    {
        if (!TryGetComponent<EntityStats>(out _entityStats))
        { _entityStats = gameObject.AddComponent<EntityStats>(); }
    }

    public bool HasTag(string tag)
    { return entityTags.Contains(tag); }
    public bool HasTags(params string[] tags)
    {
        int c = 0;
        foreach (string t in tags)
        { if (entityTags.Contains(t)) c++; }
        return c == tags.Length;
    }
    public bool HasTags(int min_count, params string[] tags)
    {
        int c = 0;
        foreach (string t in tags)
        { if (entityTags.Contains(t)) c++; }
        return c >= min_count;
    }
    public int CountTags(params string[] tags)
    {
        int c = 0;
        foreach (string t in tags)
        { if (entityTags.Contains(t)) c++; }
        return c;
    }

    public virtual void AddForce(Vector2 force)
    {
        float mult = 1f;
        if (entityStats != null)
        {
            if (entityStats.TryGetAttribute("forceResist", out EntityStats.Attribute attr))
            { mult = 1f - Utility.EaseOutSine01(attr.value * 0.01f); }
        }
        _velocity += force * mult;
    }
    public virtual void ApplyDamage(Damage damage, MonoBehaviour origin)
    {
        Debug.Log("Entity: ApplyDamage()");
        if (entityStats == null) return;
        entityStats.ApplyDamage(damage, origin);
    }

    public bool HasParentEntity(int lookoutDepth = 20, Transform stop_at = null)
    {
        if (stop_at == null) stop_at = transform.root;
        Transform pt = transform.parent;
        for (int i = 0; i < lookoutDepth; i++)
        {
            if (pt == stop_at || pt == transform.root)
            { return false; }
            if (pt.GetComponent<Entity>())
            { return true; }
            pt = pt.parent;
        }
        return false;
    }
}
