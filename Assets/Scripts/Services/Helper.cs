using System;
using System.Collections;
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
}
