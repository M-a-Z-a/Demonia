using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    static AudioManager instance;
    [SerializeField] AudioMixer _mixer;
    static AudioMixer _aMixer;

    [SerializeField] AudioMixer[] amixers;
    [SerializeField] AudioMixerGroup[] amixgroups;

    public static MixerGroup mixerMaster, mixerUI, mixerMusic, mixerAmbient, mixerEffects;
    public static IEnumerable<MixerGroup> MixerGroups { get => IMixerGroups(); }
    public static AudioMixer mixer { get => instance._mixer; }

    private void Awake()
    {
        instance = this;
        SetMixer(_mixer);
    }

    public void SetMixer(AudioMixer mixer)
    {
        _aMixer = _mixer;

        mixerMaster = new MixerGroup("Master");
        mixerMaster.BasicFetch();
        //mixerMaster.AddValue("volume", "Master_volume");

        mixerUI = new MixerGroup("UI");
        //mixerMaster.AddValue("volume", "UI_volume");

        mixerMusic = new MixerGroup("Music");
        //mixerMaster.AddValue("volume", "UI_volume");

        mixerAmbient = new MixerGroup("Ambient");
        //mixerMaster.AddValue("volume", "UI_volume");

        mixerEffects = new MixerGroup("Effects");
        //mixerMaster.AddValue("volume", "UI_volume");

        foreach (MixerGroup mg in MixerGroups)
        { mg.BasicFetch(); }
    }

    public static void SetScaledVolume(float value)
    {
        float v = (1f - value) * -80f;
        //mixer.SetFloat("UI_volume", v);
        //mixer.SetFloat("Music_volume", v);
        //mixer.SetFloat("Ambient_volume", v);
        //mixer.SetFloat("Effects_volume", v);
        mixerUI["volume"].value = v;
        mixerMusic["volume"].value = v;
        mixerAmbient["volume"].value = v;
        mixerEffects["volume"].value = v;
    }
    public static void SetScaledPitch(float value)
    {
        //mixer.SetFloat("UI_pitch", value);
        //mixer.SetFloat("Music_pitch", value);
        //mixer.SetFloat("Ambient_pitch", value);
        //mixer.SetFloat("Effects_pitch", value);
        mixerUI["pitch"].value = value;
        mixerMusic["pitch"].value = value;
        mixerAmbient["pitch"].value = value;
        mixerEffects["pitch"].value = value;
    }

    public class MixerValue
    {
        public MixerValue(string exposed_name) { param_name = exposed_name; }

        string param_name;
        public string exposedName { get => param_name; }
        public float value { get => GetValue(); set => SetValue(value); }
    
        public float GetValue()
        { _aMixer.GetFloat(param_name, out float v); return v; }
        public void SetValue(float value)
        { _aMixer.SetFloat(param_name, value); }
        public void Reset()
        { _aMixer.ClearFloat(param_name); }

        public static implicit operator float(MixerValue a)
        { return a.value; }
    }

    public class MixerGroup
    {
        // naming rule $"{_name}_{value}"
        static string[] defaultValueSearch = new[] { "volumeBase", "volume", "pitch" };

        string _name;
        Dictionary<string, MixerValue> _values;
        public string name { get => _name; }

        public MixerGroup(string name)
        { _name = name; _values = new(); }

        public bool AddValue(string name, string exposed_name)
        {
            if (_values.ContainsKey(name)) return false;
            if (_aMixer.GetFloat(exposed_name, out _))
            {
                _values.Add(name, new MixerValue(exposed_name));
                //Debug.Log($"Value {name}({exposed_name}) added to {_name}");
                return true; 
            }
            return false;
        }

        public bool GetValue(string name, out MixerValue value)
        { return _values.TryGetValue(name, out value); }

        public bool HasValue(string name)
        { return _values.ContainsKey(name); }

        public MixerValue this[string name]
        { get => _values[name]; }

        public void BasicFetch()
        {
            foreach (string k in defaultValueSearch)
            { AddValue(k, $"{_name}_{k}"); }
            
        }
    }

    public static Coroutine FadeMixerValue(string mxValue, float value, float time)
    { return instance.StartCoroutine(instance.IFadeMixerValue(mxValue, value, time)); }
    public static Coroutine FadeValue(MixerValue mxValue, float value, float time)
    { return instance.StartCoroutine(instance.IFadeValue(mxValue, value, time)); }

    IEnumerator IFadeMixerValue(string mxValue, float value, float time)
    {
        float init_val;
        if (!mixer.GetFloat(mxValue, out init_val)) yield break;
        float t = 0;
        while (t < time)
        {
            t += Time.deltaTime;
            mixer.SetFloat(mxValue, Mathf.Lerp(init_val, value, t / time));
            yield return new WaitForEndOfFrame();
        }
        mixer.SetFloat(mxValue, value);
    }

    IEnumerator IFadeValue(MixerValue mxValue, float target_val, float time)
    {
        float init_val = mxValue;
        float t = 0;
        while(t < time)
        {
            t += Time.deltaTime;
            mxValue.value = Mathf.Lerp(init_val, target_val, t / time);
            yield return new WaitForEndOfFrame();
        }
        mxValue.value = target_val;
    }
    
    static IEnumerable<MixerGroup> IMixerGroups()
    {
        yield return mixerUI;
        yield return mixerMusic;
        yield return mixerAmbient;
        yield return mixerEffects;
    }

}

