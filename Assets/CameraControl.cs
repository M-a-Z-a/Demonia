using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class CameraControl : MonoBehaviour
{
    public static CameraControl instance { get; protected set; }
    PixelPerfectCamera pixcam;
    Camera _cam;
    public static Camera cam { get => instance._cam; }
    static float deg90 = 90 * Mathf.Deg2Rad;
    public float cameraSpeed = 16;
    public Transform followTarget;
    Vector2 targetPos;
    public Rect bounds;
    public Vector2 padding = new Vector2(1f, 1f);

    private void Awake()
    {
        instance = this;
        _cam = GetComponent<Camera>();
        pixcam = GetComponent<PixelPerfectCamera>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (bounds == null && Room.activeRoom != null)
        { bounds = Room.activeRoom.roomBounds; }
        FollowTarget();
    }

    

    void FollowTarget()
    {
        Vector2 tpos = transform.position;
        Vector2 moveTo = CameraTarget.activeTarget != null ? CameraTarget.targetTransform.position : targetPos;

        if (bounds != null) moveTo = ClampInRect(moveTo, GetCamClampRect(), padding);

        Vector2 dist = moveTo - tpos; 
        Vector2 dir = dist.normalized;
        float d = dist.magnitude;
        float s = Mathf.Max(1f, d * cameraSpeed);

        Vector2 npos = tpos + Vector2.ClampMagnitude(dir * s * Time.deltaTime, d);

        transform.position = new Vector3(npos.x, npos.y, transform.position.z);
        return;
        
    }

    Rect GetCamClampRect()
    {
        float y = _cam.orthographicSize * 2;
        Vector2 s = new Vector2(y * _cam.aspect, y);
        return new Rect(bounds.position + s / 2, bounds.size - s); 
    }
    Vector2 ClampInRect(Vector2 p, Rect r, Vector2 padding)
    { return new Vector2(Mathf.Clamp(p.x, r.xMin+padding.x, r.xMax-padding.x), Mathf.Clamp(p.y, r.yMin+padding.y, r.yMax-padding.y)); }

    public float SineSlider(float d)
    { return Mathf.Sin(d*deg90); }

    public void OnSceneLoaded()
    {
        if (Player.instance != null)
        { followTarget = Player.instance.transform; }
    }
}
