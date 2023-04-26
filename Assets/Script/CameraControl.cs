using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif


[RequireComponent(typeof(Camera))]
public class CameraControl : MonoBehaviour
{
    public static CameraControl instance { get; protected set; }
    public static Camera cam { get => instance._cam; }
    Camera _cam;
    [SerializeField] List<Camera> subCameras = new();
    UnityEngine.Experimental.Rendering.Universal.PixelPerfectCamera pixcam;

    public float cameraSpeed = 16;
    public Transform followTarget;

    Vector2 targetPos, nudge = Vector2.zero, snudge = Vector2.zero;
    public Vector2 padding = new Vector2(1f, 1f);

    private void Awake()
    {
        instance = this;
        _cam = GetComponent<Camera>();
        pixcam = GetComponent<UnityEngine.Experimental.Rendering.Universal.PixelPerfectCamera>();
    }
    
    private void OnValidate()
    {
        _cam = GetComponent<Camera>();
        OnCameraChanged();
    }

    // Start is called before the first frame update
    void Start()
    {
        //pixcam = GetComponent<PixelPerfectCamera>();
        Application.targetFrameRate = 60;
        Debug.Log($"pixcam: {pixcam != null}");
        Debug.Log($"({pixcam.refResolutionX}, {pixcam.refResolutionY})");
        Screen.SetResolution(pixcam.refResolutionX, pixcam.refResolutionY, FullScreenMode.Windowed);
    }

    void OnCameraChanged()
    {
        for (int i = 0; i < subCameras.Count; i++)
        {
            subCameras[i].orthographic = _cam.orthographic;
            subCameras[i].fieldOfView = _cam.fieldOfView;
            subCameras[i].orthographicSize = _cam.orthographicSize;
        }
    }



#if UNITY_EDITOR 
    public void GetChildCamerasEditor()
    { GetChildCameras(); }
#endif

    void GetChildCameras()
    {
        subCameras = new(GetComponentsInChildren<Camera>());
    }

    public void SetCameraSize(int width, int height)
    {
        OnCameraChanged();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        FollowTarget();
    }

    void FollowTarget()
    {
        Vector2 tpos = transform.position;
        Vector2 moveTo = CameraTarget.activeTarget != null ? CameraTarget.targetTransform.position : targetPos;

        if (Room.ActiveRoom?.roomBounds != null) moveTo = ClampInRect(moveTo, GetCamClampRect(), padding);

        Vector2 dist = moveTo - tpos; 
        Vector2 dir = dist.normalized;
        float d = dist.magnitude;
        float s = Mathf.Max(1f, d * cameraSpeed);

        Vector2 npos = tpos + Vector2.ClampMagnitude(dir * s * Time.unscaledDeltaTime, d);

        transform.position = new Vector3(npos.x + nudge.x + snudge.x, npos.y + nudge.y + snudge.y, transform.position.z);
        return;
    }

    Rect GetCamClampRect()
    {
        float y = _cam.orthographicSize * 2;
        Vector2 s = new Vector2(y * _cam.aspect, y);
        return new Rect(Room.ActiveRoom.roomWorldBounds.position + s / 2, Room.ActiveRoom.roomWorldBounds.size - s); 
    }



    Vector2 ClampInRect(Vector2 p, Rect r, Vector2 padding)
    { return new Vector2(Mathf.Clamp(p.x, r.xMin+padding.x, r.xMax-padding.x), Mathf.Clamp(p.y, r.yMin+padding.y, r.yMax-padding.y)); }

    public void OnSceneLoaded()
    {
        if (Player.instance != null)
        { followTarget = Player.instance.transform; }
    }

    int nudgePriority = 0, shakePriority = 0;
    Coroutine currentNudge, currentShake;
    public Coroutine Nudge(Vector2 magnitude, float time_in, float time_out, int priority = 0, bool useUnscaledTime = false)
    {
        if (currentNudge != null)
        {
            if (priority < nudgePriority) return currentNudge;
            StopCoroutine(currentNudge);
        }
        currentNudge = StartCoroutine(INudge(magnitude, time_in, time_out, useUnscaledTime));
        nudgePriority = priority;
        return currentNudge;
    }

    
    public Coroutine Shake(Vector2 magnitude, float velocity, float time, int priority = 0, bool useUnscaledTime = false)
    {
        if (currentShake != null)
        {
            if (priority < shakePriority) return currentShake;
            StopCoroutine(currentShake);
        }
        currentShake = StartCoroutine(IShake(magnitude, velocity, time, useUnscaledTime));
        shakePriority = priority;
        return currentShake;
    }

    IEnumerator IShake(Vector2 magnitude, float velocity, float time, bool useUnscaledTime = false)
    {
        float t = 0, d, a = 0, _rad360 = Mathf.PI * 2, a_one = _rad360 * velocity / time;
        Action t_addDelta = useUnscaledTime ? () => { t += Time.unscaledDeltaTime; } : () => { t += Time.deltaTime; };
        while (t < time)
        {
            t_addDelta();
            d = t / time;
            snudge = magnitude * Mathf.Sign(Mathf.Sin(a_one * d));
            yield return new WaitForEndOfFrame();
        }
        snudge = Vector2.zero;

        shakePriority = 0;
        currentShake = null;
    }

    IEnumerator INudge(Vector2 magnitude, float time_in = 0.05f, float time_out = 0.2f, bool useUnscaledTime = true)
    {
        float t = 0;
        Action t_addDelta = useUnscaledTime ? () => { t += Time.unscaledDeltaTime; } : () => { t += Time.deltaTime; };
        while (t < time_in)
        { 
            t_addDelta(); 
            nudge = Vector2.Lerp(Vector2.zero, magnitude, t / time_in); 
            yield return new WaitForEndOfFrame(); 
        }
        nudge = magnitude;

        t = 0;
        while (t < time_in)
        { 
            t_addDelta(); 
            nudge = Vector2.Lerp(magnitude, Vector2.zero, t / time_out); 
            yield return new WaitForEndOfFrame(); 
        }
        nudge = Vector2.zero;

        nudgePriority = 0;
        currentNudge = null;
    }

}

#if UNITY_EDITOR

[CustomEditor(typeof(CameraControl))]
public class CameraControlEditor : Editor
{

    CameraControl instance;
    private void OnEnable()
    { instance = (CameraControl)target; }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        //DrawDefaultInspector()
        if (GUILayout.Button("Get child cameras"))
        { instance.GetChildCamerasEditor(); }
    }
}


#endif
