using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Merthsoft.DesignatorShapes.Shapes;

// Pending corners only: no map mutation until the user explicitly finishes.
public sealed class PolygonStroke
{
    public const int MaximumVertices = 128;
    private readonly List<IntVec3> vertices = new();
    public IReadOnlyList<IntVec3> Vertices => vertices;
    public int Version { get; private set; }
    public bool CanFinish => vertices.Count >= 3 && vertices.Skip(2).Any(p =>
        (long)(vertices[1].x-vertices[0].x)*(p.z-vertices[0].z)
        != (long)(vertices[1].z-vertices[0].z)*(p.x-vertices[0].x));
    private bool Fits(IntVec3 point) => vertices.Count == 0 ||
        (long)Math.Max(vertices.Max(p=>p.x),point.x)-Math.Min(vertices.Min(p=>p.x),point.x)<512
        && (long)Math.Max(vertices.Max(p=>p.z),point.z)-Math.Min(vertices.Min(p=>p.z),point.z)<512;
    public bool TryAdd(IntVec3 point)
    {
        if(vertices.Count>=MaximumVertices || vertices.Contains(point) || !Fits(point)) return false;
        vertices.Add(point);Version++;return true;
    }
    public void RemoveLast() { if(vertices.Count==0)return;vertices.RemoveAt(vertices.Count-1);Version++; }
    public void Clear() { if(vertices.Count==0)return;vertices.Clear();Version++; }
    public IEnumerable<IntVec3> Preview(IntVec3 cursor,bool filled,int thickness,bool fillCorners)
    {
        if(vertices.Count==0)return Array.Empty<IntVec3>();
        var points=new List<IntVec3>(vertices);
        if(points.Count<MaximumVertices && cursor!=points[0] && cursor!=points[^1] && Fits(cursor))points.Add(cursor);
        return Primitives.Polygon(points,filled,thickness,fillCorners);
    }
    public IEnumerable<IntVec3> Finish(bool filled,int thickness,bool fillCorners)
        => CanFinish ? Primitives.Polygon(vertices,filled,thickness,fillCorners) : Array.Empty<IntVec3>();
}
