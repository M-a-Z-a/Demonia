using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Text.RegularExpressions;
using Unity.VisualScripting;

public class EntityStats : MonoBehaviour
{
    public const string formula_attribute = "%<attr>%";
    public const string forumla_stat = "%<stat><stat.max>%";
    public const string formulaRegex = "";

    [SerializeField] Dictionary<string, Stat> _stats = new();
    [SerializeField] Dictionary<string, Attribute> _attributes = new();
    [SerializeField] List<StatusEffect> _statusEffects = new();
    [SerializeField] Dictionary<string, int> _flags = new();
    Dictionary<string, float> _statMods = new(), _attrMods = new();


    [SerializeField] string entityStatsPath = "";
    static string _gEntityStatsPath = "Data/stats_default";
    public Dictionary<string, Stat> stats { get => _stats; }
    public Dictionary<string, Attribute> attributes { get => _attributes; }
    public List<StatusEffect> statusEffects { get => _statusEffects; }

    public delegate void OnDamageDelegate(float value, float percentage);

    [NonSerialized] public UnityEvent<StatusEffect> onStatusEffectAdded = new(), onStatusEffectRemoved = new();
    [NonSerialized] public UnityEvent<Attribute> onAttributeAdded = new();
    [NonSerialized] public UnityEvent<Stat> onStatAdded = new();


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


    public bool GetFlag(string flag)
    { return _flags.ContainsKey(flag); }
    public List<string> GetFlags()
    {
        List<string> s = new();
        foreach (string k in _flags.Keys)
        { s.Add(k); }
        return s;
    }

    void AddFlag(string flag)
    {
        if (_flags.ContainsKey(flag))
        { _flags[flag]++; return; }
        _flags.Add(flag, 1);
    }
    void AddFlags(IEnumerable<string> flags)
    {
        List<string> fadded = new();
        foreach (string f in flags)
        {
            if (fadded.Contains(f)) continue;
            fadded.Add(f);
            AddFlag(f);
        }
    }
    bool RemoveFlag(string flag)
    {
        if (_flags.TryGetValue(flag, out int v))
        { 
            if (v > 1) 
            { _flags[flag]--; return true; }
            _flags.Remove(flag);
            return true;
        }
        return false;
    }
    int RemoveFlags(IEnumerable<string> flags)
    {
        int frem = 0;
        List<string> fremoved = new();
        foreach (string f in flags)
        {
            if (fremoved.Contains(f)) continue;
            fremoved.Add(f);
            if (RemoveFlag(f)) frem++;
        }
        return frem;
    }


    public void ApplyDamage(Damage damage, MonoBehaviour origin, bool applyEffects = true)
    {
        Debug.Log("EntityStats: ApplyDamage()");
        for (int i = 0; i < damage.flags.Count; i++)
        {
            switch (damage.flags[i])
            {
                case "positive":
                    break;

                case "neutral":
                    break;

                case "negative":
                default:
                    break;
            }
        }
        //Debug.Log(_stats["health"].value);
        if (damage.formula != null)
        {
            float dmg_value = ParseFormula(damage);
            _stats["health"].value -= dmg_value; 
        }
        else
        { _stats["health"].value -= damage.damage; }
        //Debug.Log(_stats["health"].value);

        if (!applyEffects) return;
        for (int i = 0; i < damage.effects.Count; i++)
        { AddEffect(damage.effects[i], origin); }
    }

    public bool AddEffect(StatusEffect effect, MonoBehaviour origin)
    {
        if (!effect.Init(this, origin)) return false;
        _statusEffects.Add(effect);
        effect.OnStart();
        AddFlags(effect.flags);
        onStatusEffectAdded.Invoke(effect);
        return true;
    }
    public bool RemoveEffect(StatusEffect effect)
    { 
        if (_statusEffects.Remove(effect))
        {
            effect.OnEnd();
            RemoveFlags(effect.flags);
            onStatusEffectRemoved.Invoke(effect);
            return true;
        }
        return false;
    }


    void UpdateMods()
    {
        Attribute attr;
        Stat stat;
        foreach (string k in _attrMods.Keys)
        {
            if (!_attributes.TryGetValue(k, out attr)) continue;
            attr.value_mod += _attrMods[k];
        }
        foreach (string k in _statMods.Keys)
        {
            if (!_stats.TryGetValue(k, out stat)) continue;
            stat.max_mod += _statMods[k];
        }
    }
    bool UpdateStatMod(string stat_name)
    {
        if (!_stats.TryGetValue(stat_name, out Stat stat)) return false;
        if (_statMods.TryGetValue(stat_name, out float value))
        { stat.max_mod += value; }
        return true;
    }
    bool UpdateAttrMod(string attr_name)
    {
        if (!_attributes.TryGetValue(attr_name, out Attribute attr)) return false;
        if (_attrMods.TryGetValue(attr_name, out float value))
        { attr.value_mod += value; }
        return true;
    }

    /*
    public void UpdateEffects()
    {
        if (_statusEffects.Count > 0)
        {
            for (int i = 0; i < _statusEffects.Count; i++)
            { _statusEffects[i].OnUpdate(); }
        }
    }
    */

    float ParseFormula(Damage damage)
    {
        return damage.damage;
    }

    public void CleanseEffects()
    {
        for (int i = _statusEffects.Count-1; i >= 0; i--)
        { RemoveEffect(_statusEffects[i]); }
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
        onStatAdded.Invoke(_stats[name]);
        return _stats[name];
    }
    public Stat GetSetStat(string name, float value_if_set = 100, float max_if_set = 100)
    {
        if (TryGetStat(name, out Stat stat))
        { return stat; }
        _stats.Add(name, new Stat(name, value_if_set, max_if_set));
        onStatAdded.Invoke(_stats[name]);
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
        onAttributeAdded.Invoke(_attributes[name]);
        return _attributes[name];
    }
    public Attribute GetSetAttribute(string name, float value_if_set = 0f)
    {
        if (TryGetAttribute(name, out Attribute attr))
        { return attr; }
        _attributes.Add(name, new Attribute(name, value_if_set));
        onAttributeAdded.Invoke(_attributes[name]);
        return _attributes[name];
    }



    [System.Serializable]
    public class Stat
    {
        [SerializeField] string _name;
        [SerializeField] float _value = 0, _max, _modmax = 0, _vlast = 0;
        public string name { get => _name; }
        public float value { get => _value; set => SetValue(value); }
        public float max { get => _max + _modmax; set => SetModMax(value); }
        public float max_raw { get => _max; set => SetMax(value); }
        public float max_mod { get => _modmax; set => SetMod(value); }
        public float delta { get => _max > 0 ? _value / _max : 0; }

        [NonSerialized] public UnityEvent<Stat, float, float> onValueChanged = new ();
        [NonSerialized] public UnityEvent<Stat, float> onMaxChanged = new();

        public void TestMax()
        { onMaxChanged.Invoke(this, max); }
        public void TestValue()
        { onValueChanged.Invoke(this, _value, _value); }

        public void Refill()
        { value = max; }

        public void SetValues(float value, float max)
        { _max = max; SetValue(value); onMaxChanged.Invoke(this, max); }

        public Stat(string name, float value = 100, float max = 100)
        { _name = name; SetMax(max); SetValue(value); }

        public bool TryConsume(float value)
        {
            if (_value < value) return false;
            this.value -= value;
            return true;
        }

        float _valtmp;
        void SetValue(float value)
        {
            _valtmp = _value;
            _value = Mathf.Clamp(value, 0, max);
            if (_value == _vlast) return;
            _vlast = _value;
            onValueChanged.Invoke(this, _value, _valtmp);
        }
        void SetMax(float value)
        {
            float v = _max;
            _max = Mathf.Max(value, 0);
            float vdiff = _max - v;
            if (_value > 0) { SetValue(Mathf.Max(_value + vdiff, 1)); }
            else { SetValue(_value + vdiff); }
            MaxChanged(); 
        }
        void SetModMax(float v)
        { SetMod(v - _max); }
        void SetMod(float v, bool maxchanged = true)
        { _modmax = v; SetValue(_value); MaxChanged(); }

        void MaxChanged()
        { onMaxChanged.Invoke(this, max); }

        public static implicit operator float(Stat stat)
        { return stat.value; }
        public static implicit operator int(Stat stat)
        { return Mathf.FloorToInt(stat.value); }
        public static implicit operator string(Stat stat)
        { return $"{stat.value}"; }
    }

    [System.Serializable]
    public class Attribute
    {
        [SerializeField] string _name;
        [SerializeField] float _value = 0, _modvalue = 0, _vlast = 0;
        public float value { get => _value + _modvalue; set => SetModValue(value); }
        public float value_mod { get => _modvalue; set => SetMod(value); }
        public float value_raw { get => _value; set => SetValue(value); }
        public string name { get => _name; }
        [NonSerialized] public UnityEvent<Attribute, float, float> onValueChanged = new();//, onModChanged = new();

        public void TestValue()
        {
            onValueChanged.Invoke(this, value, _vlast);
        }

        public Attribute(string name, float value = 0)
        {
            _name = name;
            _value = value;
            _vlast = this.value;
        }

        void SetValue(float v)
        { 
            _value = v;
            if (value == _vlast) return;
            onValueChanged.Invoke(this, value, _vlast);
            _vlast = value;
        }
        void SetModValue(float v)
        {
            float val = v - _value;
            SetMod(val);
        }
        void SetMod(float v)
        { 
            _modvalue = v;
            if (value == _vlast) return;
            onValueChanged.Invoke(this, value, _vlast);
            _vlast = value;
        }

        public static implicit operator float(Attribute attr)
        { return attr.value; }
        public static implicit operator int(Attribute attr)
        { return Mathf.FloorToInt(attr.value); }
        public static implicit operator string(Attribute attr)
        { return $"{attr.value}"; }
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




[System.Serializable]
[CreateAssetMenu(fileName = "SaveDataAsset(EntityStats)", menuName = "SaveData/EntityStats")]
public class SaveDataAssetEntityStats : ScriptableObject
{
    [SerializeField] Dictionary<string, EntityStats.Stat> stats = new();
    [SerializeField] Dictionary<string, EntityStats.Attribute> attributes = new();

    public void PushValues(EntityStats estats, bool push_attributes = true, bool push_stats = true)
    {
        EntityStats.Attribute attr, attr_out;
        EntityStats.Stat stat, stat_out;
        if (push_stats)
        {
            foreach (string k in estats.stats.Keys)
            {
                stat = estats.stats[k];
                if (stats.TryGetValue(k, out stat_out))
                {
                    stat_out.SetValues(stat.value, stat.max_raw);
                    continue;
                }
                stats.Add(k, new EntityStats.Stat(k, stat.value, stat.max_raw));
            }
        }
        if (push_attributes)
        {
            foreach (string k in estats.attributes.Keys)
            {
                attr = estats.attributes[k];
                if (attributes.TryGetValue(k, out attr_out))
                {
                    attr_out.value_raw = attr.value_raw;
                    continue;
                }
                attributes.Add(k, new EntityStats.Attribute(k, attr.value_raw));
            }
        } 

        Debug.Log($"saved health: {(stats.TryGetValue("health", out EntityStats.Stat hstat) ? hstat.value : "none")}");
    }

    public void PullValues(EntityStats estats, bool pull_attributes = true, bool pull_stats = true)
    {
        EntityStats.Attribute attr, attr_out;
        EntityStats.Stat stat, stat_out;
        
        if (pull_attributes)
        {
            foreach (string k in attributes.Keys)
            {
                attr = attributes[k];
                if (estats.attributes.TryGetValue(k, out attr_out))
                {
                    attr_out.value_raw = attr.value_raw;
                    continue; 
                }
                estats.SetAttribute(k, attr.value_raw);
            }
        }
        if (pull_stats)
        {
            foreach (string k in stats.Keys)
            {
                stat = stats[k];
                if (estats.stats.TryGetValue(k, out stat_out))
                {
                    stat_out.SetValues(stat.value, stat.max_raw);
                    continue;
                }
                estats.SetStat(k, stat.value, stat.max_raw);
            }
        }
    }


    public void Clear()
    {
        stats = new();
        attributes = new();
    }

    public string ToJson()
    { 
        SaveDataAssetEntityStatsWrapper wrapper = new SaveDataAssetEntityStatsWrapper();

        wrapper.stats = new();
        foreach (string k in stats.Keys)
        { wrapper.stats.Add(new StatWrapper(stats[k])); }

        wrapper.attributes = new();
        foreach (string k in attributes.Keys)
        { wrapper.attributes.Add(new AttributeWrapper(attributes[k])); }

        return JsonUtility.ToJson(wrapper, true);
    }
    public void FromJson(string json)
    {
        SaveDataAssetEntityStatsWrapper wrapper = JsonUtility.FromJson<SaveDataAssetEntityStatsWrapper>(json);
        stats = new();
        attributes = new();
        EntityStats.Stat estat;
        EntityStats.Attribute eattr;
        foreach (StatWrapper statw in wrapper.stats)
        {
            estat = new EntityStats.Stat(statw.name, statw.value, statw.max);
            stats.Add(statw.name, estat);
        }
        foreach (AttributeWrapper attrw in wrapper.attributes)
        {
            eattr = new EntityStats.Attribute(attrw.name, attrw.value);
            attributes.Add(attrw.name, eattr);
        }
    }

    [System.Serializable]
    public class SaveDataAssetEntityStatsWrapper
    {
        public List<StatWrapper> stats;
        public List<AttributeWrapper> attributes;
    }
    [System.Serializable]
    public class StatWrapper
    {
        public string name;
        public float value, max;
        public StatWrapper(EntityStats.Stat stat)
        {
            name = stat.name;
            value = stat.value;
            max = stat.max_raw;
        }
        public bool SetStat(EntityStats.Stat stat)
        {
            if (stat.name != name) return false;
            stat.SetValues(value, max);
            return true;
        }
    }
    [System.Serializable]
    public class AttributeWrapper
    {
        public string name;
        public float value;
        public AttributeWrapper(EntityStats.Attribute attribute)
        {
            name = attribute.name;
            value = attribute.value_raw;
        }
        public bool SetAttribute(EntityStats.Attribute attribute)
        {
            if (attribute.name != name) return false;
            attribute.value_raw = value;
            return true;
        }
    }

}