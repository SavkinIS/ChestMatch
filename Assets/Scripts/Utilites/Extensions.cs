using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static double Lerp(double a, double b, float t)
    {
        return a + (b - a) * Mathf.Clamp01(t);
    }
    
    public static void SetActiveOptimize(this GameObject go, bool state)
    {
        if (go != null && go.activeSelf != state) go.SetActive(state);
    }

    public static void SetActiveOptimize(this Component comp, bool state)
    {
        if (comp == null) return;
        var go = comp.gameObject;
        if (go != null && go.activeSelf != state) go.SetActive(state);
    }

    public static void Fill<T>(this IList<T> collection, T value)
    {
        for (int i = collection.Count - 1; i >= 0; --i)
        {
            collection[i] = value;
        }
    }

    [System.Serializable]
    public class BytesData
    {
        public byte[] Data;

        public BytesData(byte[] data)
        {
            Data = data;
        }
    }
}