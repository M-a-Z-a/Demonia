using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    Coroutine playAnimCoroutine;
    bool flipX = false;

    private void Awake()
    {
        FetchRenderer();
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
                { anims[anim.id] = manim; continue; }
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
            rendererTransform.localScale = rtransScale.Mult(flipX ? -1 : 1);
        }
    }
    public void SetState(string state, float? t = null)
    {
        if (curAnim == state && t == null) return;
        curAnim = state;
        if (t != null) currentAnimTime = (float)t;
        NextFrame();
    }
    void NextFrame()
    {
        currentAnimTime %= anims[curAnim].Duration;
        curFrame = anims[curAnim].GetFrame(currentAnimTime % anims[curAnim].Duration);
        mat.SetVector("_Offset", curFrame.rect.position);
        mat.SetVector("_Tiling", curFrame.rect.size);
        float diff = curFrame.rect.size.x / curFrame.rect.size.x;
        Vector3 vec = rendererTransform.localScale;
        rendererTransform.localPosition = rtransOffset.Add(curFrame.pivot.x * rendererTransform.localScale.x, curFrame.pivot.y * rendererTransform.localScale.y);
        playAnimCoroutine = StartCoroutine(IWaitNextFrame(curFrame.Time));
    }

    IEnumerator IWaitNextFrame(float time)
    {
        float t = 0;
        while (t < time)
        {
            t += Time.deltaTime;
            yield return null; 
        }
        currentAnimTime += t;
        NextFrame();
    }
}
