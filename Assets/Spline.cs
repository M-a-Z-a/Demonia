using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spline : MonoBehaviour
{

    public Vector2 start_origin, start_direction, end_origin, end_direction;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(start_origin, start_direction);
        Gizmos.DrawRay(end_origin, end_direction);

        Vector2 lpoint = start_origin, cpoint;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < 11; i++)
        {
            cpoint = GetPointAtCurve(0.1f * i);
            Gizmos.DrawLine(lpoint, cpoint);
            lpoint = cpoint;
        }
    }


    public Vector2 GetPointAtCurve(float delta)
    {
        float dneg = 1f - delta;

        Vector2 dist = end_origin - start_origin;
        float distmag = dist.magnitude;

        float y = Mathf.Sin(delta * 2 - 1f), yneg = 2 - y;

        Vector2 p = start_direction * y + end_direction * yneg;
        return start_origin + dist * delta + p;
    }
}
