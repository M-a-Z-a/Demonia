using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class SplineController : MonoBehaviour
{

    Dictionary<string, AnimationCurve> _curves;
    public Dictionary<string, AnimationCurve> curves { get => _curves; }


    public bool AddCurve(string name, out AnimationCurve curve_out)
    {
        curve_out = default;
        if (_curves.ContainsKey(name))
        { return false; }
        _curves.Add(name, curve_out);
        return true;
    }
    public bool AddCurve(string name, AnimationCurve curve)
    {
        foreach (string k in _curves.Keys)
        {
            if (k == name) return false;
            if (System.Object.ReferenceEquals(_curves[k], curve)) return false;
        }
        _curves.Add(name, curve);
        return true;
    }

    public bool RemoveCurve(string name)
    { return _curves.Remove(name); }
    public bool RemoveCurve(AnimationCurve curve)
    {
        foreach (string k in _curves.Keys)
        {
            if (System.Object.ReferenceEquals(curve, _curves[k]))
            { return _curves.Remove(k); }
        }
        return false;
    }


}

