using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Light2D))]
public class EntranceLight : MonoBehaviour
{
    [SerializeField] Room entranceRoom;
    Light2D l2d;

    private void Awake()
    {
        l2d = GetComponent<Light2D>();
    }

    private void OnValidate()
    {
        l2d = GetComponent<Light2D>();
        UpdateLight();
    }

    private void OnEnable()
    {
        UpdateLight();
    }

    public void UpdateLight()
    {
        if (entranceRoom == null) return;
        //Debug.Log($"entranceRoom? {entranceRoom}");
        entranceRoom.GetAmbientLight(out Color color, out float intensity);
        l2d.color = color;
        l2d.intensity = intensity;
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(EntranceLight))]
class EntranceLightEditor : Editor
{
    EntranceLight instance { get => (EntranceLight)target; }
    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();
        base.OnInspectorGUI();

        if (GUILayout.Button("Update light"))
        { instance.UpdateLight(); }
    }
}

#endif