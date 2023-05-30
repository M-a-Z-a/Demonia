using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

public class PlasmaBall : Entity
{

    public LayerMask layerMask;

    [SerializeField] Vector2 initVelocity = Vector2.zero;
    public float centerRadius = 0.25f, shockRadius = 2;
    float maxMag = 99f;
    [SerializeField] int shockCount = 12;

    List<Entity> entsInRange = new();
    List<LineRenderer> lrend = new();
    [SerializeField] Material shockMaterial;

    protected override void Awake()
    {
        base.Awake();
        shockCount = Mathf.FloorToInt((float)shockCount / 2) * 2;
    }
    protected override void Start()
    {
        base.Start();
        LineRenderer lr = GetComponent<LineRenderer>();
        for (int i = 0; i < shockCount; i++)
        {
            //lr = LineManager.CreateLineRenderer();
            lr.positionCount = 10;
            lr.widthMultiplier = 1f;
            lr.enabled = false;
            lr.material = new Material(shockMaterial);
            lr.material.SetFloat("_Width", 0.1f);
            lr.material.SetFloat("_Intensity", 2f);
            lrend.Add(lr);
        }
        _velocity = initVelocity;
        StartCoroutine(ILerpMaxMag(0.5f, 5f));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Utility.DrawGizmoArrow(transform.position, _velocity, 0.25f);
        Gizmos.color = Color.cyan;
        Utility.DrawGizmoCircle(transform.position, centerRadius);
        Gizmos.color = new Color(0.5f, 0, 0.75f);
        Utility.DrawGizmoCircle(transform.position, shockRadius);
    }
    protected override void OnValidate()
    {
        base.OnValidate();
        _velocity = initVelocity;
    }


    private void Update()
    {
        RaycastCircle();
        RaycastHit2D rhit = Physics2D.Raycast(transform.position, _velocity.normalized, centerRadius*2, layerMask);
        if (rhit.collider)
        { _velocity += rhit.normal * 8f * Time.deltaTime; }
        

        if (_velocity == Vector2.zero) return;

        Vector2.ClampMagnitude(_velocity, maxMag);
        transform.position = transform.position.Add(_velocity.x * Time.deltaTime, _velocity.y * Time.deltaTime);
        _velocity = Utility.TowardsTargetVector(_velocity, Vector2.zero, 0.05f * Time.deltaTime);
    }


    void RaycastCircle()
    {
        Vector2 tpos = transform.position;
        float a = _velocity.SignedAngle();
        float add = 360f / 12;
        float newa, shockrange = shockRadius-centerRadius;
        float rdhalf;
        Vector2 veca, vecs;
        RaycastHit2D rhit;
        int ic; int iadd = -1;
        for (int i = 0; i < shockCount; i+=2)
        {
            iadd++;

            ic = i;
            newa = (a + add * iadd) * Mathf.Deg2Rad;
            CastRay(newa);

            ic++; 
            newa = (a - add * iadd) * Mathf.Deg2Rad;
            CastRay(newa);
        }
        return;

        void CastRay(float fangle)
        {
            veca = new Vector2(Mathf.Cos(fangle), Mathf.Sin(fangle));
            vecs = tpos + veca * centerRadius;
            rhit = Physics2D.Raycast(vecs, veca, shockrange, layerMask);
            Debug.DrawRay(tpos, veca * centerRadius, Color.cyan);
            if (rhit.collider)
            {
                rdhalf = rhit.distance / 2;
                if (rhit.distance > centerRadius)
                { _velocity += (rhit.point - tpos).normalized * 0.01f * Time.deltaTime; }
                Debug.DrawLine(vecs, rhit.point, new Color(0.5f, 0f, 0.75f));
                lrend[ic].enabled = true;
                lrend[ic].SetPositions(CreateShockCurve(lrend[ic].positionCount, vecs, veca * rdhalf, rhit.point, rhit.normal * rdhalf).Cast<Vector3>().ToArray());
                return;
            }
            lrend[ic].enabled = false;
            Debug.DrawRay(vecs, veca * shockrange, Color.blue);
        }
    }

    IEnumerable<Vector3> CreateShockCurve(int segments, Vector3 p1, Vector3 d1, Vector3 p2, Vector3 d2)
    {
        segments = Mathf.Max(2, segments);
        float r = segments - 1, t;
        Vector3 scurve, prang = p2 - p1;
        for (int i = 0; i < segments; i++)
        {
            t = (float)i / r;
            scurve = Utility.SineVector(t);
            yield return (p1 + prang * scurve.x) + (d1 * (1f - scurve.x) * scurve.y) + (d2 * scurve.x * scurve.y);
        }
    }

    IEnumerator ILerpMaxMag(float target_speed, float time)
    {
        float spd = _velocity.magnitude;
        float t = 0;
        while (t < time)
        {
            t += Time.deltaTime;
            maxMag = Mathf.Lerp(spd, target_speed, t / time);
            yield return null;
        }
        maxMag = target_speed;
    }


    private void OnDestroy()
    {
        for (int i = 0; i < lrend.Count; i++)
        {
            if (lrend[i] != null)
            { Destroy(lrend[i].gameObject); }
        }
    }


    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.TryGetComponent<Entity>(out Entity ent))
        {
            if (!entsInRange.Contains(ent))
            { entsInRange.Add(ent); }
            if (entsInRange.Count == 1)
            { processEntitiesCoroutine = StartCoroutine(IProcessEntitiesInRange()); }
        }
    }
    private void OnTriggerExit2D(Collider2D coll)
    {
        if (coll.gameObject.TryGetComponent<Entity>(out Entity ent))
        { 
            entsInRange.Remove(ent); 
        }
    }

    Coroutine processEntitiesCoroutine;
    IEnumerator IProcessEntitiesInRange()
    {
        while (entsInRange.Count > 0)
        {
            foreach (Entity ent in entsInRange)
            {
                Vector2 dist = ent.transform.position - transform.position;
                Vector2 dnorm = dist.normalized;
                float magndelta = Mathf.Clamp(dist.magnitude / 4f, 0f, 1f);
                _velocity += (1f - magndelta) * 0.25f * Time.deltaTime * dnorm;
                ent.AddForce((1f - magndelta) * 0.5f * Time.deltaTime * dnorm);
                Debug.DrawLine(transform.position, ent.transform.position, Color.Lerp(Color.red, Color.blue, magndelta));
            }
            yield return null;
        }
        processEntitiesCoroutine = null;
    }

}
