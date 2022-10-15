using System.Collections.Generic;
using System;
using Verse;

namespace Merthsoft.DesignatorShapes.Shapes
{
    public static class Snaps
    {
        public static IntVec3 Angles(IntVec3 vert1, IntVec3 vert2, int numAngles) {
            var angle = Math.Atan2(vert2.z - vert1.z, vert2.x - vert1.x);
            var dist = Math.Sqrt((vert2.z - vert1.z) * (vert2.z - vert1.z) + (vert2.x - vert1.x) * (vert2.x - vert1.x));
            var newAngle = Math.Round(angle / Math.PI / 2 * numAngles) * 2 * Math.PI / numAngles;
            return new IntVec3((int) Math.Round(vert1.x + Math.Cos(newAngle) * dist), vert1.y, (int) Math.Round(vert1.z + Math.Sin(newAngle) * dist));
        }
        public static IntVec3 Line(IntVec3 vert1, IntVec3 vert2) => Angles(vert1, vert2, 24);

        public static IntVec3 Rectangle(IntVec3 s, IntVec3 t) => Angles(s, t, 24);

        public static IntVec3 Pentagon(IntVec3 s, IntVec3 t) => Angles(s, t, 30);

        public static IntVec3 Hexagon(IntVec3 s, IntVec3 t) => Angles(s, t, 24);

        public static IntVec3 Ellipse(IntVec3 s, IntVec3 t) => Angles(s, t, 24);

    }
}
