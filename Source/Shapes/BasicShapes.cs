using System.Collections.Generic;
using Verse;

namespace Merthsoft.DesignatorShapes.Shapes;

public static class BasicShapes
{
    public static IEnumerable<IntVec3> Line(IntVec3 vert1, IntVec3 vert2) =>
        Primitives.Line(vert1, vert2, DesignatorShapes.Thickness, DesignatorShapes.FillCorners);

    public static IEnumerable<IntVec3> Rectangle(IntVec3 s, IntVec3 t) =>
        Primitives.Rectangle(s.x, s.y, s.z, t.x, t.y, t.z, false, DesignatorShapes.Rotation, DesignatorShapes.Thickness, DesignatorShapes.FillCorners);

    public static IEnumerable<IntVec3> RectangleFilled(IntVec3 s, IntVec3 t) =>
        Primitives.Rectangle(s.x, s.y, s.z, t.x, t.y, t.z, true, DesignatorShapes.Rotation, DesignatorShapes.Thickness, DesignatorShapes.FillCorners);

    public static IEnumerable<IntVec3> Pentagon(IntVec3 s, IntVec3 t) =>
        Primitives.Pentagon(s.x, s.y, s.z, t.x, t.y, t.z, false, DesignatorShapes.Rotation, DesignatorShapes.Thickness, DesignatorShapes.FillCorners);

    public static IEnumerable<IntVec3> PentagonFilled(IntVec3 s, IntVec3 t) =>
        Primitives.Pentagon(s.x, s.y, s.z, t.x, t.y, t.z, true, DesignatorShapes.Rotation, DesignatorShapes.Thickness, DesignatorShapes.FillCorners);

    public static IEnumerable<IntVec3> Hexagon(IntVec3 s, IntVec3 t) =>
        Primitives.Hexagon(s.x, s.y, s.z, t.x, t.y, t.z, false, DesignatorShapes.Rotation, DesignatorShapes.Thickness, DesignatorShapes.FillCorners);

    public static IEnumerable<IntVec3> HexagonFilled(IntVec3 s, IntVec3 t) =>
        Primitives.Hexagon(s.x, s.y, s.z, t.x, t.y, t.z, true, DesignatorShapes.Rotation, DesignatorShapes.Thickness, DesignatorShapes.FillCorners);
    
    public static IEnumerable<IntVec3> MidpointHexagon(IntVec3 s, IntVec3 t) =>
            Primitives.MidpointHexagon(s, t, false, DesignatorShapes.Rotation, DesignatorShapes.Thickness, DesignatorShapes.FillCorners);

    public static IEnumerable<IntVec3> MidpointHexagonFilled(IntVec3 s, IntVec3 t) =>
        Primitives.MidpointHexagon(s, t, true, DesignatorShapes.Rotation, DesignatorShapes.Thickness, DesignatorShapes.FillCorners);

    public static IEnumerable<IntVec3> Ellipse(IntVec3 s, IntVec3 t) =>
        Primitives.Oval(s, t, false, DesignatorShapes.Thickness, DesignatorShapes.FillCorners);

    public static IEnumerable<IntVec3> EllipseFilled(IntVec3 s, IntVec3 t) =>
        Primitives.Oval(s, t, true, DesignatorShapes.Thickness, DesignatorShapes.FillCorners);

    public static IEnumerable<IntVec3> Circle(IntVec3 s, IntVec3 t) =>
        Primitives.Circle(s, t, false, DesignatorShapes.Thickness, DesignatorShapes.FillCorners);

    public static IEnumerable<IntVec3> CircleFilled(IntVec3 s, IntVec3 t) =>
        Primitives.Circle(s, t, true, DesignatorShapes.Thickness, DesignatorShapes.FillCorners);
}
