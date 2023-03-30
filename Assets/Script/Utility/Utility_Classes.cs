using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// utility classes
public static partial class Utility
{
    public class Dir4
    {
        public float left, right, up, down;
        public float x { get => right - left; }
        public float y { get => up - down; }

        public Dir4(float left = 0, float right = 0, float up = 0, float down = 0)
        { this.left = left; this.right = right; this.up = up; this.down = down; }

        public void SetAll(float value)
        { left = value; right = value; up = value; down = value; }
    }



    public class CurveSlider
    {
        float _easeIn, _easeOut, _start, _end, _rang, _xMin, _astart, _aend;
        public CurveSlider(float ease_in, float ease_out)
        { UpdateValues(ease_in, ease_out); }

        void UpdateValues(float ease_in, float ease_out)
        {
            _easeIn = Mathf.Clamp01(ease_in);
            _easeOut = Mathf.Clamp01(ease_out);

            float a1 = _rad90 - _rad90 * _easeIn;
            float a2 = _rad90 - _rad90 * _easeOut;


            _astart = Mathf.Sin(a1);
            _aend = Mathf.Sin(a2);
        }

        public float Evaluate(float a, float b, float t)
        { return Mathf.Lerp(a, b, Evaluate01(t)); }
        public float Evaluate01(float t)
        {
            t = Mathf.Clamp01(t);
            float t1 = EaseInOutSine01(t);

            float a1 = _astart * t;
            float a2 = 1f - _aend * (1f - t);

            return Mathf.Lerp(a1, a2, t1);
        }

        public void TESTVAL()
        { Debug.Log($"{_astart} {_aend}"); }

        IEnumerator<float> IGetLine(int count)
        {
            float div = Mathf.Max(count - 1, 1);
            int i; for (i = 0; i < count; i++)
            { yield return Evaluate01((float)i / div); }
        }

        public void DrawGizmo(Vector2 offset, Vector2 scale, int segments)
        {
            float div = Mathf.Max(segments - 1, 1);
            Vector2 lpos = new Vector2(offset.x, offset.y);
            Vector2 npos; float idiv;
            int i; for (i = 1; i < segments; i++)
            {
                idiv = (float)i / div;
                npos = new Vector2(offset.x + scale.x * idiv, offset.y + Evaluate01(idiv) * scale.y);
                Gizmos.DrawLine(lpos, npos);
                lpos = npos;
            }
        }
    }

    public class Curve
    {
        AnimationCurve curve;
        Vector3[] spoints;
        List<Vector3> points;
        public int Segments { get => segments; set => SetSegmentCount(value); }
        int segments;
        public int Count { get => points.Count; }
        public int LastIndex { get => points.Count - 1; }
        public Vector3 this[int index]
        { get => points[index]; }

        void SetSegmentCount(int count)
        {
            segments = Mathf.Max(count, 2);
            spoints = new Vector3[segments];
            Update();
        }

        public Curve()
        {
            Debug.Log("curve base?");
            curve = new();
            points = new();
            SetSegmentCount(11);
        }

        public void DrawGizmo()
        {
            Vector3 npoint;
            Vector3 lpoint = Evaluate(0);
            float t_one = 1f / (Segments - 1f);
            for (int i = 1; i < Segments; i++)
            {
                npoint = Evaluate(i * t_one);
                Gizmos.DrawLine(lpoint, npoint);
                lpoint = npoint;
            }
        }

        public void ClearPoints()
        { points = new(); }

        public bool SetPoint(int index, Vector3 point)
        {
            points[index] = point;
            return true; 
        }

        public void AddPoint(Vector3 point)
        { points.Add(point); }
        public void InsertPoint(int index, Vector3 point)
        { points.Insert(index, point); }
        public bool RemovePoint(Vector3 point)
        { return points.Remove(point); }

        public void AddPoints(params Vector3[] points)
        { this.points.AddRange(points);  }
        public void InsertPoints(int index, params Vector3[] points)
        { this.points.InsertRange(index, points); }
        public int RemovePoints(params Vector3[] points)
        {
            int rcount = 0;
            for (int i = 0; i < points.Length; i++)
            { if (this.points.Remove(points[i])) rcount++; }
            return rcount;
        }

        public void Update()
        {
            int pdiff = curve.keys.Length - points.Count;
            if (pdiff < 0)
            { for (int i = 0; i < -pdiff; i++) { curve.AddKey(0,0); } }
            else if (pdiff > 0)
            {
                int a = curve.keys.Length - 1, 
                    b = curve.keys.Length - pdiff;
                for (int i = a; i > b; i--)
                { curve.RemoveKey(i); }
            }

            float t;
            for (int i = 0; i < curve.keys.Length; i++)
            {
                t = (i + 1f) / (points.Count + 1f);
                float tanIn = (points[i] - points[Mathf.Max(0, i - 1)]).magnitude, 
                    tanOut = (points[Mathf.Min(points.Count - 1, i + 1)] - points[i]).magnitude;
                curve.keys[i] = new Keyframe(t, points[i].x, tanIn, tanOut);
            }

            OnUpdate();
        }


        public Vector3[] GetCurve()
        {
            float p; int pseg = segments - 1;
            for (int i = 0; i < segments; i++)
            {
                p = curve.Evaluate((float)i / pseg);
                spoints[i] = new Vector3(p, p);
            }
            return spoints;
        }

        public Vector3 Evaluate(float t)
        {
            float p = curve.Evaluate(t);
            return new Vector3(p, p);
        }


        protected virtual void OnSegmentsChanged()
        {  }
        protected virtual void OnUpdate()
        {  }

    }

}
