using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Merthsoft.DesignatorShapes.Shapes;

internal class InverseGrids
{
    public static IEnumerable<IntVec3> ThreeByThree(IntVec3 s, IntVec3 t)
        => Primitives.InverseGrid(s, t, 3, 3, DesignatorShapes.Thickness);

    public static IEnumerable<IntVec3> FourByFour(IntVec3 s, IntVec3 t)
        => Primitives.InverseGrid(s, t, 4, 4, DesignatorShapes.Thickness);

    public static IEnumerable<IntVec3> FiveByFive(IntVec3 s, IntVec3 t)
        => Primitives.InverseGrid(s, t, 5, 5, DesignatorShapes.Thickness);

    public static IEnumerable<IntVec3> SixBySix(IntVec3 s, IntVec3 t)
        => Primitives.InverseGrid(s, t, 6, 6, DesignatorShapes.Thickness);

    public static IEnumerable<IntVec3> SevenBySeven(IntVec3 s, IntVec3 t)
        => Primitives.InverseGrid(s, t, 7, 7, DesignatorShapes.Thickness);

    public static IEnumerable<IntVec3> SizeInput(IntVec3 s, IntVec3 t)
        => Primitives.InverseGrid(s, t,
            xSpacing: Ui.ShapeControlsWindow.InputVec.x,
            zSpacing: Ui.ShapeControlsWindow.InputVec.z,
            thickness: DesignatorShapes.Thickness);
}
