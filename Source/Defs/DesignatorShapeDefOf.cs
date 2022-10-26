using System;
using RimWorld;

namespace Merthsoft.DesignatorShapes.Defs
{
    [DefOf]
    public static class DesignatorShapeDefOf
    {
        public static DesignatorShapeDef Line;
        public static DesignatorShapeDef Rectangle;
        public static DesignatorShapeDef RectangleFilled;
        public static DesignatorShapeDef Ellipse;
        public static DesignatorShapeDef EllipseFilled;
        public static DesignatorShapeDef Hexagon;
        public static DesignatorShapeDef HexagonFilled;
        public static DesignatorShapeDef Circle;
        public static DesignatorShapeDef CircleFilled;
        // public static DesignatorShapeDef FloodFill;

        static DesignatorShapeDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(DesignatorShapeDefOf));
            DefOfHelper.EnsureInitializedInCtor(typeof(OverlayGroupDefOf));
        }
    }

    [DefOf]
    public static class OverlayGroupDefOf
    {
        public static OverlayGroupDef Line;
        public static OverlayGroupDef Rectangle;
        public static OverlayGroupDef Rectangle_Input;
        public static OverlayGroupDef Ellipse;
        public static OverlayGroupDef Ellipse_Input;
        public static OverlayGroupDef MoreShapes;
        public static OverlayGroupDef FloodFill;
        public static OverlayGroupDef Etc;

        static OverlayGroupDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(DesignatorShapeDefOf));
            DefOfHelper.EnsureInitializedInCtor(typeof(OverlayGroupDefOf));
        }
    }
}
