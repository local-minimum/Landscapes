using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static Vector2 RandomPoint(this Rect r) {
        var x = Random.Range(r.xMin, r.xMax);
        var y = Random.Range(r.yMin, r.yMax);
        return new Vector2(x, y);
    }
}
