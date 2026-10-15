using System;
using Merthsoft.DesignatorShapes.Defs;
namespace Merthsoft.DesignatorShapes;

public static class ShapeEligibility
{
    public static bool IsDrawable(DesignatorShapeDef shape)
    {
        if (shape == null || shape.defName is "Undo" or "Redo" || string.IsNullOrEmpty(shape.drawMethodName)) return false;
        try { return shape.DrawMethod != null; } catch (ArgumentException) { return false; } catch (Exception) { return false; }
    }
}
