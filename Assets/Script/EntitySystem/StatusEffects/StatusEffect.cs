using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EntityStats;

[System.Serializable]
public abstract class StatusEffect
{
    public string name { get => _name; }
    string _name;
    protected EntityStats entityStats { get => _estats; }
    EntityStats _estats;
    public List<string> flags { get => _flags; }
    List<string> _flags = new();
    public MonoBehaviour origin { get => _origin; }
    MonoBehaviour _origin;
    public StatusEffect(string name)
    { _name = name; }

    public Dictionary<string, float> mod_attr = new(), mod_stat = new();

    public Coroutine updateCoroutine;
    float _tickRate = 4f, _tickRateSec = 0;
    public float tickRate { get => _tickRate; protected set => SetTickrate(value); }
   
    void SetTickrate(float tick_rate)
    {
        if (tick_rate == 0) return;
        if (tick_rate < 0)
        { UpdateOne = IUpdate_DeltaTime; return; }
        UpdateOne = IUpdate_TickRate;
        _tickRateSec = 1f / tick_rate;
    }

    public bool Init(EntityStats estats, MonoBehaviour origin)
    {
        bool rbool = false;
        if (_estats == null)
        { 
            _estats = estats;
            _origin = origin;
            rbool = Init();
            if (tickRate != 0)
            { SetTickrate(tickRate); updateCoroutine = _estats.StartCoroutine(IUpdate()); }
        }
        return rbool;
    }
    public void End()
    {
        if (updateCoroutine != null) _estats.StopCoroutine(updateCoroutine);
        _estats = null;
        OnEnd();
    }

    protected virtual bool Init()
    { 
        return true;
    }
    public virtual void OnStart()
    {
        foreach (string k in mod_attr.Keys)
        { _estats.GetSetAttribute(k).value_mod += mod_attr[k]; }
        foreach (string k in mod_stat.Keys)
        {
            if (_estats.TryGetStat(k, out Stat stat))
            { stat.max_mod += mod_stat[k]; }
        }
    }
    public virtual void OnUpdate(float dt)
    { }
    public virtual void OnEnd()
    {
        foreach (string k in mod_attr.Keys)
        { _estats.GetSetAttribute(k).value_mod -= mod_attr[k]; }
        foreach (string k in mod_stat.Keys)
        {
            if (_estats.TryGetStat(k, out Stat stat))
            { stat.max_mod -= mod_stat[k]; }
        }
    }
    public bool Remove()
    {
        if (_estats != null)
        { return _estats.RemoveEffect(this); }
        return false;
    }

    protected bool AddFlag(string flag)
    {
        if (_flags.Contains(flag)) return false;
        _flags.Add(flag);
        return true;
    }
    protected bool RemoveFlag(string flag)
    { return _flags.Remove(flag); }

    public virtual IEnumerator IUpdate()
    {
        while (true)
        { yield return UpdateOne(); }
    }

    delegate IEnumerator UpdateDelegate();
    UpdateDelegate UpdateOne;
    IEnumerator IUpdate_TickRate()
    {
        OnUpdate(_tickRateSec);
        yield return new WaitForSeconds(_tickRateSec);
    }
    IEnumerator IUpdate_DeltaTime()
    {
        OnUpdate(Time.deltaTime);
        yield return new WaitForEndOfFrame();
    }

}
