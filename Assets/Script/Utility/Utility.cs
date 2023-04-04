using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// base utility
public static partial class Utility
{

    public const float _rad90 = Mathf.PI / 2;
    public const float _rad180 = Mathf.PI;
    public const float _rad270 = _rad90 * 3;
    public const float _rad360 = Mathf.PI * 2;
    

    public static float ClampMult(float value, float mult, float min, float max)
    { return Mathf.Clamp(value * mult, min, max) / mult; }
    public static float ClampDiv(float value, float div, float min, float max)
    { return Mathf.Clamp(value / div, min, max) * div; }

    public delegate float EaseActionDelegate(float t);

    public static float EaseOutSine01(float t)
    { return Mathf.Sin(t * Mathf.PI / 2); }
    public static float EaseInSine01(float t)
    { return 1f - Mathf.Cos(t * Mathf.PI / 2); }
    public static float EaseInOutSine01(float t)
    { return -(Mathf.Cos(Mathf.PI * t) - 1) / 2; }
    
    public static float EaseInCirc01(float t)
    { return 1 - Mathf.Sqrt(1 - Mathf.Pow(t, 2)); }
    public static float EaseOutCirc01(float t)
    { return Mathf.Sqrt(1 - Mathf.Pow(t - 1, 2)); }
    public static float EaseInOutCirc01(float t)
    { return t < 0.5 ? (1 - Mathf.Sqrt(1 - Mathf.Pow(2 * t, 2))) / 2 : (Mathf.Sqrt(1 - Mathf.Pow(-2 * t + 2, 2)) + 1) / 2; }


    public static float CurveCombination(float t, EaseActionDelegate _in, EaseActionDelegate _out, float offset = 0.5f)
    {
        if (t < offset)
        { return _in(t/offset); }
        return 1f - _out((t - offset) / (1f - offset));
    }

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
    public static int ComparePolarity(float a, float b)
    {
        if (a < 0 && b > 0) return -1;
        if (a > 0 && b < 0) return 1;
        return 0;
    }
    public static int ComparePolarityTo(float a, float b, float zero)
    {
        if (a < zero && b > zero) return -1;
        if (a > zero && b < zero) return 1;
        return 0;
    }
    
    
    
    

    public static float ToRad(this float value)
    { return value * Mathf.Deg2Rad; }
    public static float ToDeg(this float value)
    { return value * Mathf.Rad2Deg; }


    public static Component GetSetCompononent<T>(this GameObject go, T component) where T : Component
    {
        if (go.TryGetComponent<T>(out T c_out)) 
        { return c_out; }
        return go.AddComponent<T>();
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