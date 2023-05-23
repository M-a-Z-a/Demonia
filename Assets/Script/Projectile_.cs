using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static Utility;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Projectile_ : MonoBehaviour
{
    public UnityEvent<Projectile_, RaycastHit2D> onColliderEnter = new(), onColliderExit = new();

    public Vector2 velocity = Vector2.right;
    public LayerMask layerMask;
    public float distance = 50, distleft;
    public bool turnTowardsVelocity = true;
    public float turnRateMin = 15f, turnRateMax = 75f;
    [SerializeField] TrailRenderer trail;
    public bool stop = false;

    private void Awake()
    {
        //trail = GetComponent<TrailRenderer>();
    }
    // Start is called before the first frame update
    void Start()
    {
        distleft = distance;
        if (turnTowardsVelocity) TurnTowardsVelocityInstant();
    }

    // Update is called once per frame
    void Update()
    {
        if (stop)
        { return; }

        if (UpdatePosition(Vector2.Distance(Vector2.zero, velocity * Time.deltaTime)) <= 0)
        {
            //StartCoroutine(IQueDestroy()); 
            GetComponent<Renderer>().enabled = false;
            stop = true;
            Invoke("DestroyThis", trail.time);
        }
        TurnTowardsVelocity();
    }

    void DestroyThis()
    { Destroy(gameObject); }

    public void TurnTowardsVelocityInstant()
    {
        Vector3 eul = transform.eulerAngles;
        eul.z = Vector2.SignedAngle(transform.right, velocity);
        transform.eulerAngles = eul;
    }

    void TurnTowardsVelocity()
    {
        if (turnTowardsVelocity)
        {
            if (velocity != Vector2.zero)
            {
                float a = Vector2.SignedAngle(transform.right, velocity);
                if (a != 0)
                {
                    float tspeed = 15f + a / 180 * 75f;
                    Vector3 eul = transform.eulerAngles;
                    eul.z = Mathf.LerpAngle(eul.z, a, Mathf.Min(tspeed / a * Time.deltaTime, 1f));
                    transform.eulerAngles = eul;
                }
            }
        }
    }

    RaycastHit2D rhit;
    float UpdatePosition(float dist)
    {
        float dot;
        Vector2 rpos = transform.position;
        Vector2 vnorm;
        float minmove = 0.1f;
        while (dist > 0)
        {
            vnorm = velocity.normalized;
            if ((rhit = Physics2D.Raycast(rpos, vnorm, dist, layerMask)).collider != null)
            {
                
                distleft -= dist -= rhit.distance+minmove;
                rpos = rhit.point + vnorm*minmove;
                if ((dot = DotProduct(vnorm, rhit.normal)) < 0)
                {
                    Debug.Log ($"IN (dot: {dot})");
                    Debug.DrawRay(rhit.point, rhit.normal, Color.red, 1f);
                    onColliderEnter.Invoke(this, rhit);
                }
                else if (dot > 0)
                {
                    Debug.Log($"OUT (dot: {dot})");
                    Debug.DrawRay(rhit.point, rhit.normal, Color.green, 1f);
                    onColliderExit.Invoke(this, rhit);
                }
                continue;
            }
            
            rpos += vnorm * dist;
            distleft -= dist; dist = 0;
        }
        transform.position = rpos;
        return distleft / distance;
    }



}


#if UNITY_EDITOR
[CustomEditor(typeof(Projectile_))]
public class ProjectileEditor : Editor
{
    Projectile_ instance;
    Transform t;

    private void OnEnable()
    {
        instance = (Projectile_)target;
        t = instance.transform;
    }

    /*
    private void OnSceneGUI()
    {
        
        Handles.BeginGUI();

        Handles.color = Color.yellow;
        Handles.DrawLine(t.position, t.position + (Vector3)instance.direction, 0.1f);
        Handles.DrawWireArc(t.position, Vector3.forward, Vector3.right, Vector2.Angle(Vector2.right, instance.direction), 2f);
        Handles.color = Color.red;
        Handles.DrawLine(t.position, t.position + (Vector3)instance.velocity, 0.1f);
        Handles.DrawWireArc(t.position, Vector3.forward, Vector3.right, Vector2.Angle(Vector2.right, instance.velocity), 2.5f);

        Handles.EndGUI();
        //Repaint();
        
    }
    */

}
#endif
