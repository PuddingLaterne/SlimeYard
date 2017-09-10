using UnityEngine;
using System;
using System.Collections.Generic;

public static  class ExtensionMethods
{
    public static bool Approximately(this float value, float targetValue, float epsilon = 0.001f)
    {
        return Mathf.Abs(value - targetValue) < epsilon;
    }

    public static Vector2 Rotate(this Vector2 v, float degrees)
    {
        float sin = Mathf.Sin(degrees * Mathf.Deg2Rad);
        float cos = Mathf.Cos(degrees * Mathf.Deg2Rad);

        float tx = v.x;
        float ty = v.y;
        v.x = (cos * tx) - (sin * ty);
        v.y = (sin * tx) + (cos * ty);
        return v;
    }

    public static float GetAngle(this Vector2 v)
    {
        return CalculateAngle(v, Vector2.up);
    }

    public static float GetAngleBetween(this Vector2 a, Vector2 b)
    {
        return CalculateAngle(a, b);
    }

    private static float CalculateAngle(Vector2 a, Vector2 b)
    {
        a = a.normalized;
        b = b.normalized;
        return Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg * 2f;
    }

    public static float Ratio(this float a, float b)
    {
        return a / (a + b);
    }

    public static float SignedSquared(this float value)
    {
        return Mathf.Sign(value) * Mathf.Pow(value, 2);
    }

    public static int ToBitmask(this int x)
    {
        return (int)Mathf.Pow(2, x);
    }

    public static bool IsPartOfBitmask(this int x, int mask)
    {
        return (x & mask) != 0;
    }

    public static int[] Shorten(this int[] array, int targetLength)
    {
        int[] shortenedArray = new int[targetLength];
        Array.Copy(array, shortenedArray, targetLength);
        return shortenedArray;
    }

    public static bool Contains(this int[] array, int value)
    {
        foreach (var element in array)
        {
            if (element == value) return true;
        }
        return false;
    }

    public static bool HasParameter(this Animator anim, string parameterName)
    {
        foreach(AnimatorControllerParameter parameter in anim.parameters)
        {
            if(parameter.name == parameterName)
            {
                return true;
            }
        }
        return false;
    }

    public static float GetAnimationClipLength(this Animator anim, string clipName)
    {
        RuntimeAnimatorController animatorController = anim.runtimeAnimatorController;
        foreach (AnimationClip clip in animatorController.animationClips)
        {
            if(clip.name == clipName)
            {
                return clip.length;
            }
        }
        return 0f;
    }

    public static void SetLayersRecursively(this GameObject gameObject, int layer)
    {
        gameObject.layer = layer;
        foreach(Transform child in gameObject.transform)
        {
            child.gameObject.layer = layer;
        }
    }

    public static Vector3[] GetPositions3D(this List<Point> points)
    {
        Vector3[] positions = new Vector3[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            positions[i] = points[i].Position;
        }
        return positions;
    }

    public static Vector2[] GetPositions2D(this List<Point> points)
    {
        Vector2[] positions = new Vector2[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            positions[i] = points[i].Position;
        }
        return positions;
    }
}
