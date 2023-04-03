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
}
