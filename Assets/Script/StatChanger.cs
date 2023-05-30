using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatChanger : MonoBehaviour
{
    public enum StatType { Attribute_Base, Attribute_Mod, Stat_Value, Stat_Max, Stat_MaxMod }
    public enum ChangeActionType { Set, Add, Subtract, Multiply, Divide, Round, Floor, Ceil }
    [SerializeField] List<StatChange> statChanges;

    public void ApplyChanges(Collider2D collider)
    {
        if (collider.TryGetComponent<Entity>(out Entity ent))
        { ApplyChanges(ent); }
    }
    public void ApplyChanges(CollectRelay relay)
    { ApplyChanges(relay.ent); }
    public void ApplyChanges(Entity entity)
    {
        if (!entity.entityStats) return;
        EntityStats.Attribute attr = default;
        EntityStats.Stat stat = default;
        float v;
        foreach (StatChange ac in statChanges)
        {
            //attr = entity.entityStats.GetSetAttribute(ac.statName, ac.defaultValue);
            //stat = entity.entityStats.GetSetStat(ac.statName, ac.defaultValue);

            switch(ac.statType)
            {
                case StatType.Attribute_Base:
                    attr = entity.entityStats.GetSetAttribute(ac.statName, ac.defaultValue);
                    v = attr.value_raw; 
                    break;
                case StatType.Attribute_Mod:
                    attr = entity.entityStats.GetSetAttribute(ac.statName, ac.defaultValue);
                    v = attr.value_mod;
                    break;
                case StatType.Stat_Value:
                    stat = entity.entityStats.GetSetStat(ac.statName, ac.defaultValue, ac.defaultValue);
                    v = stat.value;
                    break;
                case StatType.Stat_Max:
                    stat = entity.entityStats.GetSetStat(ac.statName, ac.defaultValue, ac.defaultValue);
                    v = stat.max_raw;
                    break;
                case StatType.Stat_MaxMod:
                    stat = entity.entityStats.GetSetStat(ac.statName, ac.defaultValue, ac.defaultValue);
                    v = stat.max_mod;
                    break;

                default: continue;
            }
            
            switch(ac.changeType)
            {
                case ChangeActionType.Add: v += ac.value; break;
                case ChangeActionType.Subtract: v -= ac.value; break;
                case ChangeActionType.Multiply: v *= ac.value; break;
                case ChangeActionType.Divide: v /= ac.value; break;
                case ChangeActionType.Round: v = RoundValue(v, Mathf.RoundToInt(ac.value)); break;
                case ChangeActionType.Floor: v = FloorValue(v, Mathf.RoundToInt(ac.value)); break;
                case ChangeActionType.Ceil: v = CeilValue(v, Mathf.RoundToInt(ac.value)); break;
                default: v = ac.value; break;
            }

            switch (ac.statType)
            {
                case StatType.Attribute_Base: attr.value_raw = v; break;
                case StatType.Attribute_Mod: attr.value_mod = v; break;
                case StatType.Stat_Value: stat.value = v; break;
                case StatType.Stat_Max: stat.max_raw = v; break;
                case StatType.Stat_MaxMod: stat.max_mod = v; break;
            }
        }
    }

    int GetDecimalMult(int count = 0)
    {
        int m = 1;
        for (int i = 0; i < count; i++)
        { m *= 10; }
        return m;
    }
    float RoundValue(float value, int decimals)
    {
        int m = GetDecimalMult(decimals);
        return Mathf.Round(value * m) / m;
    }
    float FloorValue(float value, int decimals)
    {
        int m = GetDecimalMult(decimals);
        return Mathf.Floor(value * m) / m;
    }
    float CeilValue(float value, int decimals)
    {
        int m = GetDecimalMult(decimals);
        return Mathf.Ceil(value * m) / m;
    }

    [System.Serializable]
    public class StatChange
    {
        public string statName = "";
        public float value = 0, defaultValue = 0;
        public ChangeActionType changeType = 0;
        public StatType statType = 0;
        public StatChange(string stat_name, float value, StatType stat_type = 0, ChangeActionType change_type = 0, float default_value = 0 )
        {
            statName = stat_name;
            this.value = value;
            defaultValue = default_value;
            changeType = change_type;
            statType = stat_type;
        }
    }
}
