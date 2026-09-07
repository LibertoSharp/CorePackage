using System.Collections.Generic;
using UnityEngine;

public static class VectorExtensions
{
    public static Vector3 ToXZ(this Vector2 v) => new Vector3(v.x, 0, v.y);
     public static Vector2 ToXY(this Vector3 v) => new Vector2(v.x, v.z);
    public static Vector2 SwapXY(this Vector2 v) => new Vector2(v.y, v.x);
    public static Vector3 WithY(this Vector3 v, float newY) => new Vector3(v.x, newY, v.z);
    public static Vector3 Avarage(this IEnumerable<Vector3> vecs)
    {
        Vector3 result = Vector3.zero;
        int count = 0;

        foreach (Vector3 v in vecs)
        {
            result += v;
            count++;
        }

        return count == 0 ? Vector3.zero : result / count;
    }

    public static Vector2Int ToInt(this Vector2 v) => new Vector2Int((int)v.x, (int)v.y);

    public static Vector3 WithCenter(this Vector3 v, Vector3 center) => v - center;
}