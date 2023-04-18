using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class CameraControl : MonoBehaviour
{
    public static CameraControl instance { get; protected set; }
    public static Camera cam { get => instance._cam; }
    Camera _cam;
    PixelPerfectCamera pixcam;

    public float cameraSpeed = 16;
    public Transform followTarget;

    Vector2 targetPos;
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

        transform.position = new Vector3(npos.x, npos.y, transform.position.z);
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
}
