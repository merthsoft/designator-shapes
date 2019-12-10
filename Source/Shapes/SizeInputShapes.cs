using System.Collections.Generic;
using Verse;

namespace Merthsoft.DesignatorShapes.Shapes {
    public static class SizeInputShapes {
        public static IEnumerable<IntVec3> Rectangle(IntVec3 center, IntVec3 size) =>
            BasicShapes.Rectangle(center - size.Halve(), center + size.Halve());

        public static IEnumerable<IntVec3> RectangleFilled(IntVec3 center, IntVec3 size) =>
            BasicShapes.RectangleFilled(center - size.Halve(), center + size.Halve());

        public static IEnumerable<IntVec3> Ellipse(IntVec3 center, IntVec3 size) =>
            Primitives.RadialEllipse(center.x, center.y, center.z, size.x / 2, size.z / 2, false, DesignatorShapes.Thickness, DesignatorShapes.FillCorners);

        public static IEnumerable<IntVec3> EllipseFilled(IntVec3 center, IntVec3 size) =>
            Primitives.RadialEllipse(center.x, center.y, center.z, size.x / 2, size.z / 2, true, DesignatorShapes.Thickness, DesignatorShapes.FillCorners);
    }
}
