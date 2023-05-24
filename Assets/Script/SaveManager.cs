using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveManager : MonoBehaviour
{

    SaveData saveData = new();

    public SaveValue RemoveValue(string id)
    { return saveData.RemoveValue(id); }
    public void SetString(string id, string value)
    {
        if (saveData.TryGetValue(id, out SaveValue v))
        { v.stringValue = value; return; }
        saveData.values.Add(new SaveValue(id, value));
        return;
    }
    public void SetInt(string id, int value)
    {
        if (saveData.TryGetValue(id, out SaveValue v))
        { v.intValue = value; return; }
        saveData.values.Add(new SaveValue(id, value));
        return;
    }
    public void SetFloat(string id, float value)
    {
        if (saveData.TryGetValue(id, out SaveValue v))
        { v.floatValue = value; return; }
        saveData.values.Add(new SaveValue(id, value));
        return;
    }
    public SaveValue GetValue(string id)
    {
        if (saveData.TryGetValue(id, out SaveValue v))
        { return v; }
        return null;
    }
    public bool TryGetValue(string id, out SaveValue out_value)
    {
        if (saveData.TryGetValue(id, out out_value))
        { return true; }
        return false;
    }


    public class SaveData
    {
        [SerializeField] public List<SaveValue> values;

        public SaveData(params SaveValue[] values)
        { this.values = new(values); }
        
        public string ToJson()
        { return JsonUtility.ToJson(this); }
        public static SaveData FromJson(string json)
        { return JsonUtility.FromJson<SaveData>(json); }
        public bool TryGetValue(string id, out SaveValue value_out)
        {
            foreach (SaveValue v in values)
            {
                if (v.id == id)
                {
                    value_out = v;
                    return true; 
                }
            }
            value_out = null;
            return false;
        }
        public SaveValue GetValue(string id)
        {
            foreach (SaveValue v in values)
            {
                if (v.id == id)
                { return v; }
            }
            return null;
        }
        public SaveValue RemoveValue(string id)
        {
            for (int i = 0; i < values.Count; i++)
            { 
                if (values[i].id == id)
                {
                    SaveValue v = values[i];
                    values.RemoveAt(i);
                    return v;
                }
            }
            return null;
        }
    }

    [System.Serializable]
    public class SaveValue
    {
        [System.Serializable] public enum ValueType { STRING, INTEGER, FLOAT }
        [SerializeField] ValueType type;
        [SerializeField] string value_string = "";
        [SerializeField] int value_int = 0;
        [SerializeField] float value_float = 0;

        public string id;
        public string stringValue { get => GetString(); set => SetString(value); }
        public int intValue { get => GetInt(); set => SetInt(value); }
        public float floatValue { get => GetFloat(); set => SetFloat(value); }


        public SaveValue(string id, float value)
        { this.id = id; SetFloat(value); }
        public SaveValue(string id, int value)
        { this.id = id; SetInt(value); }
        public SaveValue(string id, string value)
        { this.id = id; SetString(value); }

        int GetInt()
        {
            switch (type)
            {
                case ValueType.STRING: return int.Parse(value_string);
                case ValueType.INTEGER: return value_int;
                case ValueType.FLOAT: return Mathf.FloorToInt(value_float);
            }
            return 0;
        }
        float GetFloat()
        {
            switch (type)
            {
                case ValueType.STRING: return float.Parse(value_string);
                case ValueType.INTEGER: return value_int;
                case ValueType.FLOAT: return value_float;
            }
            return 0;
        }
        string GetString()
        {
            switch (type)
            {
                case ValueType.STRING: return value_string;
                case ValueType.INTEGER: return value_int.ToString();
                case ValueType.FLOAT: return value_float.ToString();
            }
            return "";
        }

        void SetString(string value)
        {
            ClearValues();
            type = ValueType.STRING;
            value_string = value;
        }
        void SetInt(int value)
        {
            ClearValues();
            type = ValueType.INTEGER;
            value_int = value;
        }
        void SetFloat(float value)
        {
            ClearValues();
            type = ValueType.FLOAT;
            value_float = value;
        }

        void ClearValues()
        {
            value_string = "";
            value_int = 0;
            value_float = 0;
        }

        public string ToJson()
        { return JsonUtility.ToJson(this, true); }
        public static SaveValue FromJson(string json)
        { return JsonUtility.FromJson<SaveValue>(json); }
    }

    
}
