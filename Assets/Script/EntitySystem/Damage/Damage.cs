using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Damage
{
    public float damage { get => _damage; }
    protected float _damage;
    public List<string> flags { get => _flags; }
    protected List<string> _flags;
    public List<StatusEffect> effects { get => _effects; }
    protected List<StatusEffect> _effects;

    protected string _formula = null;
    public string formula { get => _formula; }

    public Damage(float damage, IEnumerable<StatusEffect> statusEffects = null, IEnumerable<string> flags = null)
    {
        _damage = Mathf.Max(damage, 0);
        _effects = statusEffects == null ? new() : new(statusEffects);
        _flags = flags == null ? new() : new(flags);
    }
}


