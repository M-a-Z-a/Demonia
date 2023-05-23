using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public partial class MatAnimExtras
{
    
    public static Dictionary<string, JsonAnimGroup> jsonAnimGroupDict = new();
    public static Dictionary<string, JsonAtlas> jsonAtlasDict = new();

    static Regex framePathSplit = new Regex(@"[/\\]");
    public static bool GetAtlasFrame(string frame_path, out JsonAtlas.Frame atlasFrame)
    {
        atlasFrame = null;
        string[] strarr = framePathSplit.Split(frame_path);
        if (strarr.Length < 2)
        { 
            Debug.LogWarning($"MatAnimExtras > GetAtlasFrame: Invalid path \"{frame_path}\""); 
            return false; 
        }
        if (jsonAtlasDict.TryGetValue(strarr[0], out JsonAtlas jatlas))
        {
            for (int i = 0; i < jatlas.frames.Length; i++)
            {
                if (jatlas.frames[i].id == strarr[1])
                { atlasFrame = jatlas.frames[i]; return true; }
            }
        }
        Debug.LogWarning($"MatAnimExtras > GetAtlasFrame: Frame \"{frame_path}\" not found");
        return false;
    }


    public static bool LoadJsonAnimGroup(string json_path, out JsonAnimGroup agroup)
    {
        TextAsset tasset = Resources.Load(json_path) as TextAsset;
        if (tasset == null)
        {
            Debug.LogWarning($"MatAnimExtras > LoadJsonAnimGroup: Invalid load path <TextAsset>\"{json_path}\"");
            agroup = null; return false; 
        }
        agroup = JsonUtility.FromJson<JsonAnimGroup>(tasset.text);
        if (agroup == null)
        {
            Debug.LogWarning($"MatAnimExtras > LoadJsonAnimGroup: Can't load Json <JsonAnimGroup>\"{tasset.name}\"");
            return false; 
        }
        if (jsonAnimGroupDict.ContainsKey(agroup.id))
        { jsonAnimGroupDict[agroup.id] = agroup; }
        else
        { jsonAnimGroupDict.Add(agroup.id, agroup); }
        return true;
    }
    public static bool LoadJsonAtlas(string json_path, out JsonAtlas atlas)
    {
        TextAsset tasset = Resources.Load(json_path) as TextAsset;
        if (tasset == null)
        {
            Debug.LogWarning($"MatAnimExtras > LoadJsonAtlas: Invalid load path <TextAsset>\"{json_path}\""); 
            atlas = null; return false; 
        }
        atlas = JsonUtility.FromJson<JsonAtlas>(tasset.text);
        if (atlas == null)
        {
            Debug.LogWarning($"MatAnimExtras > LoadJsonAtlas: Can't load Json <JsonAtlas>\"{tasset.name}\"");
            return false;
        }
        if (jsonAtlasDict.ContainsKey(atlas.id))
        { atlas.SetFramesToAtlasScale(); jsonAtlasDict[atlas.id] = atlas; }
        else
        { atlas.SetFramesToAtlasScale(); jsonAtlasDict.Add(atlas.id, atlas); }

        return true;
    }

    public static bool GetOrLoadJsonAtlas(string atlas_name, string json_path, out JsonAtlas atlas)
    {
        if (jsonAtlasDict.TryGetValue(atlas_name, out atlas))
        { return true; }
        if (LoadJsonAtlas(json_path, out atlas))
        { return true; }
        return false;
    }
    public static bool GetOrLoadJsonAnimGroup(string group_name, string json_path, out JsonAnimGroup agroup)
    {
        if (jsonAnimGroupDict.TryGetValue(group_name, out agroup))
        { return true; }
        if (LoadJsonAnimGroup(json_path, out agroup))
        { return true; }
        return false;
    }


    [System.Serializable]
    public class JsonAnimGroup
    {
        public string id;
        public JsonAnim[] animations;
    }

    [System.Serializable]
    public class JsonAnim
    {
        public string id;
        public Frame[] frames;
        public bool loop;
        public string[] flags;
        [System.Serializable]
        public class Frame
        {
            public string id;
            public float time;
            public string[] flags;
        }
    }

    [System.Serializable]
    public class JsonAtlas
    {
        public string id;
        public float[] sample_size, frame_size;
        public Frame[] frames;
        bool scaled = false;
        
        [System.Serializable]
        public class Frame
        {
            public string id;
            public float[] position, size, pivot;
            public int[] frame_position;
            public override string ToString()
            {
                return $"{{" +
                    $"id: {id}, position[{position.Length}]:[{string.Join(',', position)}], " +
                    $"size[{size.Length}]:[{string.Join(',', size)}], " +
                    $"pivot[{pivot.Length}]:[{string.Join(',', pivot)}]}}";
                //, frame_position[{frame_position.Length}]:[{string.Join(',', frame_position)}]}}"; 
            }
        }

        public Vector2 GetSizeNormal()
        { return new Vector2(1f / sample_size[0], 1f / sample_size[1]); }
        Frame FrameSizeToAtlas(Frame frame)
        {
            Frame f = new Frame() { id = frame.id, position = frame.position, size = frame.size, pivot = frame.pivot, frame_position = frame.frame_position };

            if (frame_size != null && frame_size.Length == 2 && 
                f.frame_position != null && f.frame_position.Length == 2 && 
                frame_size[0] > 0 && frame_size[1] > 0)
            {
                f.position = new float[] { f.frame_position[0] * frame_size[0] / sample_size[0], 1f - (f.frame_position[1]+1) * frame_size[1] / sample_size[1] };
                f.pivot[0] = f.pivot[0] / frame_size[0] - 0.5f; f.pivot[1] = f.pivot[1] / frame_size[1] - 0.5f;
                f.size = new float[] { frame_size[0] / sample_size[0], frame_size[1] / sample_size[1] };
            }
            else
            {
                f.position[0] /= sample_size[0]; f.position[1] = 1f - (f.position[1] / sample_size[1]);
                f.pivot[0] = f.pivot[0] / f.size[0] - 0.5f; f.pivot[1] = f.pivot[1] / f.size[1] - 0.5f;
                f.size[0] /= sample_size[0]; f.size[1] /= sample_size[1];
            }

            return f;
        }
        public void SetFramesToAtlasScale()
        {
            if (scaled) return;
            for(int i = 0; i < frames.Length; i++)
            { frames[i] = FrameSizeToAtlas(frames[i]); }
            scaled = true;
        }
    }

}
