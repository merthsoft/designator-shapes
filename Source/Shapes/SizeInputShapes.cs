using System.Collections.Generic;
using Verse;
using static Merthsoft.DesignatorShapes.Shapes;

namespace Merthsoft.DesignatorShapes {
    public static class SizeInputShapes {
        public static IEnumerable<IntVec3> Rectangle(IntVec3 center, IntVec3 size) =>
            Shapes.Rectangle(center - size.Halve(), center + size.Halve());

        public static IEnumerable<IntVec3> RectangleFilled(IntVec3 center, IntVec3 size) =>
            Shapes.RectangleFilled(center - size.Halve(), center + size.Halve());

        public static IEnumerable<IntVec3> Ellipse(IntVec3 center, IntVec3 size) =>
            RadialEllipse(center.x, center.y, center.z, size.x / 2, size.z / 2, false, DesignatorShapes.Thickness);

        public static IEnumerable<IntVec3> EllipseFilled(IntVec3 center, IntVec3 size) =>
            RadialEllipse(center.x, center.y, center.z, size.x / 2, size.z / 2, true, DesignatorShapes.Thickness);
    }
}
