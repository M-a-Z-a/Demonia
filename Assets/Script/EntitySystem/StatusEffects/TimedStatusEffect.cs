using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedStatusEffect : StatusEffect
{

    protected float _duration = 1f;
    public float duration { get => _duration; set => _duration = value; }

    public TimedStatusEffect(string name, float duration) : base(name)
    { _duration = duration; tickRate = 10f; }

    protected override bool Init()
    {
        bool rbool = base.Init();
        return rbool;
    }
    public override void OnUpdate(float dt)
    {
        _duration -= dt;
        if (_duration <= 0)
            Remove();
    }

}
