using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Vector utility
public static partial class Utility
{
    // Vector2 shorthand math
    public static Vector2 Add(this Vector2 vec, float x = 0, float y = 0)
    { return new Vector2(vec.x + x, vec.y + y); }
    public static Vector2 Mult(this Vector3 vec, float x = 1, float y = 1)
    { return new Vector2(vec.x * x, vec.y * y); }
    public static Vector2 Div(this Vector2 vec, float x = 1, float y = 1)
    { return new Vector2(vec.x / x, vec.y / y); }

    // Vector3 shorthand math
    public static Vector3 Add(this Vector3 vec, float x = 0, float y = 0, float z = 0)
    { return new Vector3(vec.x + x, vec.y + y, vec.z + z); }
    public static Vector3 Mult(this Vector3 vec, float x = 1, float y = 1, float z = 1)
    { return new Vector3(vec.x * x, vec.y * y, vec.z * z); }
    public static Vector3 Div(this Vector3 vec, float x = 1, float y = 1, float z = 1)
    { return new Vector3(vec.x / x, vec.y / y, vec.z / z); }



    public static Vector2 TowardsTargetVector(Vector2 a, Vector2 b, Vector2 add)
    { return new Vector2(TowardsTargetValue(a.x, b.x, add.x), TowardsTargetValue(a.y, b.y, add.y)); }
    public static Vector2 TowardsTargetVector(Vector2 a, Vector2 b, float add)
    {
        Vector2 _add = (b - a).Abs().normalized * add;
        return new Vector2(TowardsTargetValue(a.x, b.x, _add.x), TowardsTargetValue(a.y, b.y, _add.y));
    }
    public static Vector2 Vector2Clamp(Vector2 vec, Vector2 min, Vector2 max)
    { return new Vector2(Mathf.Clamp(vec.x, min.x, max.x), Mathf.Clamp(vec.y, min.x, max.x)); }
    public static Vector2 Vector2Abs(Vector2 vec)
    { return new Vector2(Mathf.Abs(vec.x), Mathf.Abs(vec.y)); }

    public static Vector2 TowardsTarget(this Vector2 vec, Vector2 target, Vector2 value)
    { return TowardsTargetVector(vec, target, value); }
    public static Vector2 Clamp(this Vector2 vec, Vector2 min, Vector2 max)
    { return Vector2Clamp(vec, min, max); }
    public static Vector2 Abs(this Vector2 vec)
    { return Vector2Abs(vec); }
    public static Vector2 Redirect(this Vector2 vec, Vector2 dir)
    { float mag = vec.magnitude; return dir * mag; }
    public static Vector2 Redirect(this Vector2 vec, Vector2 dir, float delta)
    { float mag = vec.magnitude * delta; return dir * mag; }


    public static Vector2 MoveAndRedirect(this Vector2 vec, float distance, Vector2 redirect, float delta)
    {
        float mag = vec.magnitude;
        float lmin = Mathf.Min(distance, mag);
        float leftov = distance - lmin;
        Vector2 nvec = vec.normalized * lmin;
        if (leftov > 0) return nvec + redirect * leftov;
        return nvec;
    }

}
