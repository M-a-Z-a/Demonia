using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatFrame
{

    string _name;
    float _time;
    Rect _rect;
    public string name { get => _name; set => SetName(value); }
    public float time { get => _time; set => SetTime(value); }
    public Rect rect { get => _rect; set => SetRect(value); }
    public Vector2 topleft { get => new Vector2(_rect.xMin, _rect.yMax); }
    public Vector2 topright { get => new Vector2(_rect.xMax, _rect.yMax); }
    public Vector2 bottomleft { get => new Vector2(_rect.xMin, _rect.yMin); }
    public Vector2 bottomright { get => new Vector2(_rect.xMax, _rect.yMin); }

    void SetName(string name)
    { _name = name; }
    void SetRect(Rect rect)
    { _rect = rect; }
    void SetTime(float value)
    { _time = Mathf.Max(value, 0); }

    public MatFrame(string name, float time, Rect rect)
    {
        SetName(name);
        SetTime(time);
        SetRect(rect);
    }

}
