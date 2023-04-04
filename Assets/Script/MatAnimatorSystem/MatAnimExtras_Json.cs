using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MatAnimExtras
{




    [System.Serializable]
    public class JsonAnim
    {
        public Frame[] frames;
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
        public string name;
        public float[] textureSize;
        [System.Serializable]
        public class Frame
        {
            public string id;
            public float[] position, size;
        }
    }

}
