using System;
using System.Collections.Generic;
using System.Linq;
namespace Merthsoft.DesignatorShapes;

public static class RememberedShapeStore
{
    public const int Capacity = 512;
    public static string Key(Type type, string defName) => type == null ? null : type.FullName + (defName == null ? "" : ":" + defName);
    public static void Put(Dictionary<string, string> entries, string key, string shape)
    { if (entries != null && key != null && shape != null && (entries.ContainsKey(key) || entries.Count < Capacity)) entries[key] = shape; }
    public static void Trim(Dictionary<string, string> entries)
    { foreach (var key in entries.Keys.Skip(Capacity).ToArray()) entries.Remove(key); }
}
