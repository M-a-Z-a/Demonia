using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioListenerControl : MonoBehaviour
{
    public static AudioListenerControl instance { get; private set; }
    [SerializeField] AudioSource sourceMaster, sourceUI, sourceMusic, sourceAmbient, sourceEffects;

    private void Awake()
    { if (instance == null) instance = this; }

    public static bool SetParent(Transform parent, Vector3? local_position)
    { return instance._SetParent(parent, local_position); }
    bool _SetParent(Transform parent, Vector3? local_position)
    {
        if (parent == null) return false;
        transform.SetParent(parent, false);
        transform.localPosition = local_position != null ? (Vector3)local_position : Vector3.zero;
        return true;
    }

    public static void Music_Set(AudioClip clip)
    { instance.sourceMusic.clip = clip; }
    public static void Music_Play()
    { instance.sourceMusic.Play(); }
    public static Coroutine Music_Play(float volume, float fade_t)
    { return instance.StartCoroutine(instance.IMusicPlay(volume, fade_t)); }
    public static void Music_Stop()
    { instance.sourceMusic.Stop(); }
    public static Coroutine Music_Stop(float fade_t)
    { return instance.StartCoroutine(instance.IMusicStop(fade_t)); }


    IEnumerator ISetMusicFade(AudioClip clip, float time, float fade_from_t, float fade_to_t)
    {
        float initial_value = AudioManager.mixerMusic["volume"].value;
        float t = 0;
        while (t < time)
        {
            t += Time.deltaTime;
            AudioManager.mixerMusic["volume"].value = Mathf.Lerp(initial_value, 0, t / time);
            yield return null;
        }
    }
    IEnumerator IMusicStop(float time)
    {
        yield return AudioManager.FadeValue(AudioManager.mixerMusic["volume"], 0f, time);
        instance.sourceMusic.Stop();
    }
    IEnumerator IMusicPlay(float volume, float time)
    {
        yield return AudioManager.FadeValue(AudioManager.mixerMusic["volume"], volume, time);
        instance.sourceMusic.Play();
    }

    IEnumerable<AudioSource> IAudioSources()
    {
        yield return sourceUI;
        yield return sourceMusic;
        yield return sourceAmbient;
        yield return sourceEffects;
    }
}
