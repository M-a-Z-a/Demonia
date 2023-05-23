using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttributeChanger : MonoBehaviour
{
    public enum AttributeChangeType { Set, Add, Subtract, Multiply, Divide, Round, Floor, Ceil }
    [SerializeField] List<AttrChange> attributeChanges;

    public void ApplyAttributes(CollectRelay crel)
    {
        //if (!entity?.entityStats) return;
        EntityStats.Attribute attr;
        EntityStats estats = crel.ent.GetComponent<EntityStats>();
        float v;
        foreach (AttrChange ac in attributeChanges)
        {
            attr = estats.GetSetAttribute(ac.attributeName, ac.defaultValue);
            v = attr.value;
            switch(ac.changeType)
            {
                case AttributeChangeType.Add: v += ac.value; break;
                case AttributeChangeType.Subtract: v -= ac.value; break;
                case AttributeChangeType.Multiply: v *= ac.value; break;
                case AttributeChangeType.Divide: v /= ac.value; break;
                case AttributeChangeType.Round: v = RoundValue(v, Mathf.RoundToInt(ac.value)); break;
                case AttributeChangeType.Floor: v = FloorValue(v, Mathf.RoundToInt(ac.value)); break;
                case AttributeChangeType.Ceil: v = CeilValue(v, Mathf.RoundToInt(ac.value)); break;
                default: v = attr.value; break;
            }
            estats.SetAttribute(ac.attributeName, v);
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
    public class AttrChange
    {
        public string attributeName = "";
        public float value = 0, defaultValue = 0;
        public AttributeChangeType changeType = 0;
        public AttrChange(string attr_name, float value, AttributeChangeType change_type = 0, float default_value = 0 )
        {
            attributeName = attr_name;
            this.value = value;
            defaultValue = default_value;
            changeType = change_type;
        }
    }
}
