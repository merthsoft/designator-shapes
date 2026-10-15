using System;
using System.Collections.Generic;
using Verse;

namespace Merthsoft.DesignatorShapes.Shapes;

// Bounded by map cells, independent of sample frequency. Jacbo's circular brush is retained.
public sealed class FreeformStroke
{
    private readonly HashSet<IntVec3> centers = new();
    private readonly HashSet<IntVec3> cells = new();
    private IntVec3 last;
    private bool started;
    private int thickness;
    private object mapContext, toolContext, styleContext;
    private IntVec3 origin;
    private bool hasContext;
    public bool BeginContext(object map, object tool, object style, IntVec3 start)
    {
        if (hasContext && ReferenceEquals(mapContext, map) && ReferenceEquals(toolContext, tool) && ReferenceEquals(styleContext, style) && origin == start) return false;
        Clear(); hasContext = true; mapContext = map; toolContext = tool; styleContext = style; origin = start; return true;
    }
    public IEnumerable<IntVec3> Cells => cells;
    public int SampleCount => centers.Count;
    public void Clear() { centers.Clear(); cells.Clear(); started = false; thickness = 0; hasContext = false; mapContext = null; toolContext = null; styleContext = null; }
    public void Add(IntVec3 target, int width, int height, int requestedThickness)
    {
        if (width <= 0 || height <= 0) return;
        target = new IntVec3(Math.Clamp(target.x, 0, width - 1), target.y, Math.Clamp(target.z, 0, height - 1));
        int brush = Math.Clamp(requestedThickness, 1, 50);
        if (brush != thickness)
        { thickness = brush; cells.Clear(); foreach (var center in centers) Stamp(center, width, height); }
        if (!started) { last = target; started = true; }
        int steps = Math.Max(Math.Abs(target.x - last.x), Math.Abs(target.z - last.z));
        for (int i = 0; i <= steps; i++)
        {
            double fraction = steps == 0 ? 0 : (double)i / steps;
            var center = new IntVec3((int)Math.Round(last.x + (target.x - last.x) * fraction, MidpointRounding.AwayFromZero), target.y,
                (int)Math.Round(last.z + (target.z - last.z) * fraction, MidpointRounding.AwayFromZero));
            if (centers.Add(center)) Stamp(center, width, height);
        }
        last = target;
    }
    private void Stamp(IntVec3 center, int width, int height)
    {
        int radius = thickness / 2, midpoint = thickness - 1, radiusSquared = thickness * thickness;
        for (int z = 0; z < thickness; z++) for (int x = 0; x < thickness; x++)
        {
            int dx = 2 * x - midpoint, dz = 2 * z - midpoint;
            if (dx * dx + dz * dz > radiusSquared) continue;
            var p = new IntVec3(center.x + x - radius, center.y, center.z + z - radius);
            if (p.x >= 0 && p.z >= 0 && p.x < width && p.z < height) cells.Add(p);
        }
    }
}

public static class FreeformLine
{
    private static readonly FreeformStroke stroke = new();
    internal static void FreeMemory() => stroke.Clear();
    public static IEnumerable<IntVec3> Line(IntVec3 s, IntVec3 t)
    {
        var currentMap = Find.CurrentMap;
        if (currentMap == null) { FreeMemory(); return Array.Empty<IntVec3>(); }
        var manager = Find.DesignatorManager;
        if (!manager.Dragger.Dragging)
        {
            FreeMemory(); stroke.Add(t, currentMap.Size.x, currentMap.Size.z, Math.Max(1, DesignatorShapes.Thickness));
            return stroke.Cells;
        }
        if (stroke.BeginContext(currentMap, manager.SelectedDesignator, manager.SelectedStyle, s))
            stroke.Add(s, currentMap.Size.x, currentMap.Size.z, Math.Max(1, DesignatorShapes.Thickness));
        stroke.Add(t, currentMap.Size.x, currentMap.Size.z, Math.Max(1, DesignatorShapes.Thickness));
        return stroke.Cells;
    }
}
