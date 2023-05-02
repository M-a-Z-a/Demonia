using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class EntityStats : MonoBehaviour
{

    [SerializeField] Dictionary<string, Stat> _stats = new();
    [SerializeField] Dictionary<string, Attribute> _attributes = new();
    [SerializeField] List<StatusEffect> _statusEffects = new();
    Dictionary<string, int> _flags = new();

    [SerializeField] string entityStatsPath = "";
    static string _gEntityStatsPath = "Data/stats_default";
    public Dictionary<string, Stat> stats { get => _stats; }
    public Dictionary<string, Attribute> attributes { get => _attributes; }
    public List<StatusEffect> statusEffects { get => _statusEffects; }

    public delegate void OnDamageDelegate(float value, float percentage);


    private void Awake()
    {
        //GetSetStat("health");
    }

    private void Start()
    {
        
    }

    private void OnValidate()
    {
    }

    private void Update()
    {
        UpdateEffects();
    }

    public bool GetFlag(string flag)
    { return _flags.ContainsKey(flag); }
    void AddFlag(string flag)
    {
        if (_flags.ContainsKey(flag))
        { _flags[flag]++; return; }
        _flags.Add(flag, 1);
    }
    void RemoveFlag(string flag)
    {
        if (_flags.TryGetValue(flag, out int v))
        { 
            if (v > 1) 
            { _flags["flag"]--; return; }
            _flags.Remove("flag");
        }
    }


    public void ApplyDamage(Damage damage, MonoBehaviour origin, bool applyEffects = true)
    {
        Debug.Log("EntityStats: ApplyDamage()");
        bool isUniversal = false;
        for (int i = 0; i < damage.flags.Count; i++)
        {
            switch (damage.flags[i])
            {
                case "positive":
                    break;

                case "neutral":
                    break;

                case "universal":
                    isUniversal = true;
                    break;

                case "negative":
                default:
                    break;
            }
        }
        Debug.Log(_stats["health"].value);
        _stats["health"].value -= damage.damage;
        Debug.Log(_stats["health"].value);

        if (!applyEffects) return;
        for (int i = 0; i < damage.effects.Count; i++)
        { AddEffect(damage.effects[i], origin); }
    }

    public bool AddEffect(StatusEffect effect, MonoBehaviour origin, bool apply_OnStart = true)
    {
        if (!effect.Init(this, origin)) return false;
        _statusEffects.Add(effect);
        if (apply_OnStart) effect.OnStart();
        return true;
    }
    public bool RemoveEffect(StatusEffect effect, bool apply_OnEnd = true)
    { 
        if (_statusEffects.Remove(effect))
        {
            if (apply_OnEnd) effect.OnEnd();
            return true;
        }
        return false;
    }
    public void UpdateEffects()
    {
        if (_statusEffects.Count > 0)
        {
            for (int i = 0; i < _statusEffects.Count; i++)
            { _statusEffects[i].OnUpdate(); }
        }
    }

    public void CleanseEffects(bool apply_OnEnd = false)
    {
        for (int i = _statusEffects.Count-1; i >= 0; i--)
        { RemoveEffect(_statusEffects[i], apply_OnEnd); }
    }

    public bool TryGetStat(string name, out Stat stat)
    { return _stats.TryGetValue(name, out stat); }
    public Stat GetStat(string name)
    {
        TryGetStat(name, out Stat stat);
        return stat;
    }
    public Stat SetStat(string name, float value, float max)
    {
        if (TryGetStat(name, out Stat stat))
        { stat.max = max; stat.value = value; return stat; }
        _stats.Add(name, new Stat(name, value, max));
        return _stats[name];
    }
    public Stat GetSetStat(string name, float value_if_set = 100, float max_if_set = 100)
    {
        if (TryGetStat(name, out Stat stat))
        { return stat; }
        _stats.Add(name, new Stat(name, value_if_set, max_if_set));
        return _stats[name];
    }

    public bool TryGetAttribute(string name, out Attribute attr)
    { return _attributes.TryGetValue(name, out attr); }
    public Attribute GetAttribute(string name)
    {
        TryGetAttribute(name, out Attribute attr);
        return attr;
    }
    public Attribute SetAttribute(string name, float value)
    {
        if (TryGetAttribute(name, out Attribute attr))
        { attr.value = value; return attr; }
        _attributes.Add(name, new Attribute(name, value));
        return _attributes[name];
    }
    public Attribute GetSetAttribute(string name, float value_if_set = 0f)
    {
        if (TryGetAttribute(name, out Attribute attr))
        { return attr; }
        _attributes.Add(name, new Attribute(name, value_if_set));
        return _attributes[name];
    }



    [System.Serializable]
    public class Stat
    {
        string _name;
        [SerializeField] float _value = 0, _max, _vlast = 0;
        public string name { get => _name; }
        public float value { get => _value; set => SetValue(value); }
        public float max { get => _max; set => SetMax(value); }
        public float delta { get => _max > 0 ? _value / _max : 0; }

        List<Action<float, float>> onValueChanged = new ();

        public Stat(string name, float value = 100, float max = 100)
        { _name = name; SetMax(max); SetValue(value); }

        void SetValue(float value)
        {
            _value = Mathf.Clamp(value, 0, _max);
            if (_value == _vlast) return;
            _vlast = _value;
            for (int i = 0; i < onValueChanged.Count; i++)
            { onValueChanged[i].Invoke(_value, _vlast); }
        }
        void SetMax(float value)
        { _max = Mathf.Max(value, 0); SetValue(_value); }

        public bool AddListener(Action<float, float> action)
        { 
            if (onValueChanged.Contains(action)) return false; 
            onValueChanged.Add(action); return true;
        }
        public bool RemoveListener(Action<float, float> action)
        { return onValueChanged.Remove(action); }

        public static implicit operator float(Stat stat)
        { return stat.value; }
        public static implicit operator int(Stat stat)
        { return Mathf.FloorToInt(stat.value); }
        public static implicit operator string(Stat stat)
        { return $"{stat.value}"; }
    }

    public class Attribute
    {
        string _name;
        float _value, _vlast = 0;
        public float value { get => _value; set => SetValue(value); }
        public string name { get => _name; }
        List<Action<float, float>> onValueChanged = new();

        public Attribute(string name, float value = 0)
        {
            _name = name;
            this.value = value;
        }
        void SetValue(float v)
        { 
            _value = v;
            if (_value == _vlast) return;
            _vlast = _value;
            for (int i = 0; i < onValueChanged.Count; i++)
            { onValueChanged[i].Invoke(_value, _vlast); }
        }
        public bool AddListener(Action<float, float> action)
        {
            if (onValueChanged.Contains(action)) return false;
            onValueChanged.Add(action); return true;
        }
        public bool RemoveListener(Action<float, float> action)
        { return onValueChanged.Remove(action); }

        public static implicit operator float(Attribute attr)
        { return attr.value; }
        public static implicit operator int(Attribute attr)
        { return Mathf.FloorToInt(attr.value); }
        public static implicit operator string(Attribute attr)
        { return $"{attr.value}"; }
    }




    [System.Serializable]
    public class StatusEffect
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

        public bool Init(EntityStats estats, MonoBehaviour origin)
        {
            if (_estats == null)
            { _estats = estats; _origin = origin; return Init(); }
            return false;
        }
        protected virtual bool Init()
        { return true; }
        public virtual void OnStart()
        { }
        public virtual void OnUpdate()
        { }
        public virtual void OnEnd()
        { }
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

    }

    public class Damage
    {
        public float damage { get => _damage; }
        protected float _damage;
        public List<string> flags { get => _flags; }
        protected List<string> _flags;
        public List<StatusEffect> effects { get => _effects; }
        protected List<StatusEffect> _effects;

        public Damage(float damage, IEnumerable<StatusEffect> statusEffects = null, IEnumerable<string> flags = null)
        {
            _damage = Mathf.Max(damage, 0);
            _effects = statusEffects == null ? new() : new(statusEffects);
            _flags = flags == null ? new() : new(flags);
        }

    }


    public void LoadFromResources(string path)
    {
        TextAsset t = Resources.Load<TextAsset>(path);
        if (t)
        {

        }
    }


    public bool LoadStats(bool compareToDefault = true)
    { return LoadStats(entityStatsPath, compareToDefault); }

    public bool LoadStats(string path, bool compareToDefault = true)
    {
        TextAsset t = Resources.Load<TextAsset>(path);
        if (!t) return false;
        
        if (GetJsonContainer(path, out JsonContainer jcont))
        {
            Debug.Log($"Json data loaded at path \"{path}\"");
            if (compareToDefault && GetJsonContainer(_gEntityStatsPath, out JsonContainer jcont_def))
            {
                List<JsonStat> jstatlist = new(jcont.stats);
                List<JsonAttribute> jattrlist = new(jcont.attributes);
                for (int i = 0; i < jcont_def.stats.Length; i++)
                {
                    if (!GetJcont<JsonStat>(jcont_def.stats[i].name, out _, jstatlist))
                    { jstatlist.Add(jcont_def.stats[i]); }
                }
                for (int i = 0; i < jcont_def.attributes.Length; i++)
                {
                    if (!GetJcont<JsonAttribute>(jcont_def.attributes[i].name, out _, jattrlist))
                    { jattrlist.Add(jcont_def.attributes[i]); }
                }
                jcont.stats = jstatlist.ToArray();
                jcont.attributes = jattrlist.ToArray();
            }
            jcont.LogData();

            ApplyJsonContainer(jcont);

            return true;
        }
        else
        {
            Debug.Log($"Json data not found at path \"{path}\"");
            if ( entityStatsPath != _gEntityStatsPath && GetJsonContainer(_gEntityStatsPath, out jcont))
            {
                Debug.Log($"loaded default {_gEntityStatsPath}");
                ApplyJsonContainer(jcont);
            }
            else
            {
                Debug.Log($"No default json found at \"{_gEntityStatsPath}\"");
            }
        }

        return false;
    }

    bool GetJcont<T>(string name, out JsonSA jcont, List<T> jcontarr) where T: JsonSA
    {
        for (int i = 0; i < jcontarr.Count; i++)
        {
            if (jcontarr[i].name == name && jcontarr[i].GetType() == typeof(T))
            { jcont = jcontarr[i]; return true; }
        }
        jcont = null;
        return false;
    }



    bool GetJsonContainer(string path, out JsonContainer jcont)
    {
        TextAsset t = Resources.Load<TextAsset>(path);
        if (!t) { jcont = default; return false; };
        jcont = JsonUtility.FromJson<JsonContainer>(t.ToString());
        return jcont != null;
    }


    void ApplyJsonContainer(JsonContainer jcont)
    {
        foreach (JsonStat jstat in jcont.stats)
        { SetStat(jstat.name, jstat.value, jstat.max); }
        foreach (JsonAttribute jattr in jcont.attributes)
        { SetAttribute(jattr.name, jattr.value); }
    }

    [System.Serializable]
    class JsonContainer
    {
        public JsonStat[] stats = new JsonStat[0];
        public JsonAttribute[] attributes = new JsonAttribute[0];


        public void LogData()
        {
            List<string> s_stat = new();
            for (int i = 0; i < stats.Length; i++) { s_stat.Add(stats[i]); }
            List<string> s_attr = new();
            for (int i = 0; i < attributes.Length; i++) { s_attr.Add(attributes[i]); }
            string s = $"Stats: \n +{string.Join("\n +", s_stat)}\nAttributes: \n +{string.Join("\n +", s_attr)}";
            Debug.Log(s);
        }
    }

    [System.Serializable]
    class JsonSA
    {
        public string name;
    }
    [System.Serializable]
    class JsonStat : JsonSA
    {
        public float value, max;
        public JsonStat(string name, float value, float max)
        { this.name = name; this.value = value; this.max = max; }
        public JsonStat(Stat stat)
        { this.name = stat.name; this.value = stat.value; this.max = stat.max; }

        public string GetString()
        { return $"{name}: {value}/{max}"; }

        public static implicit operator string(JsonStat jstat)
        { return jstat.GetString(); }
    }
    [System.Serializable]
    class JsonAttribute : JsonSA
    {
        public float value;
        public JsonAttribute(string name, float value)
        { this.name = name; this.value = value; }
        public string GetString()
        { return $"{name}: {value}"; }

        public static implicit operator string(JsonAttribute jattr)
        { return jattr.GetString(); }
    }

}
