using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SaveManager : MonoBehaviour
{
    static string F_SAVE_GENERAL = "general.json", F_SAVE_PLAYERDATA = "player.json";

    [SerializeField] SaveDataAsset saveData;

    public static string currentSaveName = "Save0";
    public static SaveManager instance { get; protected set; }

    static string SAVE_DIRECTORY { get => $"{Application.persistentDataPath}/saves"; }
    static string GetSavePath(string file_name = "") 
    {
        if (!ValidateString(currentSaveName)) return null;
        return $"{SAVE_DIRECTORY}/{currentSaveName}{(ValidateString(file_name)?$"/{file_name}":"")}"; 
    }
    static bool TryGetSavePath(string file_name, out string path_out)
    {
        path_out = "";
        if (!ValidateString(currentSaveName)) return false;
        path_out = $"{SAVE_DIRECTORY}/{currentSaveName}{(ValidateString(file_name) ? $"/{file_name}" : "")}";
        return true;
    }


    public static string[] GetSaves(bool full_path = false, bool log_output = false)
    {
        string[] sarr = Directory.GetDirectories(SAVE_DIRECTORY);
        if (!full_path)
        {
            for (int i = 0; i < sarr.Length; i++)
            { sarr[i] = sarr[i].Split(new char[] { '/', '\\' }).Last(); }
        }
        if (log_output) Debug.Log($"Saves: {string.Join(',', sarr)}");
        return sarr;
    }
    public static bool SaveExists(string save_name = null)
    {
        save_name = save_name ?? currentSaveName;
        string[] sarr = GetSaves();
        if (sarr.Length == 0) return false;
        return sarr.Contains(save_name);
    }
    public static bool DeleteSave(string save_name = null, bool clear_temp_save_data = true)
    {
        save_name = save_name ?? currentSaveName;
        string[] sarr = GetSaves();
        if (!sarr.Contains(save_name)) return false;
        Directory.Delete($"{SAVE_DIRECTORY}/{save_name}", true);
        if (clear_temp_save_data) ClearTempSaveData();
        return true;
    }
    public static void ClearTempSaveData(bool clear_all = true)
    { instance.saveData.Clear(clear_all); }

    private void Awake()
    { instance = this; }


    public static SaveDataAssetEntityStats GetPlayerEntityStats()
    { return instance.saveData.playerEntityStats; }

    public static void SetBool(string key, bool value) { instance._SetBool(key, value); }
    public static void SetInt(string key, int value) { instance._SetInt(key, value); }
    public static void Setfloat(string key, float value) { instance._SetFloat(key, value); }
    public static void SetString(string key, string value) { instance._SetString(key, value); }
    public static void SetVector(string key, Vector3 value) { instance._SetVector(key, value); }

    void _SetBool(string key, bool value)
    {
        if (saveData.boolValues.ContainsKey(key))
        { saveData.boolValues[key] = value; return; }
        saveData.boolValues.Add(key, value);
    }
    void _SetInt(string key, int value)
    {
        if (saveData.intValues.ContainsKey(key))
        { saveData.intValues[key] = value; return; }
        saveData.intValues.Add(key, value);
    }
    void _SetFloat(string key, float value)
    {
        if (saveData.floatValues.ContainsKey(key))
        { saveData.floatValues[key] = value; return; }
        saveData.floatValues.Add(key, value);
    }
    void _SetString(string key, string value)
    {
        if (saveData.stringValues.ContainsKey(key))
        { saveData.stringValues[key] = value; return; }
        saveData.stringValues.Add(key, value);
    }
    void _SetVector(string key, Vector3 value)
    {
        if (saveData.vectorValues.ContainsKey(key))
        { saveData.vectorValues[key] = value; return; }
        saveData.vectorValues.Add(key, value);
    }



    public static bool GetBool(string key, out bool value_out) { return instance._GetBool(key, out value_out); }
    public static bool GetInt(string key, out int value_out) { return instance._GetInt(key, out value_out); }
    public static bool GetFloat(string key, out float value_out) { return instance._GetFloat(key, out value_out); }
    public static bool GetString(string key, out string value_out) { return instance._GetString(key, out value_out); }
    public static bool GetVector(string key, out Vector3 value_out) { return instance._GetVector(key, out value_out); }

    bool _GetBool(string key, out bool value_out)
    { return saveData.boolValues.TryGetValue(key, out value_out); }
    bool _GetInt(string key, out int value_out)
    { return saveData.intValues.TryGetValue(key, out value_out); }
    bool _GetFloat(string key, out float value_out)
    { return saveData.floatValues.TryGetValue(key, out value_out); }
    bool _GetString(string key, out string value_out)
    { return saveData.stringValues.TryGetValue(key, out value_out); }
    bool _GetVector(string key, out Vector3 value_out)
    { return saveData.vectorValues.TryGetValue(key, out value_out); }


    public static bool SaveDataToFile()
    {
        Debug.Log($"Saving {currentSaveName}...");

        if (!GetSaveDirectory(out string dpath)) return false;
        string general_save = GetSavePath(F_SAVE_GENERAL);
        string player_save = GetSavePath(F_SAVE_PLAYERDATA);

        string general_json = instance.saveData.ToJson();
        string player_json = instance.saveData.playerEntityStats.ToJson();

        int saved = 0;
        if (WriteToFile(dpath, F_SAVE_GENERAL, general_json)) saved += 1;
        if (WriteToFile(dpath, F_SAVE_PLAYERDATA, player_json)) saved += 2;

        if (saved > 0)
        { Debug.Log($"{currentSaveName} saved!"); return true; }
        else
        { Debug.Log($"{currentSaveName} not saved?"); return false; }
    }
    public static bool LoadDataFromFile()
    {
        Debug.Log($"Loading {currentSaveName}...");

        if (!TryGetSavePath("", out string spath)) return false;
        if (!Directory.Exists(spath)) return false;

        int loaded = 0;
        if (ReadFromFile(GetSavePath(F_SAVE_GENERAL), out string json))
        { instance.saveData.FromJson(json); loaded += 1; }
        if (ReadFromFile(GetSavePath(F_SAVE_PLAYERDATA), out json))
        { instance.saveData.playerEntityStats.FromJson(json); loaded += 1; }

        if (loaded > 0)
        { Debug.Log($"{currentSaveName} loaded!"); return true; }
        else
        { Debug.Log($"{currentSaveName} not loaded?"); return false; }

    }


    static bool ValidateString(string str)
    { return !(str == null || str == ""); }
    static bool GetSaveDirectory(out string path_to_directory)
    {
        Debug.Log($"Checking directory...");
        path_to_directory = "";
        if (!ValidateString(currentSaveName)) return false;
        string s = $"{SAVE_DIRECTORY}/{currentSaveName}";
        Debug.Log(s);
        Directory.CreateDirectory(s);
        path_to_directory = s;
        bool bexists = Directory.Exists(path_to_directory);
        Debug.Log($"directory exists? {bexists}");
        return bexists;
    }
    static bool WriteToFile(string directory, string file, string content)
    {
        if (!Directory.Exists(directory)) return false;
        string file_path = $"{directory}/{file}";
        if (File.Exists(file_path))
        { File.WriteAllText(file_path, content); return true; }
        File.Create(file_path).Close();
        File.WriteAllText(file_path, content);
        return true;
    }
    static bool ReadFromFile(string file_path, out string contents)
    {
        if (!File.Exists(file_path)) { contents = ""; return false; }
        contents = File.ReadAllText(file_path);
        return true;
    }
}


[CreateAssetMenu(fileName = "SaveDataAsset", menuName = "SaveData/SaveData")]
public class SaveDataAsset : ScriptableObject
{
    public SaveDataAssetEntityStats playerEntityStats;

    public Dictionary<string, bool> boolValues = new();
    public Dictionary<string, int> intValues = new();
    public Dictionary<string, float> floatValues = new();
    public Dictionary<string, string> stringValues = new();
    public Dictionary<string, Vector3> vectorValues = new();

    public string ToJson()
    {
        SaveDataAssetWrapper wrapdata = new();
        wrapdata.boolValues.Set(boolValues);
        wrapdata.intValues.Set(intValues);
        wrapdata.floatValues.Set(floatValues);
        wrapdata.stringValues.Set(stringValues);
        wrapdata.vectorValues.Set(vectorValues);
        return JsonUtility.ToJson(wrapdata, true);
    }
    public void FromJson(string json)
    {
        SaveDataAssetWrapper wrapdata = JsonUtility.FromJson<SaveDataAssetWrapper>(json);
        boolValues = wrapdata.boolValues.GetDict();
        intValues = wrapdata.intValues.GetDict();
        floatValues = wrapdata.floatValues.GetDict();
        stringValues = wrapdata.stringValues.GetDict();
        vectorValues = wrapdata.vectorValues.GetDict();
    }
    public void Clear(bool clear_all = false)
    {
        boolValues = new();
        intValues = new();
        floatValues = new();
        stringValues = new();
        vectorValues = new();
        if (clear_all)
        {
            playerEntityStats.Clear();
        }
    }

    [System.Serializable]
    public class SaveDataAssetWrapper
    {
        public SaveDataDictWrapperBool boolValues = new();
        public SaveDataDictWrapperInt intValues = new();
        public SaveDataDictWrapperFloat floatValues = new();
        public SaveDataDictWrapperString stringValues = new();
        public SaveDataDictWrapperVector vectorValues = new();
    }
    [System.Serializable]
    public class SaveDataDictWrapperBool
    {
        public List<string> keys;
        public List<bool> values;
        public void Set(Dictionary<string, bool> bool_dict)
        {
            keys = new();
            values = new();
            foreach(string k in bool_dict.Keys)
            { keys.Add(k); values.Add(bool_dict[k]); }
        }
        public Dictionary<string, bool> GetDict()
        {
            Dictionary<string, bool> dict = new(); ;
            for (int i = 0; i < keys.Count; i++)
            { dict.Add(keys[i], values[i]); }
            return dict;
        }
    }

    [System.Serializable]
    public class SaveDataDictWrapperInt
    {
        public List<string> keys;
        public List<int> values;
        public void Set(Dictionary<string, int> int_dict)
        {
            keys = new();
            values = new();
            foreach (string k in int_dict.Keys)
            { keys.Add(k); values.Add(int_dict[k]); }
        }
        public Dictionary<string, int> GetDict()
        {
            Dictionary<string, int> dict = new(); ;
            for (int i = 0; i < keys.Count; i++)
            { dict.Add(keys[i], values[i]); }
            return dict;
        }
    }

    [System.Serializable]
    public class SaveDataDictWrapperFloat
    {
        public List<string> keys;
        public List<float> values;
        public void Set(Dictionary<string, float> float_dict)
        {
            keys = new();
            values = new();
            foreach (string k in float_dict.Keys)
            { keys.Add(k); values.Add(float_dict[k]); }
        }
        public Dictionary<string, float> GetDict()
        {
            Dictionary<string, float> dict = new(); ;
            for (int i = 0; i < keys.Count; i++)
            { dict.Add(keys[i], values[i]); }
            return dict;
        }
    }
    [System.Serializable]
    public class SaveDataDictWrapperString
    {
        public List<string> keys;
        public List<string> values;
        public void Set(Dictionary<string, string> string_dict)
        {
            keys = new();
            values = new();
            foreach (string k in string_dict.Keys)
            { keys.Add(k); values.Add(string_dict[k]); }
        }
        public Dictionary<string, string> GetDict()
        {
            Dictionary<string, string> dict = new(); ;
            for (int i = 0; i < keys.Count; i++)
            { dict.Add(keys[i], values[i]); }
            return dict;
        }
    }

    [System.Serializable]
    public class SaveDataDictWrapperVector
    {
        public List<string> keys;
        public List<Vector3> values;
        public void Set(Dictionary<string, Vector3> vector_dict)
        {
            keys = new();
            values = new();
            foreach (string k in vector_dict.Keys)
            { keys.Add(k); values.Add(vector_dict[k]); }
        }
        public Dictionary<string, Vector3> GetDict()
        {
            Dictionary<string, Vector3> dict = new(); ;
            for (int i = 0; i < keys.Count; i++)
            { dict.Add(keys[i], values[i]); }
            return dict;
        }
    }

}



    /*

    [System.Serializable] public enum ValueType { STRING, INTEGER, FLOAT }
    static Dictionary<string, SaveGroup> saveData = new();

    public static bool RemoveValue(string id)
    { return saveData.Remove(id); }
    public static void SetJson(string group, string id, object obj, bool allow_type_change = false)
    {
        SetString(group, id, JsonUtility.ToJson(obj), allow_type_change);
    }
    public static void SetString(string group, string id, string value, bool allow_type_change = false)
    {
        if (saveData.TryGetValue(group, out SaveGroup sgroup))
        {
            for (int i = 0; i < sgroup.values.Count; i++)
            {
                if (sgroup.values[i].id == id)
                {
                    if (!allow_type_change && sgroup.values[i].value_type != ValueType.STRING) return;
                    sgroup.values[i].stringValue = value;
                    sgroup.hasChanged = true;
                    return; 
                }
            }
            sgroup.values.Add(new SaveValue(id, value));
            sgroup.hasChanged = true;
            return;
        }
        sgroup = new SaveGroup(group);
        sgroup.values.Add(new SaveValue(id, value));
        sgroup.hasChanged = true;
        saveData.Add(group, sgroup);
    }
    public static void SetInt(string group, string id, int value, bool allow_type_change = false)
    {
        if (saveData.TryGetValue(group, out SaveGroup sgroup))
        {
            for (int i = 0; i < sgroup.values.Count; i++)
            {
                if (sgroup.values[i].id == id)
                {
                    if (!allow_type_change && sgroup.values[i].value_type != ValueType.INTEGER) return;
                    sgroup.values[i].intValue = value;
                    sgroup.hasChanged = true; 
                    return; 
                }
            }
            sgroup.values.Add(new SaveValue(id, value));
            sgroup.hasChanged = true;
            return;
        }
        sgroup = new SaveGroup(group);
        sgroup.values.Add(new SaveValue(id, value));
        sgroup.hasChanged = true;
        saveData.Add(group, sgroup);
    }
    public static void SetFloat(string group, string id, float value, bool allow_type_change = false)
    {
        if (saveData.TryGetValue(group, out SaveGroup sgroup))
        {
            for (int i = 0; i < sgroup.values.Count; i++)
            {
                if (sgroup.values[i].id == id)
                {
                    if (!allow_type_change && sgroup.values[i].value_type != ValueType.FLOAT) return;
                    sgroup.values[i].floatValue = value;
                    sgroup.hasChanged = true; 
                    return; 
                }
            }
            sgroup.values.Add(new SaveValue(id, value));
            sgroup.hasChanged = true;
            return;
        }
        sgroup = new SaveGroup(group);
        sgroup.values.Add(new SaveValue(id, value));
        sgroup.hasChanged = true;
        saveData.Add(group, sgroup);
    }


    public static SaveValue GetValue(string group, string id)
    {
        if (saveData.TryGetValue(group, out SaveGroup sgroup))
        { 
            for (int i = 0; i < sgroup.values.Count; i++)
            { if (sgroup.values[i].id == id) return sgroup.values[i]; }
        }
        return null;
    }
    public static bool TryGetValue(string group, string id, out SaveValue out_value)
    {
        if (saveData.TryGetValue(group, out SaveGroup sgroup))
        {
            for (int i = 0; i < sgroup.values.Count; i++)
            { 
                if (sgroup.values[i].id == id)
                { out_value = sgroup.values[i]; return true; } 
            }
        }
        out_value = null;
        return false;
    }

    public static bool Save(string save_group = null, SaveMode save_mode = 0)
    {
        string spath = GetSavePath($"{save_group}.json");
        //Debug.Log($"Attempting to save group \"{save_group}\" to {spath}");
        if (!Directory.Exists(SAVE_DIRECTORY))
        {
            Debug.Log($"Directory does not exist, attempting to create...");
            Directory.CreateDirectory(SAVE_DIRECTORY); 
        }
        if (save_group != null)
        {
            Debug.Log($"Savegroup \"{save_group}\" exists...");
            if (!saveData.TryGetValue(save_group, out SaveGroup sgroup))
            { return false; }
            sgroup.Save(save_mode: save_mode);
            return true;
        }

        foreach (string k in saveData.Keys)
        { saveData[k].Save(save_mode: save_mode); }
        return true;
    }
    public static bool Load(string save_group = null)
    {
        string spath = GetSavePath($"{save_group}.json");
        Debug.Log($"Attempting to load \"{save_group}\" at {spath}");
        SaveGroup sgroup;
        if (!Directory.Exists(SAVE_DIRECTORY))
        { return false; }
        if (save_group != null && save_group != "")
        {
            if (!File.Exists(spath))
            { return false; }
            string json = File.ReadAllText(spath);
            if (saveData.TryGetValue(save_group, out SaveGroup cgroup))
            { 
                JsonUtility.FromJsonOverwrite(json, cgroup);
                cgroup.hasChanged = false;
            }
            else
            { 
                sgroup = JsonUtility.FromJson<SaveGroup>(json);
                sgroup.hasChanged = false;
                saveData.Add(save_group, sgroup);
            }
            Debug.Log($"SaveGroup \"{save_group}\" loaded at {spath}");
            return true;
        }
        return false;
    }


    [System.Serializable]
    public class SaveGroup
    {
        public string id = "";
        [System.NonSerialized] public bool hasChanged;
        public List<SaveValue> values;
        public SaveGroup(string id)
        { this.id = id; hasChanged = true; values = new(); }

        public bool Save(string save_group = null, SaveMode save_mode = 0, bool force_save = false)
        {
            if (!(hasChanged || force_save)) return false;
            save_group = save_group == null ? id : save_group;
            if (save_group == "") return false;
            string spath = GetSavePath($"{save_group}.json");
            Debug.Log($"Attempting to save group \"{save_group}\" to {spath}");
            if (!Directory.Exists(SAVE_DIRECTORY))
            { Directory.CreateDirectory(SAVE_DIRECTORY); }

            if (!saveData.TryGetValue(save_group, out SaveGroup sgroup))
            { return false; }
            string s;
            if (File.Exists(spath))
            {
                switch (save_mode)
                {
                    case SaveMode.Override:
                        s = JsonUtility.ToJson(sgroup);
                        File.WriteAllText(spath, s);
                        Debug.Log($"SaveGroup \"{save_group}\" saved at {spath}");
                        return true;
                    case SaveMode.Merge:
                    default: return false;
                }
            }
            s = JsonUtility.ToJson(sgroup);
            using (StreamWriter f = File.CreateText(spath))
            { f.Write(s); }
            sgroup.hasChanged = false;
            Debug.Log($"SaveGroup \"{save_group}\" saved at {spath}");
            return true;
        }
    }

    [System.Serializable]
    public class SaveValue
    {
        [SerializeField] ValueType type;
        [SerializeField] string value_string = "";
        [SerializeField] int value_int = 0;
        [SerializeField] float value_float = 0;

        public string id;
        public ValueType value_type { get => type; }
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
    }

    */
