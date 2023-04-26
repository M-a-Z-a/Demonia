using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class MatAnimExtras
{
    [System.Serializable]
    public class MatAnimation
    {
        [SerializeField] string id;
        [SerializeField] float duration;
        [SerializeField] Dictionary<string, MatFrame> frames;
        [SerializeField] List<string> frameNames = new(), flags = new();
        [SerializeField] bool loop;

        public string ID { get => id; }
        public float Duration { get => duration; }
        public int FrameCount { get => frames.Count; }
        public List<string> FrameNames { get => frameNames; }
        public bool Loop { get => loop; }
        public List<string> Flags { get => flags; }



        public MatAnimation(string id, bool loop, params MatFrame[] frames)
        {
            this.id = id;
            this.frames = new();
            this.loop = loop;
            duration = 0;
            foreach (MatFrame f in frames)
            {
                AddFrame(f);
                duration += f.Time;
            }
        }


        public static bool FromJsonAnim(JsonAnim json_anim, out MatAnimation matAnim)
        {
            List<MatFrame> frms = new();
            MatFrame tmp;
            for (int i = 0; i < json_anim.frames.Length; i++)
            {
                if (MatFrame.FromJsonAnimFrame(json_anim.frames[i], out tmp))
                {
                    tmp.SetFlags(json_anim.frames[i].flags);
                    frms.Add(tmp); 
                }
            }

            matAnim = new(json_anim.id, json_anim.loop, frms.ToArray());
            if (json_anim.flags != null) matAnim.SetFlags(json_anim.flags);
            return true;
        }
        public void SetFlags(params string[] flags)
        { this.flags = new(flags); }
        public bool HasFlag(string flag)
        { return flags.Contains(flag); }

        public bool TryGetFrame(string id, out MatFrame frame_out)
        {
            return frames.TryGetValue(id, out frame_out);
        }
        public MatFrame GetFrame(string id)
        {
            TryGetFrame(id, out MatFrame mfram);
            return mfram;
        }

        public MatFrame GetFrame(float time, out float total_time)
        {
            total_time = 0;
            float t = 0;
            for (int i = 0; i < frameNames.Count; i++)
            {
                MatFrame f = frames[frameNames[i]];
                total_time = t;
                t += f.Time;
                if (time < t)
                { return f; }
            }
            return null;
        }

        public delegate int SortDelegate(MatFrame a, MatFrame b);
        public void SortByNameAscending()
        { frameNames.Sort((a, b) => { return a.CompareTo(b); }); }
        public void SortByNameDescending()
        { frameNames.Sort((a, b) => { return -a.CompareTo(b); }); }

        public bool AddFrame(MatFrame frame, bool replace = false)
        {
            int i = _AddFrame(frame, replace);
            switch (i)
            {
                case 1:
                    frameNames.Add(frame.ID);
                    return true;
                case 2:
                    return true;
                default:
                    return false;
            }
        }
        public int _AddFrame(MatFrame frame, bool replace = false)
        {
            if (frames.ContainsKey(frame.ID))
            {
                if (replace)
                {
                    frames[frame.ID] = frame;
                    return 2;
                }
                return 0;
            }
            frames.Add(frame.ID, frame);
            return 1;
        }
        public bool InsertFrame(int index, MatFrame frame)
        {
            int i = _AddFrame(frame);
            switch (i)
            {
                case 1:
                    frameNames.Insert(index, frame.ID);
                    return true;
                case 2:
                    return true;
                default:
                    return false;
            }
        }


        public bool RemoveFrame(int index)
        {
            if (index < 0 || index >= frameNames.Count || index >= frameNames.Count) return false;
            frames.Remove(frameNames[index]);
            frameNames.RemoveAt(index);
            return true;
        }
        public bool RemoveFrame(string id)
        {
            if (!frameNames.Contains(id)) return false;
            frames.Remove(id);
            frameNames.Remove(id);
            return true;
        }

        public int RemoveFrames(params int[] indexes)
        {
            int rcount = 0;
            List<int> ilist = new(indexes);
            ilist.Sort();
            for (int i = ilist.Count - 1; i > -1; i--)
            {
                if (RemoveFrame(i))
                { rcount++; }
            }
            return rcount;
        }
        public int RemoveFrames(params string[] ids)
        {
            int rcount = 0;
            for (int i = 0; i < ids.Length; i++)
            {
                if (RemoveFrame(ids[i]))
                { rcount++; }
            }
            return rcount;
        }

        void UpdateFrameNames()
        {
            frameNames = new();
            foreach (string k in frames.Keys)
            { frameNames.Add(k); }
        }

        public MatFrame this[int index]
        { get => frames[frameNames[index]]; }
        public MatFrame this[string name]
        { get => frames[name]; }

    }


    [System.Serializable]
    public class MatFrame
    {
        [SerializeField] string id;
        [SerializeField] float time;
        [SerializeField] Rect _rect;
        [SerializeField] Vector2 _pivot;
        [SerializeField] List<string> flags;
        public string ID { get => id; set => SetID(value); }
        public float Time { get => time; set => SetTime(value); }
        public List<string> Flags { get => flags; }
        public Rect rect { get => _rect; set => SetRect(value); }
        public Vector2 pivot { get => _pivot; set => SetPivot(value); }
        public Vector2 topleft { get => new Vector2(_rect.xMin, _rect.yMax); }
        public Vector2 topright { get => new Vector2(_rect.xMax, _rect.yMax); }
        public Vector2 bottomleft { get => new Vector2(_rect.xMin, _rect.yMin); }
        public Vector2 bottomright { get => new Vector2(_rect.xMax, _rect.yMin); }

        void SetID(string id)
        { this.id = id; }
        void SetRect(Rect rect)
        { _rect = rect; }
        void SetTime(float value)
        { time = Mathf.Max(value, 0); }
        void SetPivot(Vector2 value)
        { _pivot = value; }
        public void SetFlags(params string[] flags)
        { this.flags = new(flags); }

        public MatFrame(string name, float time, Rect rect, Vector2 pivot, params string[] flags)
        {
            SetID(name);
            SetTime(time);
            SetRect(rect);
            SetPivot(pivot);
            SetFlags(flags);
        }

        public static bool FromJsonAnimFrame(JsonAnim.Frame frame, out MatFrame matFrame)
        {
            matFrame = null;
            if (GetAtlasFrame(frame.id, out JsonAtlas.Frame jframe))
            {
                matFrame = new MatFrame(
                    frame.id, frame.time, 
                    new Rect(jframe.position[0], jframe.position[1], jframe.size[0], jframe.size[1]), 
                    new Vector2(jframe.pivot[0], jframe.pivot[1])
                    );
                return true;
            }
            return false;
        }

        public bool HasFlag(string flag)
        { return flags.Contains(flag); }


    }
}