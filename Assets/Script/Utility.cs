using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{

    public static float ClampMultiplier(float value, float mult, float min, float max)
    { return Mathf.Clamp(value * mult, min, max) / mult; }
    public static float ClampDivision(float value, float div, float min, float max)
    { return Mathf.Clamp(value / div, min, max) * div; }

    public static float SineSlider(float d)
    { return Mathf.Sin(d); }
    public static float SineSlider(float d, float r)
    { return Mathf.Sin(d) * r; }
    public static float InverseSineSlider(float d)
    { return 1f - Mathf.Sin(d); }
    public static float InverseSineSlider(float d, float r)
    { return (1f - Mathf.Sin(d)) * r; }

    public static float TowardsTargetValue(float a, float b, float add)
    {
        if (b > a)
        { return Mathf.Min(a + add, b); }
        else if (b < a)
        { return Mathf.Max(a - add, b); }
        return a;
    }
    public static bool IsEqualPolarity(float a, float b)
    {
        if (a < 0)
        { return a + b < a; }
        return a + b > a;
    }

    public static Vector2 TowardsTargetVector(Vector2 a, Vector2 b, Vector2 add)
    { return new Vector2(TowardsTargetValue(a.x, b.x, add.x), TowardsTargetValue(a.y, b.y, add.y)); }
    public static Vector2 TowardsTargetVector(Vector2 a, Vector2 b, float add)
    {
        Vector2 _add = (b - a).Abs().normalized * add;
        return new Vector2(TowardsTargetValue(a.x, b.x, _add.x), TowardsTargetValue(a.y, b.y, _add.y)); 
    }
    public static Vector2 Vector2Clamp(Vector2 vec, Vector2 min, Vector2 max)
    { return new Vector2(Mathf.Clamp(vec.x, min.x, max.x), Mathf.Clamp(vec.y, min.x, max.x)); }
    public static Vector2 Vector2Abs(Vector2 vec)
    { return new Vector2(Mathf.Abs(vec.x), Mathf.Abs(vec.y)); }

    public static Vector2 TowardsTarget(this Vector2 vec, Vector2 target, Vector2 value)
    { return TowardsTargetVector(vec, target, value); }
    public static Vector2 Clamp(this Vector2 vec, Vector2 min, Vector2 max)
    { return Vector2Clamp(vec, min, max); }
    public static Vector2 Abs(this Vector2 vec)
    { return Vector2Abs(vec); }
    public static Vector2 Redirect(this Vector2 vec, Vector2 dir)
    { float mag = vec.magnitude; return dir * mag; }
    public static Vector2 Redirect(this Vector2 vec, Vector2 dir, float delta)
    { float mag = vec.magnitude * delta; return dir * mag; }

    public static Vector2 MoveAndRedirect(this Vector2 vec, float distance, Vector2 redirect, float delta)
    {
        float mag = vec.magnitude;
        float lmin = Mathf.Min(distance, mag);
        float leftov = distance - lmin;
        Vector2 nvec = vec.normalized * lmin;
        if (leftov > 0) return nvec + redirect * leftov;
        return nvec;
    }

    public static float Angle(this Vector2 vec)
    { return Vector2.Angle(vec, Vector2.right); }
    public static Vector2 Turn90CW(this Vector2 vec)
    { return new Vector2(vec.y, -vec.x); }
    public static Vector2 Turn90CCW(this Vector2 vec)
    { return new Vector2(-vec.y, vec.x); }
    public static Vector2 Turn(this Vector2 vec, float a)
    {
        float mag = vec.magnitude;
        float newa = Mathf.Atan2(vec.x, vec.y) + a * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(newa), Mathf.Sin(newa)) * mag;
    }
    public static void DrawDebugRay(this Vector2 vec, Vector2 origin, Color color)
    { Debug.DrawRay(origin, vec, color); }
    public static void DrawDebugRay(this Vector2 vec, Vector2 origin, float mult, Color color)
    { Debug.DrawRay(origin, vec * mult, color); }

    public static Vector2 Add(this Vector2 vec, float x = 0, float y = 0)
    { return new Vector2(vec.x + x, vec.y + y); }
    public static Vector2 Add(this Vector2 vec, Vector2 o)
    { return new Vector2(vec.x + o.x, vec.y + o.y); }
    public static Vector2 Add(this Vector2 vec, Vector3 o)
    { return new Vector2(vec.x+o.x, vec.y + o.y); }
    public static Vector3 Add(this Vector3 vec, float x = 0, float y = 0, float z = 0)
    { return new Vector3(vec.x + x, vec.y + y, vec.z + z); }
    public static Vector3 Add(this Vector3 vec, Vector2 o)
    { return new Vector3(vec.x + o.x, vec.y + o.y, vec.z); }
    public static Vector3 Add(this Vector3 vec, Vector3 o)
    { return new Vector3(vec.x + o.x, vec.y + o.y, vec.z + o.z); }


    public static float Scalar(Vector2 a, Vector2 b)
    { 
        return a.x * b.x + a.y * b.y; 
    }
}














/*
    public abstract class Waveform
    {
        protected AnimationCurve acurve;
        protected float amp = 1f, freq = 1f, cent = 0;
        protected Vector2 wbal;
        public float time = 0f;
        public float amplitude { get => amp; set => amp = value; }
        public float frequency { get => freq; set => freq = value; }
        public Vector2 balance { get => wbal; }
        public AnimationCurve animCurve { get => acurve; }

        public virtual float GetValueAt(float t)
        { return acurve.Evaluate(t * freq) * amp; }
        public virtual float GetValue()
        { return GetValueAt(time); }
        public virtual float GetValueNext(float t_add)
        { return GetValueAt(time += t_add); }
        protected abstract void CreateCurve();
    }
    public class SineWave : Waveform
    {
        public SineWave(float amplitude, float frequency, float timeOffset, Vector2 balance, float waveCenter = 0f)
        { amp = amplitude; freq = frequency; wbal = balance; cent = waveCenter; CreateCurve(); }


        protected override void CreateCurve()
        {
            float r360 = Mathf.PI * 2f;
            float r4th = r360 / 4;
            float center = r360 * ((Mathf.Clamp(cent, -1f, 1f) + 1) / 2);
            float yHigh = Mathf.Clamp(1f + wbal.y, 0, 1f), yLow = Mathf.Clamp(-1f + wbal.y, -1f, 0);
            float xOffm = Mathf.Clamp(wbal.x, -1f, 1f), xOff = 0.25f * (xOffm + 1f);
            float dOff = (xOffm+1)/2, wHigh = dOff, wLow = 1f - dOff;
            acurve = new AnimationCurve();
            acurve.postWrapMode = WrapMode.Loop;
            acurve.preWrapMode = WrapMode.Loop;
            acurve.AddKey(new Keyframe(0, 0, 1, 1));
            acurve.AddKey(new Keyframe(r360 * xOff, yHigh, 0, 0, wHigh, wLow));
            acurve.AddKey(new Keyframe(r360 * 0.5f, 0, -1, -1));
            acurve.AddKey(new Keyframe(r360 * (1f - xOff), yLow, 0, 0, wLow, wHigh));
            acurve.AddKey(new Keyframe(r360, 0, 1, 1));
        }


    }

    public class SquareWave : Waveform
    {
        public SquareWave(float amplitude, float frequency, Vector2 balance, float waveCenter = 0f)
        { amp = amplitude; freq = frequency; wbal = balance; cent = waveCenter; CreateCurve(); }
        protected override void CreateCurve()
        {
            float r360 = Mathf.PI * 2f;
            float yHigh = Mathf.Clamp(1f + wbal.y, 0, 1f), yLow = Mathf.Clamp(-1f + wbal.y, -1f, 0);
            float xOffm = Mathf.Clamp(wbal.x, -1f, 1f), xOff = (xOffm + 1) / 2;
            acurve = new AnimationCurve();
            acurve.postWrapMode = WrapMode.Loop;
            acurve.preWrapMode = WrapMode.Loop;
            acurve.AddKey(new Keyframe(0, yHigh, 0, 0, 0, 0));
            acurve.AddKey(new Keyframe(r360 * xOff, yHigh, 0, 0, 0, 0));
            acurve.AddKey(new Keyframe(r360 * (1f - xOff), yLow, 0, 0, 0, 0));
            acurve.AddKey(new Keyframe(r360, yLow, 0, 0, 0, 0));
        }
    }

    public class SawWave : Waveform
    {
        public SawWave(float amplitude, float frequency, Vector2 balance, float waveCenter = 0f)
        { amp = amplitude; freq = frequency; wbal = balance; cent = waveCenter; CreateCurve(); }
        protected override void CreateCurve()
        {
            float r360 = Mathf.PI * 2f;
            float yOff = Mathf.Clamp(wbal.y, -1f, 1f);
            float yHigh = Mathf.Clamp(1f + wbal.y, 0, 1f), yLow = Mathf.Clamp(-1f + wbal.y, -1f, 0);
            float xOffm = Mathf.Clamp(wbal.x, -1f, 1f), xOff = 0.25f * (xOffm + 1f);
            acurve = new AnimationCurve();
            acurve.postWrapMode = WrapMode.Loop;
            acurve.preWrapMode = WrapMode.Loop;
            acurve.AddKey(new Keyframe(0, 0, 1, 1, 0, 0));
            acurve.AddKey(new Keyframe(r360 * xOff, yHigh, 0, 0, 0, 0));
            acurve.AddKey(new Keyframe(r360 * (1f - xOff), yLow, 0, 0, 0, 0));
            acurve.AddKey(new Keyframe(r360, 0, 1, 1, 0, 0));
        }
    }
    */