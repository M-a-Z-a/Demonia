using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Light2DBlinker : MonoBehaviour
{
    
    [SerializeField] AnimationCurve intensityPattern = new(new Keyframe(0f, 1f), new Keyframe(1f, 1f));
    [SerializeField] float intensityScale = 1f, timeOffset = 0f;
    Light2D light;

    private void OnValidate()
    {
        light = GetComponent<Light2D>();
        intensityPattern.preWrapMode = WrapMode.Loop;
        intensityPattern.postWrapMode = WrapMode.Loop;
    }

    private void Update()
    {
        float t;
#if UNITY_EDITOR
        t = (float)EditorApplication.timeSinceStartup + timeOffset;
#endif
        t = Time.time + timeOffset;
        light.intensity = intensityPattern.Evaluate(t);
    }

}
