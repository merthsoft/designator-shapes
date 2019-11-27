using System.Collections.Generic;
using Verse;

namespace Merthsoft.DesignatorShapes.Shapes {
    public static class BasicShapes {
        public static IEnumerable<IntVec3> Rectangle(IntVec3 s, IntVec3 t) =>
            PrimitiveShapes.Rectangle(s.x, s.y, s.z, t.x, t.y, t.z, false, DesignatorShapes.Rotation, DesignatorShapes.Thickness);

        public static IEnumerable<IntVec3> RectangleFilled(IntVec3 s, IntVec3 t) =>
            PrimitiveShapes.Rectangle(s.x, s.y, s.z, t.x, t.y, t.z, true, DesignatorShapes.Rotation, DesignatorShapes.Thickness);

        public static IEnumerable<IntVec3> Pentagon(IntVec3 s, IntVec3 t) =>
            PrimitiveShapes.Pentagon(s.x, s.y, s.z, t.x, t.y, t.z, false, DesignatorShapes.Rotation);

        public static IEnumerable<IntVec3> PentagonFilled(IntVec3 s, IntVec3 t) =>
            PrimitiveShapes.Pentagon(s.x, s.y, s.z, t.x, t.y, t.z, true, DesignatorShapes.Rotation);

        public static IEnumerable<IntVec3> Hexagon(IntVec3 s, IntVec3 t) =>
            PrimitiveShapes.Hexagon(s.x, s.y, s.z, t.x, t.y, t.z, false, DesignatorShapes.Rotation);

        public static IEnumerable<IntVec3> HexagonFilled(IntVec3 s, IntVec3 t) =>
            PrimitiveShapes.Hexagon(s.x, s.y, s.z, t.x, t.y, t.z, true, DesignatorShapes.Rotation);

        public static IEnumerable<IntVec3> Ellipse(IntVec3 s, IntVec3 t) =>
            PrimitiveShapes.Ellipse(s.x, s.y, s.z, t.x, t.y, t.z, false, DesignatorShapes.Thickness);

        public static IEnumerable<IntVec3> EllipseFilled(IntVec3 s, IntVec3 t) =>
            PrimitiveShapes.Ellipse(s.x, s.y, s.z, t.x, t.y, t.z, true, DesignatorShapes.Thickness);

        public static IEnumerable<IntVec3> Circle(IntVec3 s, IntVec3 t) =>
            PrimitiveShapes.Circle(s, t, false, DesignatorShapes.Thickness);

        public static IEnumerable<IntVec3> CircleFilled(IntVec3 s, IntVec3 t) =>
            PrimitiveShapes.Circle(s, t, true, DesignatorShapes.Thickness);
    }
}
