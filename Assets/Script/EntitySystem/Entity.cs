using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{

    public string entityName { get => _entityName; set => Rename(value); }
    [SerializeField] string _entityName = "Entity";
    public EntityStats entityStats { get => _entityStats; }
    protected EntityStats _entityStats;

    public static float gravity = -9.81f * 2;

    public Vector2 velocity { get => _velocity; }
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
        _entityName = name;
        gameObject.name = $"Entity({name})";
    }

    public void FetchEntityComponents()
    {
        if (!TryGetComponent<EntityStats>(out _entityStats))
        { _entityStats = gameObject.AddComponent<EntityStats>(); }
    }

    public virtual void AddForce(Vector2 force)
    {
        _velocity += force;
    }
}
