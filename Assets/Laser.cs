using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Laser : Entity
{
    public enum ToggleState { Toggle = 0, Enable, Disable }

    public float distance;
    LineRenderer lrend;
    [SerializeField] bool _isActive = true;
    public bool isActive { get => _isActive; set => SetActive(value); }
    Coroutine fireCoroutine, toggleCoroutine;
    BoxCollider2D bcoll;
    public LayerMask stopLayer;
    Light2D l2d;
    [SerializeField] int queStartIndex = 0;
    //[SerializeField] List<float> toggleQue = new();
    [SerializeField] List<StateTimePair> toggleQue = new();
    [SerializeField] float startTime = 0f;

    public UnityEvent<Laser, Collider2D> onLaserHit;

    protected override void Awake()
    {
        base.Awake();
        lrend = GetComponent<LineRenderer>();
        lrend.widthMultiplier = 3f;
        lrend.useWorldSpace = true;
        bcoll = GetComponent<BoxCollider2D>();
        bcoll.size = new Vector2(0.2f, 1f);
        l2d = GetComponent<Light2D>();

        Vector3[] spath = new Vector3[]
        {
            new Vector3(-0.1f, 0f, 0f),
            new Vector3(-0.1f, distance, 0f),
            new Vector3(0.1f, distance, 0f),
            new Vector3(0.1f, 0f, 0f)
        };
        l2d.SetShapePath(spath);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.up * distance); 
    }


    private void OnEnable()
    {
        SetActive(_isActive, true);
        if (toggleQue.Count > 0)
        { 
            if (toggleCoroutine != null) StopCoroutine(toggleCoroutine);
            toggleCoroutine = StartCoroutine(IToggler());
        }
        else
        {
            
        }
    }
    private void OnDisable()
    {
        if (toggleCoroutine != null) StopCoroutine(toggleCoroutine);
        if (fireCoroutine != null) StopCoroutine(fireCoroutine);
    }

    
    protected override void Start()
    { base.Start(); }

    public void SetActive(bool state, bool allow_reset = false)
    {
        if (state)
        { Activate(allow_reset); return; }
        Deactivate();
    }
    public void Activate(bool allow_reset = false)
    {
        if (_isActive && !allow_reset) return;
        _isActive = true;
        lrend.positionCount = 2;
        bcoll.enabled = true;
        l2d.enabled = true;
        if (fireCoroutine != null) StopCoroutine(fireCoroutine);
        fireCoroutine = StartCoroutine(IFire());
    }
    public void Deactivate()
    {
        _isActive = false;
        if (fireCoroutine != null) StopCoroutine(fireCoroutine);
        lrend.positionCount = 0;
        bcoll.enabled = false;
        l2d.enabled = false;
    }

    int NextQueIndex(int i)
    { return i++ % toggleQue.Count; }
    float NextQueDelay(int i)
    { return toggleQue[i++ % toggleQue.Count].time; }
    float NextQueTime(int i, out int new_i, float? lastQueTime = null)
    {
        new_i = (i+1) % toggleQue.Count;
        return (lastQueTime != null ? (float)lastQueTime:Time.time) + toggleQue[new_i].time;
    }


    RaycastHit2D rhit;
    IEnumerator IFire()
    {
        while(true)
        {
            rhit = Physics2D.Raycast(transform.position, transform.up, distance, stopLayer);
            lrend.SetPosition(0, transform.position);
            if (rhit.collider)
            {
                Debug.DrawLine(transform.position, rhit.point, Color.red);
                lrend.SetPosition(1, rhit.point);
                bcoll.offset = new Vector2(0, rhit.distance / 2f);
                bcoll.size = new Vector2(bcoll.size.x, rhit.distance);
                l2d.shapePath[1].y = rhit.distance;
                l2d.shapePath[2].y = rhit.distance;
            }
            else
            {
                Debug.DrawRay(transform.position, transform.up * distance, Color.red);
                lrend.SetPosition(1, transform.position + transform.up*distance);
                bcoll.offset = new Vector2(0, distance / 2f);
                bcoll.size = new Vector2(bcoll.size.x, distance);
                l2d.shapePath[1].y = distance;
                l2d.shapePath[2].y = distance;
            }
            yield return new WaitForEndOfFrame();
        }
    }

    IEnumerator IToggler()
    {
        //if (toggleQue.Count == 0) yield break;
        int t_ind = queStartIndex % toggleQue.Count, l_ind = 0;
        float next_t = toggleQue.Count > 0 ? NextQueTime(t_ind, out t_ind) : 0;
        while (true)
        {
            if (Time.time >= next_t)
            {
                if (toggleQue.Count == 0) yield break;
                switch(toggleQue[t_ind].state)
                {
                    case ToggleState.Enable: Activate(); break;
                    case ToggleState.Disable: Deactivate(); break;
                    default: isActive = !isActive; break;
                }
                next_t = NextQueTime(t_ind, out t_ind, next_t);
            }
            yield return new WaitForEndOfFrame();
        }
    }

    [System.Serializable]
    class StateTimePair
    { 
        public ToggleState state; public float time; 
        public StateTimePair(float time, ToggleState state = default)
        {
            this.time = time;
            this.state = state;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        onLaserHit.Invoke(this, collision);
    }
}


#if UNITY_EDITOR
/*
[CustomEditor(typeof(Laser))]
[CanEditMultipleObjects]
public class LaserEditor : Editor
{
    Laser instance;
    private void OnEnable()
    {
        //instance = (Laser)target; 
        //EditorApplication.update += Update;
    }
    private void OnDisable()
    {
        //instance.runInEditMode = false;
        //EditorApplication.update -= Update;
    }

    
    public override void OnInspectorGUI()
    {
        //base.OnInspectorGUI();
        DrawDefaultInspector();
        
        GUILayout.Label($"Run in Edit mode ({(instance.runInEditMode?"Enabled":"Disabled")})");
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Enable"))
        { 
            instance.runInEditMode = true;
        }
        if (GUILayout.Button("Disable"))
        { 
            instance.runInEditMode = false; 
        }
        if (GUILayout.Button($"Toggle({(instance.runInEditMode?"Disable":"Enable")})"))
        { 
            instance.runInEditMode = !instance.runInEditMode;
        }
        EditorGUILayout.EndHorizontal();
        
    }

    void Update()
    {
        if (instance.runInEditMode)
        { EditorApplication.QueuePlayerLoopUpdate(); }
    }
}
*/

#endif
