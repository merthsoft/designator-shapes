using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace Merthsoft;

public static class Extensions
{
    private static readonly BindingFlags FieldFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.GetField | BindingFlags.GetProperty;

    public static bool AsBool(this object obj) => obj is bool b && b;

    public static (IntVec3 upperLeft, IntVec3 bottomRight) GetBounds(this IEnumerable<IntVec3> cells)
    {
        var enumerator = cells.GetEnumerator();
        if (!enumerator.MoveNext())
            return (IntVec3.Invalid, IntVec3.Invalid);

        var first = enumerator.Current;
        var minX = first.x;
        var maxX = first.x;
        var minZ = first.z;
        var maxZ = first.z;
        var y = first.y;

        while (enumerator.MoveNext())
        {
            var c = enumerator.Current;
            if (c.x < minX) minX = c.x;
            if (c.x > maxX) maxX = c.x;
            if (c.z < minZ) minZ = c.z;
            if (c.z > maxZ) maxZ = c.z;
        }

        return (new IntVec3(minX, y, minZ), new IntVec3(maxX, y, maxZ));
    }

    private static Dictionary<string, FieldInfo> InstanceFieldCache = [];

    public static T GetInstanceField<T>(this object instance, string fieldName)
    {
        var type = instance.GetType();
        var key = $"{type.Name}::{fieldName}";

        if (!InstanceFieldCache.TryGetValue(key, out var field))
        {
            field = type.GetField(fieldName, FieldFlags);
            if (field == null)
            {
                Log.ErrorOnce($"Field {fieldName} not found in {type.Name}.", Guid.NewGuid().GetHashCode());
                return default;
            }
            InstanceFieldCache[key] = field;
        }

        return (T)(field.GetValue(instance));
    }

    public static void SetInstanceField<T>(this object instance, string fieldName, T value)
    {
        var type = instance.GetType();
        var key = $"{type}::{fieldName}";

        if (!InstanceFieldCache.TryGetValue(key, out var field))
        {
            field = type.GetField(fieldName, FieldFlags);
            InstanceFieldCache[key] = field;
        }

        field.SetValue(instance, value);
    }

    public static void InvokeMethod(this object obj, string methodName, params object[] methodParams)
    {
        var dynMethod = obj.GetType().GetMethod(methodName, FieldFlags);
        dynMethod.Invoke(obj, methodParams);
    }

    public static List<TResult> SelectList<T, TResult>(this IEnumerable<T> i, Func<T, TResult> f) => i.Select(f).ToList();

    public static IntVec3 FloorHalve(this IntVec3 vec) => new(vec.x / 2, vec.y / 2, vec.z / 2);

    public static IntVec3 CeilingHalve(this IntVec3 vec) => new(vec.x.CeilingHalve() - 1, vec.y.CeilingHalve() - 1, vec.z.CeilingHalve() - 1);

    public static int CeilingHalve(this int i) => (int)Math.Ceiling(i / 2d);

    public static IEnumerable<int> Range(this int count, int startValue = 0, int? by = null)
    {
        if (!by.HasValue)
        {
            by = count.Sign();
            count = count.Magnitude();
        }

        for (var i = 0; i < count; i++)
        {
            yield return startValue;
            startValue += by.Value;
        }
    }

    public static int Magnitude(this int i)
        => Math.Abs(i);

    public static int Sign(this int i)
        => Math.Sign(i);

    public static float Add(this float lhs, float? rhs)
        => lhs + rhs.GetValueOrDefault();

    public static Rect Clone(this Rect r,
        float? x = null,
        float? y = null,
        float? width = null,
        float? height = null,
        float? addX = null,
        float? addY = null,
        float? addWidth = null,
        float? addHeight = null)
        => new(x ?? r.x.Add(addX),
               y ?? r.y.Add(addY),
               width ?? r.width.Add(addWidth),
               height ?? r.height.Add(addHeight));
                
}
