using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Helper
{
    public static List<T> CreateList<T>(int capacity, T value = default(T))
    {
        List<T> coll = new List<T>(capacity);
        for (int i = 0; i < capacity; i++)
            coll.Add(value);

        return coll;
    }

    public static List<T> Clone<T>(this IList<T> listToClone) where T : ICloneable
    {
        return listToClone.Select(item => (T)item.Clone()).ToList();
    }

    public static bool PointInTriangle(Vector3 pt, List<Vector3> trianglePoints)
    {
        Vector3 v1 = trianglePoints[0];
        Vector3 v2 = trianglePoints[1];
        Vector3 v3 = trianglePoints[2];
        float d1, d2, d3;
        bool has_neg, has_pos;

        d1 = sign(pt, v1, v2);
        d2 = sign(pt, v2, v3);
        d3 = sign(pt, v3, v1);

        has_neg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        has_pos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(has_neg && has_pos);
    }

    private static float sign(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        return (p1.x - p3.x) * (p2.z - p3.z) - (p2.x - p3.x) * (p1.z - p3.z);
    }
}
