using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public partial class MatAnimatorExtras
{
    //string filename = Path.Combine(Application.persistentDataPath, modelFileNmae + SAVE_FILE);
    public static Dictionary<string, JsonAtlas> jsonAtlas = new Dictionary<string, JsonAtlas>();

    public static JsonAnimation[] GetJsonAnimation(string path)
    {
        TextAsset jsta = Resources.Load<TextAsset>(path);
        JsonAnimation[] js = JsonUtility.FromJson<JsonAnimation[]>(jsta.text);
        return js;
    }
    public static JsonAnimation[] GetJsonAnimationOverwrite(string path, JsonAnimation[] jsanim)
    {
        TextAsset jsta = Resources.Load<TextAsset>(path);
        JsonUtility.FromJsonOverwrite(jsta.text, jsanim);
        return jsanim;
    }

    public static JsonAtlas GetJsonAtlas(string path)
    {
        TextAsset jsta = Resources.Load<TextAsset>(path);
        JsonAtlas js = JsonUtility.FromJson<JsonAtlas>(jsta.text);
        return js;
    }
    public static JsonAtlas GetJsonAtlasOverwrite(string path, JsonAtlas jsatlas)
    {
        TextAsset jsta = Resources.Load<TextAsset>(path);
        JsonUtility.FromJsonOverwrite(jsta.text, jsatlas);
        return jsatlas;
    }


    

    [System.Serializable]
    public class JsonAtlas
    {
        public float[] textureSize;
        public Frame[] frames; 
        
        [System.Serializable]
        public class Frame
        {
            public string id;
            public float[] position, size;
        }
    }

    [System.Serializable]
    public class JsonAnimation
    {
        public string name;
        public bool loop;
        public Frame[] frames;

        [System.Serializable]
        public class Frame 
        {
            public string id;
            public float time;
            public string[] flags;
        }
    }

}
