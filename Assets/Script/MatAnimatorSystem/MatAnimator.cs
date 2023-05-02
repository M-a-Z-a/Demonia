using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static MatAnimExtras;
using static Utility;

public class MatAnimator : MonoBehaviour
{

    Dictionary<string, MatAnimation> anims;
    [SerializeField] Transform rendererTransform;
    [SerializeField] string animationGroup, jsonPath;
    Renderer rend;
    Material mat;
    public MatAnimation currentAnimation { get => anims[curAnim]; }
    string curAnim;
    [SerializeField] float currentAnimTime = 0;
    [SerializeField] MatFrame curFrame;
    [SerializeField] string defaultState = "";
    Vector3 rtransOffset, rtransScale;
    [SerializeField] bool autoFetchOnStart = false;
    public float animSpeed = 1f;

    public Dictionary<string, UnityAction> flagActions;
    public List<UnityAction> onAnimEnd;

    Coroutine playAnimCoroutine;
    public bool flipX = false;
    Vector2 frameflip = Vector2.one;

    private void Awake()
    {
        FetchRenderer();
        flagActions = new();
        onAnimEnd = new();
        anims = new();
    }

    private void Start()
    {
        if (autoFetchOnStart) FetchAnimationData();
    }

    void ParseJsonAnimGroup(JsonAnimGroup agroup)
    {
        foreach (JsonAnim anim in agroup.animations)
        {
            if (MatAnimation.FromJsonAnim(anim, out MatAnimation manim))
            {
                if (anims.ContainsKey(anim.id))
                { anims[anim.id] = manim;  continue; }
                anims.Add(anim.id, manim); 
            }
        }
    }

    public void FetchAnimationData(string animation_group = null, string animation_json_path = null, string atlas_path = null)
    {
        if (animation_group == null) animation_group = animationGroup;
        if (animation_json_path == null) animation_json_path = jsonPath;
        if (atlas_path != null) LoadJsonAtlas(atlas_path, out _);
        if (GetOrLoadJsonAnimGroup(animation_group, animation_json_path, out JsonAnimGroup agroup))
        { ParseJsonAnimGroup(agroup); }
        else
        { Debug.LogError($"AnimationGroup \"{animation_group}\"({animation_json_path}) not found!", this); }
        if (defaultState != "") SetState(defaultState);
    }

    void FetchRenderer()
    {
        if (rendererTransform != null)
        {
            rtransOffset = rendererTransform.localPosition;
            rtransScale = rendererTransform.localScale;
            rend = rendererTransform.GetComponent<Renderer>();
            if (rend != null)
            { mat = rend.material; }
        }
    }


    public void FlipX(bool flip)
    {
        if (flip != flipX)
        {
            flipX = flip;
            rendererTransform.localScale = rtransScale.Mult((flipX ? -1 : 1) * frameflip.x);
        }
    }
    public void SetState(string state, float? t = null)
    {
        bool tflags = false;
        if (curAnim == state)
        { if (t == null) return; }
        else
        { tflags = true; animEnded = false; }
        //if (curAnim == state && t == null) return;
        curAnim = state;
        //Debug.Log($"AnimState set to: {curAnim}");
        currentAnimTime = t != null ? (float)t : 0;
        NextFrame(tflags);
    }

    bool animEnded = false;
    MatFrame lframe = null;
    void NextFrame(bool init_flags = false)
    {
        MatAnimation canim;
        MatFrame mframe;
        canim = anims[curAnim];
        if (init_flags)
        { 
            foreach (string f in canim.Flags)
            { 
                if (flagActions.TryGetValue(f, out UnityAction act))
                { act.Invoke(); } 
            }
        }
        if (canim.Loop)
        { currentAnimTime %= canim.Duration; }
        else
        {
            if (!animEnded && currentAnimTime >= canim.Duration)
            {
                animEnded = true;
                for (int i = 0; i < onAnimEnd.Count; i++)
                { onAnimEnd[i].Invoke(); }
            }
            currentAnimTime = Mathf.Clamp(currentAnimTime, 0, canim.Duration); 
        }
        
        mframe = canim.GetFrame(currentAnimTime, out float atime);
        if (mframe != null)
        { curFrame = mframe; }

        frameflip = Vector2.one;
        if (curFrame != lframe)
        {
            foreach (string f in curFrame.Flags)
            {
                Debug.Log($"{curFrame.ID} flag: {f}");
                if (f == "flipX") { frameflip.x = -1; }
                else if (f == "flipY") { frameflip.y = -1; }
                if (flagActions.TryGetValue(f, out UnityAction act))
                { act.Invoke(); }
            }
        }
        lframe = curFrame;

        //mat.mainTextureOffset = curFrame.rect.position;
        //mat.mainTextureScale = curFrame.rect.size;
        //mat.SetTextureOffset("_MainTex", curFrame.rect.position);
        //mat.SetTextureScale("_MainTex", curFrame.rect.size);
        //mat.SetTextureOffset("_NormalMap", curFrame.rect.position);
        //mat.SetTextureScale("_NormalMap", curFrame.rect.size);
        mat.SetVector("_Offset", curFrame.rect.position);
        mat.SetVector("_Tiling", curFrame.rect.size);
        mat.SetVector("_NormalMultiply", frameflip);
        float diff = curFrame.rect.size.x / curFrame.rect.size.x;
        //Vector3 vec = rendererTransform.localScale;
        rendererTransform.localScale = rtransScale.Mult((flipX ? -1 : 1) * frameflip.x, frameflip.y);
        rendererTransform.localPosition = rtransOffset.Add(curFrame.pivot.x * rendererTransform.localScale.x, curFrame.pivot.y * rendererTransform.localScale.y);
        
        float ctime = currentAnimTime - atime;
        ctime = curFrame.Time - ctime;
        if (playAnimCoroutine != null) StopCoroutine(playAnimCoroutine);
        //playAnimCoroutine = StartCoroutine(IWaitNextFrameAtTime(Time.time + ctime));
        if (!animEnded) playAnimCoroutine = StartCoroutine(IWaitNextFrame(curFrame.Time, atime));// - ctime));
    }

    IEnumerator IWaitNextFrameAtTime(float time)
    {
        float t = Time.time;
        while (Time.time < time) { yield return null; }
        currentAnimTime += Time.time - t;
        NextFrame();
    }
    IEnumerator IWaitNextFrame(float time, float frameTime)
    {
        float t = 0;
        while (t < time)
        {
            t += Time.deltaTime * animSpeed;
            yield return null;
        }
        currentAnimTime = frameTime + time;
        NextFrame();
    }
}
