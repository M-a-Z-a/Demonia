using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatAnimation
{
    string _name;
    float _length;
    Dictionary<string, MatFrame> _frames;
    List<string> _frameNames;

    public string name { get => _name; set => _name = value; }
    public float length { get => _length; }
    public int frameCount { get => _frames.Count; }
    public string[] frameNames { get => _frameNames.ToArray(); }



    public MatAnimation(string name, params MatFrame[] frames)
    {
        _name = name;
        _frames = new();
        _length = 0;
        foreach (MatFrame f in frames)
        { 
            AddFrame(f);
            _length += f.time;
        }
    }


    public bool TryGetFrame(string name, out MatFrame frame_out)
    {
        return _frames.TryGetValue(name, out frame_out);
    }
    public MatFrame GetFrame(string name)
    {
        TryGetFrame(name, out MatFrame mfram);
        return mfram;
    }

    public MatFrame GetFrame(float time)
    {
        float t = 0;
        for (int i = 0; i < _frameNames.Count; i++)
        {
            MatFrame f = _frames[_frameNames[i]];
            t += f.time;
            if (time < t)
            { return f; }
        }
        return null;
    }

    public delegate int SortDelegate(MatFrame a, MatFrame b);
    public void SortByNameAscending()
    { _frameNames.Sort((a,b) => { return a.CompareTo(b); }); }
    public void SortByNameDescending()
    { _frameNames.Sort((a, b) => { return -a.CompareTo(b); }); }

    public bool AddFrame(MatFrame frame, bool replace = false)
    {
        int i = _AddFrame(frame, replace);
        switch(i)
        {
            case 1:
                _frameNames.Add(frame.name);
                return true;
            case 2:
                return true;
            default: 
                return false;
        }
    }
    public int _AddFrame(MatFrame frame, bool replace = false)
    {
        if (_frames.ContainsKey(frame.name))
        {
            if (replace)
            { 
                _frames[frame.name] = frame;
                return 2; 
            }
            return 0;
        }
        _frames.Add(frame.name, frame);
        return 1;
    }
    public bool InsertFrame(int index, MatFrame frame)
    {
        int i = _AddFrame(frame);
        switch(i)
        {
            case 1:
                _frameNames.Insert(index, frame.name);
                return true;
            case 2:
                return true;
            default:
                return false;
        }
    }


    public bool RemoveFrame(int index)
    {
        if (index < 0 || index >= _frameNames.Count || index >= _frameNames.Count) return false;
        _frames.Remove(_frameNames[index]);
        _frameNames.RemoveAt(index);
        return true;
    }
    public bool RemoveFrame(string name)
    {
        if (!_frameNames.Contains(name)) return false;
        _frames.Remove(name);
        _frameNames.Remove(name);
        return true;
    }

    public int RemoveFrames(params int[] indexes)
    {
        int rcount = 0;
        List<int> ilist = new(indexes);
        ilist.Sort();
        for (int i = ilist.Count-1; i > -1; i--)
        {
            if (RemoveFrame(i))
            { rcount++; }
        }
        return rcount;
    }
    public int RemoveFrames(params string[] names)
    {
        int rcount = 0;
        for (int i = 0; i < names.Length; i++)
        { 
            if (RemoveFrame(names[i]))
            { rcount++; }
        }
        return rcount;
    }

    void UpdateFrameNames()
    {
        _frameNames = new();
        foreach (string k in _frames.Keys)
        { _frameNames.Add(k); }
    }

    public MatFrame this[int index]
    { get => _frames[frameNames[index]]; }
    public MatFrame this[string name]
    { get => _frames[name]; }

}
