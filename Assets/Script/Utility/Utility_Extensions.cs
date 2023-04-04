using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static partial class Utility
{
    public static Rect Rect(this BoxCollider2D coll)
    { return new Rect(coll.offset + coll.size / 2, coll.size); }
    public static void SetRect(this Rect rect, BoxCollider2D coll)
    { rect.position = coll.offset; rect.size = coll.size; }
    public static Vector2 GetHalfSize(this Rect rect)
    { return new Vector2(rect.width / 2, rect.height / 2); }

    public static float[] Add(this float[] arr, float add)
    {
        for (int i = 0; i < arr.Length; i++)
        { arr[i] += add; }
        return arr;
    }
    public static float[] Mult(this float[] arr, float mult)
    {
        for (int i = 0; i < arr.Length; i++)
        { arr[i] *= mult; }
        return arr;
    }
    public static float[] Div(this float[] arr, float div)
    {
        for (int i = 0; i < arr.Length; i++)
        { arr[i] /= div; }
        return arr;
    }
}
