using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class Utility
{
    public static void DrawDebugRay(this Vector2 vec, Vector2 origin, Color color)
    { Debug.DrawRay(origin, vec, color); }
    public static void DrawDebugRay(this Vector2 vec, Vector2 origin, float mult, Color color)
    { Debug.DrawRay(origin, vec * mult, color); }

    // Gizmos
    public static void DrawGizmo(this Rect rect, Vector2 offset)
    {
        Gizmos.DrawRay(rect.min, new Vector2(rect.width, 0));
        Gizmos.DrawRay(rect.min, new Vector2(0, rect.height));
        Gizmos.DrawRay(rect.max, new Vector2(-rect.width, 0));
        Gizmos.DrawRay(rect.max, new Vector2(0, rect.height));
    }
    public static void DrawGizmoArrow(Vector2 position, Vector2 direction, float tipSize = 0.2f)
    {
        Vector2 epos = position + direction;
        Gizmos.DrawLine(position, epos);
        Vector2 atip = direction.normalized.Turn(135) * tipSize;
        Gizmos.DrawRay(epos, atip);
        Gizmos.DrawRay(epos, atip.Turn90CCW());
    }
    public static void DrawGizmoCircle(Vector2 position, float radius, int segments = 12)
    {

        float a_one = _rad360 / segments;
        float anow;
        Vector2 npos;
        Vector2 lpos = new Vector2(position.x + radius, position.y);
        for (int i = 1; i <= segments; i++)
        {
            anow = a_one * i;
            npos = position + new Vector2(Mathf.Cos(anow), Mathf.Sin(anow)) * radius;
            Gizmos.DrawLine(lpos, npos);
            lpos = npos;
        }
    }


}
