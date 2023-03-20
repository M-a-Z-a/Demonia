using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ScreenFader : MonoBehaviour
{
    static List<ScreenFader> _sfades = new();
    public static List<ScreenFader> screenFades { get => _sfades; }
    
    RawImage rimg;
    [SerializeField] string _alias;
    public bool fading = false;
    float fdelta = 1f;
    bool _paused;
    public string alias { get => _alias; }
    public float fadeDelta { get => fdelta; }
    public bool paused { get => _paused; set => Pause(value); }

    public delegate Color FadeColorDeltaDelegate(float t_delta, Color color_start, Color color_end);

    Coroutine cFade, cPause;
    List<IEnumerator> fadeQue = new();

    // Start is called before the first frame update
    void Start()
    {
        Validate();

        if (!_sfades.Contains(this))
        { _sfades.Add(this); }
        
        SetColor(Color.black);
        FadeTo(Color.white, 1f);
        FadeTo(new Color(1, 1, 1, 0f), 2f);
    }

    public static bool GetScreenFader(string alias, out ScreenFader screenFader)
    {
        foreach (ScreenFader sf in _sfades)
        {
            if (alias == sf._alias)
            { screenFader = sf; return true; };
        }
        screenFader = null;
        return false;
    }

    public static int GetScreenFaders(string name, out List<ScreenFader> sflist)
    {
        sflist = new();
        {
            foreach (ScreenFader sf in _sfades)
            {
                if (name == sf.name || name == sf._alias)
                { sflist.Add(sf); };
            }
        }
        return sflist.Count;
    }

    void Validate()
    {
        if (TestAndGetRawImage())
        {
            Debug.LogWarning($"Screen fader does not have RawImage component!", this);
            this.enabled = false;
        }
    }

    void Pause(bool p = true)
    {
        if (p)
        { _paused = true; return; }
        _paused = false;
    }
    public void Pause(float t)
    {
        _paused = true;
        cPause = StartCoroutine(IPause(t));
    }
    public void Stop()
    {
        if (cFade == null) return;
        fadeQue = new();
        StopCoroutine(cFade);
    }


    public bool FadeTo(Color? color, float time)
    { return FadeFromTo(null, color, time); }
    public bool FadeTo(Color? color, float time, FadeColorDeltaDelegate delta_scale)
    { return FadeFromTo(null, color, time); }
    public bool FadeFromTo(Color? start_color, Color? end_color, float time)
    {
        if (!TestAndGetRawImage()) return false;
        int fque = fadeQue.Count;
        fadeQue.Add(IFade(start_color, end_color, time, FadeColorDeltaDefault));
        if (fque == 0)
        { cFade = StartCoroutine(fadeQue[0]); }
        return true;
    }
    public bool FadeFromTo(Color? start_color, Color? end_color, float time, FadeColorDeltaDelegate delta_scale)
    {
        if (!TestAndGetRawImage()) return false;
        int fque = fadeQue.Count;
        fadeQue.Add(IFade(start_color, end_color, time, delta_scale));
        if (fque == 0)
        { cFade = StartCoroutine(fadeQue[0]); }
        return true;
    }

    public bool SetColor(Color color, bool stop = true)
    {
        if (!TestAndGetRawImage()) return false;
        if (stop) Stop();
        rimg.color = color; return true;
    }


    int NextFade()
    {
        while(fadeQue.Count > 0)
        {
            fadeQue.RemoveAt(0);
            if (fadeQue.Count > 0)
            {
                cFade = StartCoroutine(fadeQue[0]);
                return fadeQue.Count;
            }
        }
        cFade = null;
        return 0;
    }

    IEnumerator IFade(Color? start_color, Color? end_color, float time, FadeColorDeltaDelegate delta_color)
    {
        Color color_s = start_color != null ? (Color)start_color : rimg.color;
        Color color_e = end_color != null ? (Color)end_color : rimg.color;
        fading = true;
        rimg.color = delta_color(0f, color_s, color_e);
        float t = 0;
        while (t < time && fading)
        {
            if (!_paused)
            {
                fdelta = t / time;
                rimg.color = delta_color(fdelta, color_s, color_e);
                t += Time.deltaTime;
            }
            yield return null;
        }
        fading = false;
        fdelta = 1f;
        rimg.color = delta_color(fdelta, color_s, color_e);
        NextFade();
    }

    IEnumerator IPause(float time)
    {
        paused = true;
        float t = 0;
        while (t < time && paused)
        {
            t += Time.deltaTime;
            yield return null;
        }
        paused = false;
        cPause = null;
    }


    Color FadeColorDeltaDefault(float d, Color a, Color b)
    { return Color.Lerp(a, b, d); }

    bool TestAndGetRawImage()
    { return rimg != null || TryGetComponent<RawImage>(out rimg); }

    
}
