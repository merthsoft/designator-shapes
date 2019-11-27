using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using static Merthsoft.DesignatorShapes.Shapes;

namespace Merthsoft.DesignatorShapes {
    public static class SizeInputShapes {
        public static IEnumerable<IntVec3> Rectangle(IntVec3 center, IntVec3 size, int rotation) =>
            Shapes.Rectangle(center - size.Halve(), center + size.Halve(), rotation);

        public static IEnumerable<IntVec3> RectangleFilled(IntVec3 center, IntVec3 size, int rotation) =>
            Shapes.RectangleFilled(center - size.Halve(), center + size.Halve(), rotation);

        public static IEnumerable<IntVec3> Ellipse(IntVec3 center, IntVec3 size, int rotation) =>
            RadialEllipse(center.x, center.y, center.z, size.x / 2, size.z / 2, false);

        public static IEnumerable<IntVec3> EllipseFilled(IntVec3 center, IntVec3 size, int rotation) =>
            RadialEllipse(center.x, center.y, center.z, size.x / 2, size.z / 2, true);
    }
}
