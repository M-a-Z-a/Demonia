using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(LineRenderer))]
public class LaserPointer : MonoBehaviour
{

    [SerializeField][HideInInspector] LoopPattern lpattern;

    public float distance = 100;
    public LayerMask layerMask;
    [SerializeField] Color _color = Color.red;
    [SerializeField] Material material;
    Material lrend_mat;
    [SerializeField][HideInInspector] LineRenderer lrend;
    [SerializeField] Light2D laserLight, pointLight;
    public List<UnityAction<RaycastHit2D>> onHit = new();
    public List<UnityAction<RaycastHit2D>> onHitNew = new();
    Collider2D l_hit = null;
    public Color color { get => _color; set => SetColor(value); }
    public bool manual_update = false;

    private void OnValidate()
    {
        lpattern = GetComponent<LoopPattern>();
        lrend = GetComponent<LineRenderer>();
        if (lrend != null && material != null)
        {
            lrend_mat = new Material(material);
            lrend.material = lrend_mat;
            SetColor(_color);
        }
    }

    private void Awake()
    {
        lrend = GetComponent<LineRenderer>();
        lrend.positionCount = 2;
        SetColor(_color);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!manual_update)
        { UpdateLaser(); }
    }

    private void OnEnable()
    {
        laserLight.enabled = true;
        pointLight.enabled = false;
    }
    private void OnDisable()
    {
        laserLight.enabled = false;
    }

    public void ResetAngle()
    {
        if (lpattern != null)
        {
            lpattern.ResetEnumerator();
            Vector3 eul = transform.localEulerAngles;
            eul.z = lpattern.GetCurrent();
            transform.localEulerAngles = eul;
        }
    }
    public void GetNextAngle()
    {
        if (lpattern != null)
        {
            Vector3 eul = transform.localEulerAngles;
            eul.z = lpattern.GetNext();
            transform.localEulerAngles = eul;
        }
    }

    void SetColor(Color c)
    {
        _color = c;
        laserLight.color = _color;
        pointLight.color = _color;
        lrend_mat.SetColor("_Color1", _color);
        lrend_mat.SetColor("_Color0", _color);
    }

    Vector2 tpos;
    RaycastHit2D rhit;
    public void UpdateLaser()
    {
        tpos = transform.position;
        rhit = Physics2D.Raycast(tpos, transform.up, distance, layerMask);
        float dist;
        if (rhit.collider)
        {
            dist = rhit.distance;
            pointLight.transform.position = rhit.point;
            pointLight.enabled = true;
            foreach (var ua in onHit)
            { ua.Invoke(rhit); }
        }
        else
        { dist = distance; pointLight.enabled = false; }

        if (rhit.collider != l_hit)
        {
            foreach (var ua in onHitNew)
            { ua.Invoke(rhit); }
        }
        l_hit = rhit.collider;

        lrend.SetPosition(1, new Vector3(0, dist, 0));
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(LaserPointer))]
public class LaserPointerEditor : Editor
{
    LaserPointer instance;
    SerializedProperty smat, lrend, lpattern;
    private void OnEnable()
    {
        instance = (LaserPointer)target;
        smat = serializedObject.FindProperty("material");
        lrend = serializedObject.FindProperty("lrend");
        lpattern = serializedObject.FindProperty("lpattern");
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if ((LineRenderer)lrend.objectReferenceValue)
        { 
            if (GUILayout.Button("Test Laser"))
            {
                instance.UpdateLaser();
            }
        }
        serializedObject.ApplyModifiedProperties();
        
        if ((LoopPattern)lpattern.objectReferenceValue != null)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset Angle"))
            {
                instance.ResetAngle();
            }
            if (GUILayout.Button("Next Angle"))
            {
                instance.GetNextAngle();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}

#endif