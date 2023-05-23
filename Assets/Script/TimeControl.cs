using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeControl : MonoBehaviour
{

    static TimeControl instance;
    static float fixedDeltaTime_default, timeScale_default;
    static bool _paused;
    public static bool paused { get => _paused; set => PauseState(value); }

    static float tmp_tscale = 1f;

    static void PauseState(bool b)
    { 
        if (b) Pause(); 
        else Resume(); 
    }
    static void Pause()
    {
        if (_paused) return;
        _paused = true;
        tmp_tscale = Time.timeScale;
        SetTimescale(0);
    }
    static void Resume()
    {
        if (!_paused) return;
        SetTimescale(tmp_tscale);
    }

    private void Awake()
    {
        fixedDeltaTime_default = Time.fixedDeltaTime;
        timeScale_default = Time.timeScale;
        instance = this;

        Application.targetFrameRate = 60;
    }

    public static void SetTimescale(float scale = 1f)
    {
        if (scale < 0) return;
        Time.timeScale = scale;
        Time.fixedDeltaTime = fixedDeltaTime_default * Time.timeScale;
        AudioManager.SetScaledPitch(Time.timeScale);
    }

    public static Coroutine SetTimeScaleFade(float scale, float fade)
    { return instance.StartCoroutine(instance.IFadeTimeScale(scale, fade)); }
    public static Coroutine SetTimeScaleFadeForTime(float scale, float time, float fade_in_t = 0, float fade_out_t = 0, float? new_timescale = null)
    { return instance.StartCoroutine(instance.ISetTimeScaleForTime(scale, time, fade_in_t, fade_out_t, new_timescale)); }


    IEnumerator IFadeTimeScale(float scale, float fade)
    {
        float init_timescale = Time.timeScale;
        float t = 0;
        while (t < fade)
        {
            while (_paused) { yield return new WaitForEndOfFrame(); }
            t += Time.unscaledDeltaTime;
            SetTimescale(Mathf.Lerp(init_timescale, scale, t / fade));
            yield return new WaitForEndOfFrame();
        }

        SetTimescale(scale);
    }

    IEnumerator ISetTimeScaleForTime(float scale, float time, float fade_in_t, float fade_out_t, float? new_timescale = null)
    {
        float init_timescale = Time.timeScale;
        float tscale_out = new_timescale != null ? (float)new_timescale : init_timescale;

        yield return StartCoroutine(IFadeTimeScale(scale, fade_in_t));

        float t = 0;
        while (t < time)
        {
            while (_paused) { yield return new WaitForEndOfFrame(); }
            t += Time.unscaledDeltaTime;
            yield return new WaitForEndOfFrame();
        }

        yield return StartCoroutine(IFadeTimeScale(tscale_out, fade_out_t));
    }

    

}
