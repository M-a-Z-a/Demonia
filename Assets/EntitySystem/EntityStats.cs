using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityStats : MonoBehaviour
{

    [SerializeField] Dictionary<string, Stat> _stats = new();
    [SerializeField] Dictionary<string, Attribute> _attributes = new();
    [SerializeField] List<StatusEffect> _statusEffects = new();

    [SerializeField] string entityStatsPath = "";
    static string _gEntityStatsPath = "Data/stats_default";
    static EntityStats _gEntityStats = new EntityStats();
    public Dictionary<string, Stat> stats { get => _stats; }
    public Dictionary<string, Attribute> attributes { get => _attributes; }
    public List<StatusEffect> statusEffects { get => _statusEffects; }
    public bool TEST_loadjson;

    private void Start()
    {
        
    }

    private void OnValidate()
    {
        if (TEST_loadjson)
        { 
            TEST_loadjson = false;
            LoadStats();
        }
    }

    private void Update()
    {
        UpdateEffects();
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


    public static bool TryGetGlobalAttribute(string name, out Attribute attr)
    { return _gEntityStats.TryGetAttribute(name, out attr); }
    public static Attribute GetGlobalAttribute(string name)
    { _gEntityStats.TryGetAttribute(name, out Attribute attr); return attr; }
    public static Attribute GetSetGlobalAttribute(string name, float value_if_set = 0f)
    { return _gEntityStats.GetSetAttribute(name, value_if_set); }


    [System.Serializable]
    public class Stat
    {
        string _name;
        [SerializeField] float _value, _max;
        public string name { get => _name; }
        public float value { get => _value; set => SetValue(value); }
        public float max { get => _max; set => SetMax(value); }
        public float delta { get => _max > 0 ? _value / _max : 0; }

        public Stat(string name, float value = 100, float max = 100)
        { _name = name; SetValue(value); SetMax(max); }

        void SetValue(float value)
        { _value = Mathf.Clamp(value, 0, _max); }
        void SetMax(float value)
        { _max = Mathf.Max(_max, 0); SetValue(value); }


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
        public float value;
        public string name { get => _name; }

        public Attribute(string name, float value = 0)
        {
            _name = name;
            this.value = value;
        }

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
        string _name;
        MonoBehaviour _origin;
        EntityStats _estats;
        protected EntityStats entityStats { get => _estats; }
        public string name { get => _name; }

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
    }













    public void LoadFromResources(string path)
    {
        TextAsset t = Resources.Load<TextAsset>(path);
        if (t)
        {

        }
    }



    public static void LoadGlobalDefaults()
    { _gEntityStats.LoadStats(_gEntityStatsPath, false); }


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
