using System.Collections.Generic;
using Verse;

namespace Merthsoft.DesignatorShapes.Shapes
{
    internal class StampShapes
    {
        public static IEnumerable<IntVec3> SunLamp(IntVec3 vert1, IntVec3 vert2) =>
            Primitives.GenRadialCircle(vert1, DesignatorShapes.SunLampRadius, true);

        public static IEnumerable<IntVec3> SunLampOutline(IntVec3 vert1, IntVec3 vert2) =>
            Primitives.GenRadialCircle(vert1, DesignatorShapes.SunLampRadius, false);

        public static IEnumerable<IntVec3> TradeBeacon(IntVec3 vert1, IntVec3 vert2) =>
            Primitives.GenRadialCircle(vert1, DesignatorShapes.TradeBeaconRadius, true);

        public static IEnumerable<IntVec3> TradeBeaconOutline(IntVec3 vert1, IntVec3 vert2) =>
            Primitives.GenRadialCircle(vert1, DesignatorShapes.TradeBeaconRadius, false);
    }
}
