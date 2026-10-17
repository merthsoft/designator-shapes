using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Merthsoft.DesignatorShapes.Shapes;

// Drag bounds are inclusive. Limit sampling independently of cursor distance.
public static class AdditionalShapes
{
    public const int MaximumSpan = 512;
    private static (int x, int z, int w, int h) Bounds(IntVec3 s, IntVec3 t)
        => (Math.Min(s.x, t.x), Math.Min(s.z, t.z),
            (int)Math.Min(MaximumSpan, Math.Abs((long)t.x - s.x) + 1),
            (int)Math.Min(MaximumSpan, Math.Abs((long)t.z - s.z) + 1));

    public static IEnumerable<IntVec3> Octagon(IntVec3 s, IntVec3 t) => OctagonCells(s, t, false);
    public static IEnumerable<IntVec3> OctagonFilled(IntVec3 s, IntVec3 t) => OctagonCells(s, t, true);
    public static IEnumerable<IntVec3> MidpointOctagon(IntVec3 center, IntVec3 edge) => MidpointOctagonCells(center, edge, false);
    public static IEnumerable<IntVec3> MidpointOctagonFilled(IntVec3 center, IntVec3 edge) => MidpointOctagonCells(center, edge, true);
    private static IEnumerable<IntVec3> MidpointOctagonCells(IntVec3 center, IntVec3 edge, bool filled)
    {
        // Match midpoint hexagons: the larger X/Z offset defines the radius.
        // An odd diameter preserves the exact center even at the sampling limit.
        int radius = (int)Math.Min((MaximumSpan - 1) / 2,
            Math.Max(Math.Abs((long)edge.x - center.x), Math.Abs((long)edge.z - center.z)));
        return OctagonCells(new IntVec3(center.x - radius, center.y, center.z - radius),
            new IntVec3(center.x + radius, center.y, center.z + radius), filled);
    }
    private static IEnumerable<IntVec3> OctagonCells(IntVec3 s, IntVec3 t, bool filled)
    {
        var (x, z, w, h) = Bounds(s, t);
        var cut = (int)Math.Round((Math.Min(w, h) - 1) * (1 - 1 / Math.Sqrt(2)));
        var vertices = new[] { new IntVec3(x + cut,s.y,z), new IntVec3(x+w-1-cut,s.y,z),
            new IntVec3(x+w-1,s.y,z+cut), new IntVec3(x+w-1,s.y,z+h-1-cut),
            new IntVec3(x+w-1-cut,s.y,z+h-1),new IntVec3(x+cut,s.y,z+h-1),
            new IntVec3(x,s.y,z+h-1-cut),new IntVec3(x,s.y,z+cut) };
        bool Inside(IntVec3 p) => p.x>=x && p.x<x+w && p.z>=z && p.z<z+h
            && p.x-x+p.z-z>=cut && x+w-1-p.x+p.z-z>=cut
            && p.x-x+z+h-1-p.z>=cut && x+w-1-p.x+z+h-1-p.z>=cut;
        if (filled)
        {
            var cells = new List<IntVec3>();
            for(int dz=0;dz<h;dz++) for(int dx=0;dx<w;dx++)
            { var p=new IntVec3(x+dx,s.y,z+dz); if(Inside(p)) cells.Add(p); }
            return cells;
        }
        var outline = new HashSet<IntVec3>();
        for(int i=0;i<vertices.Length;i++) outline.UnionWith(Primitives.Line(vertices[i],vertices[(i+1)%vertices.Length],DesignatorShapes.Thickness,DesignatorShapes.FillCorners));
        return DesignatorShapes.Thickness>0 ? outline.Where(Inside).ToList() : outline;
    }
    public static IEnumerable<IntVec3> Checkerboard(IntVec3 s, IntVec3 t) => Checker(s,t,0);
    public static IEnumerable<IntVec3> CheckerboardInverse(IntVec3 s, IntVec3 t) => Checker(s,t,1);
    private static IEnumerable<IntVec3> Checker(IntVec3 s,IntVec3 t,int phase)
    {
        var (x,z,w,h)=Bounds(s,t);
        var tile = Ui.ShapeControlsWindow.CheckerTileSize;
        int tileWidth = Math.Max(1, Math.Min(MaximumSpan, tile.x));
        int tileHeight = Math.Max(1, Math.Min(MaximumSpan, tile.z));
        for(int dz=0;dz<h;dz++) for(int dx=0;dx<w;dx++)
            if(((dx / tileWidth + dz / tileHeight)&1)==phase) yield return new IntVec3(x+dx,s.y,z+dz);
    }
    public static IEnumerable<IntVec3> L(IntVec3 s,IntVec3 t) => Corner(s,t,0);
    public static IEnumerable<IntVec3> LInverse(IntVec3 s,IntVec3 t) => Corner(s,t,2);
    private static IEnumerable<IntVec3> Corner(IntVec3 s,IntVec3 t,int phase)
    {
        var (x,z,w,h)=Bounds(s,t);
        var corners=new[]{new IntVec3(x,s.y,z),new IntVec3(x,s.y,z+h-1),new IntVec3(x+w-1,s.y,z+h-1),new IntVec3(x+w-1,s.y,z)};
        var r=((DesignatorShapes.Rotation+phase)%4+4)%4;
        return Primitives.Line(corners[r],corners[(r+1)%4],DesignatorShapes.Thickness,DesignatorShapes.FillCorners)
            .Union(Primitives.Line(corners[r],corners[(r+3)%4],DesignatorShapes.Thickness,DesignatorShapes.FillCorners)).ToList();
    }
    public static IEnumerable<IntVec3> QuarterArc(IntVec3 s,IntVec3 t) => Arc(s,t,false);
    public static IEnumerable<IntVec3> SemicircleArc(IntVec3 s,IntVec3 t) => Arc(s,t,true);
    private static IEnumerable<IntVec3> Arc(IntVec3 s,IntVec3 t,bool half)
    {
        var (x,z,w,h)=Bounds(s,t);
        var cells=new HashSet<IntVec3>();
        var steps=Math.Max(1,Math.Max(w,h)*4);
        IntVec3? previous=null;
        for(int i=0;i<=steps;i++)
        {
            double angle=(half?Math.PI:Math.PI/2)*i/steps;
            double u=half?(Math.Cos(angle)+1)/2:Math.Cos(angle), v=Math.Sin(angle);
            for(int r=0;r<((DesignatorShapes.Rotation%4+4)%4);r++) (u,v)=(v,1-u);
            var p=new IntVec3(x+(int)Math.Round(u*(w-1)),s.y,z+(int)Math.Round(v*(h-1)));
            cells.UnionWith(Primitives.Line(previous??p,p,DesignatorShapes.Thickness,DesignatorShapes.FillCorners));
            previous=p;
        }
        return cells;
    }
}
