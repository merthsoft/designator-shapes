using System;
using Merthsoft.DesignatorShapes.Defs;
using RimWorld;
using UnityEngine;
using Verse;
namespace Merthsoft.DesignatorShapes;

internal static class DrawingBounds
{
    internal static bool CanSnap(DesignatorShapeDef shape) => shape != null && shape.AllowDragging
        && shape.defName is not ("Line" or "LineFreeform" or "FreeformLine" or "Circle" or "CircleFilled")
        && !shape.defName.StartsWith("Midpoint", StringComparison.Ordinal);

    internal static (IntVec3 start, IntVec3 end) Constrain(DesignatorShapeDef shape, IntVec3 start, IntVec3 end, bool held)
    {
        if (!held || !CanSnap(shape)) return (start, end);
        // Match DesignationDragger.GetCurrentBoundary: keep the drag start fixed and
        // extend the shorter side, preserving direction (including zero-axis drags).
        var delta = end - start;
        var span = Math.Abs(delta.x) >= Math.Abs(delta.z) ? delta.x : delta.z;
        end.x = start.x + (Mathf.Approximately(Mathf.Sign(delta.x), Mathf.Sign(span)) ? span : -span);
        end.z = start.z + (Mathf.Approximately(Mathf.Sign(delta.z), Mathf.Sign(span)) ? span : -span);
        return (start, end);
    }

    internal static (IntVec3 start, IntVec3 end) Current(DesignatorShapeDef shape, IntVec3 start, IntVec3 end)
        => Constrain(shape, start, end, KeyBindingDefOf.Designator_ShapeSnap?.IsDown == true);
}
