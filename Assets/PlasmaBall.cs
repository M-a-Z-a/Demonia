using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class PlasmaBall : MonoBehaviour
{

    public LayerMask layerMask;

    [SerializeField] Vector2 velocity = Vector2.zero;
    public float centerRadius = 0.25f, shockRadius = 2;
    float maxMag = 99f;
    SplineComponent spline;

    private void Awake()
    {
        spline = GetComponentInChildren<SplineComponent>();
    }
    private void Start()
    {
        StartCoroutine(ILerpMaxMag(0.5f, 5f));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Utility.DrawGizmoArrow(transform.position, velocity, 0.25f);
        Gizmos.color = Color.cyan;
        Utility.DrawGizmoCircle(transform.position, centerRadius);
        Gizmos.color = new Color(0.5f, 0, 0.75f);
        Utility.DrawGizmoCircle(transform.position, shockRadius);
    }


    private void Update()
    {
        RaycastCircle();
        if (velocity == Vector2.zero) return;

        Vector2.ClampMagnitude(velocity, maxMag);
        transform.position = transform.position.Add(velocity.x * Time.deltaTime, velocity.y * Time.deltaTime);
        velocity = Utility.TowardsTargetVector(velocity, Vector2.zero, 0.1f * Time.deltaTime);
    }

    void RaycastCircle()
    {
        Vector2 tpos = transform.position;
        float a = velocity.SignedAngle();
        float add = 360f / 12;
        float newa, shockrange = shockRadius-centerRadius;
        Vector2 veca, vecs;
        RaycastHit2D rhit;

        for (int i = 1; i < 6; i++)
        {
            newa = (a + add * i) * Mathf.Deg2Rad;
            CastRay(newa);

            newa = (a - add * i) * Mathf.Deg2Rad;
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
                velocity += (rhit.point - tpos).normalized * 0.01f * Time.deltaTime;
                Debug.DrawLine(vecs, rhit.point, new Color(0.5f, 0f, 0.75f));
                return;
            }
            Debug.DrawRay(vecs, veca * shockrange, Color.blue);
        }
    }

    IEnumerator ILerpMaxMag(float target_speed, float time)
    {
        float spd = velocity.magnitude;
        float t = 0;
        while (t < time)
        {
            t += Time.deltaTime;
            maxMag = Mathf.Lerp(spd, target_speed, t / time);
            yield return null;
        }
        maxMag = target_speed;
    }

}