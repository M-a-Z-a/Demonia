using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MixerRelay : MonoBehaviour
{
    Slider slider;
    [SerializeField] string namePrefix = "";
    [SerializeField] AudioMixer mixer;
    [SerializeField] string exposedValue = "";
    [SerializeField] [Range(-80, 20)] float min_value = -80, max_value = 0;
    //[SerializeField] AnimationCurve valueCurve;

    [SerializeField] bool _valueExists;
    float range = 0;
    public bool valueExists { get => _valueExists; }

    private void OnValidate()
    {
        GetSlider();
        TestValue();
        UpdateRange();
    }
    private void Start()
    { 
        GetSlider(); 
        TestValue();
        UpdateRange();
        slider.onValueChanged.AddListener(UpdateValue);
    }

    private void OnEnable()
    { SetSliderValue(); }
    private void OnDisable()
    { }

    void UpdateRange()
    { 
        range = max_value - min_value; 
        if (range == 0) range = 1;
    }

    bool TestValue()
    {
        if (exposedValue == "") return false;
        _valueExists = mixer != null && mixer.GetFloat(exposedValue, out float v);
        if (namePrefix != "")
        { gameObject.name = $"{(_valueExists ? "" : "!")}{namePrefix}({exposedValue})"; }
        else
        { gameObject.name = $"{(_valueExists ? "" : "!")}{exposedValue}"; }
        return _valueExists;
    }
    bool GetSlider()
    { slider = GetComponent<Slider>(); return slider != null; }
    bool SetSliderValue()
    {
        if (slider == null) return false;
        if (GetValueDelta(out float v))
        { slider.value = v; return true; }
        return false;
    }

    void UpdateValue(float value)
    { SetValueDelta(value); }
    public bool SetValueDelta(float delta)
    { return SetValue(Mathf.Lerp(min_value, max_value, delta)); }
    public bool SetValue(float value)
    { return mixer.SetFloat(exposedValue, value <= min_value ? -80 : Mathf.Clamp(value, min_value, max_value)); }
    public bool GetValue(out float value)
    { return mixer.GetFloat(exposedValue, out value); }
    public bool GetValueDelta(out float value)
    {
        if (GetValue(out float v))
        {
            value = (v-min_value) / range;
            Debug.Log($"{namePrefix}: {v}/{range}={value}");
            return true;
        }
        value = 0;
        return false;
    }
}
