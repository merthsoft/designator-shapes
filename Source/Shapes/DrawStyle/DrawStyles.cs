using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Merthsoft.DesignatorShapes.Shapes.DrawStyle;
public class SunLamp : Verse.DrawStyle
{
    public override void Update(IntVec3 origin, IntVec3 target, List<IntVec3> buffer)
    {
        buffer.Clear();
        buffer.AddRange(StampShapes.SunLamp(target, target));
    }
}

public class SunLampOutline : Verse.DrawStyle
{
    public override void Update(IntVec3 origin, IntVec3 target, List<IntVec3> buffer)
    {
        buffer.Clear();
        buffer.AddRange(StampShapes.SunLampOutline(target, target));
    }
}

public class TradeBeacon : Verse.DrawStyle
{
    public override void Update(IntVec3 origin, IntVec3 target, List<IntVec3> buffer)
    {
        buffer.Clear();
        buffer.AddRange(StampShapes.TradeBeacon(target, target));
    }
}

public class TradeBeaconOutline : Verse.DrawStyle
{
    public override void Update(IntVec3 origin, IntVec3 target, List<IntVec3> buffer)
    {
        buffer.Clear();
        buffer.AddRange(StampShapes.TradeBeaconOutline(target, target));
    }
}
public class FreeformLine : Verse.DrawStyle
{
    public override void Update(IntVec3 origin, IntVec3 target, List<IntVec3> buffer)
    {
        buffer.Clear();
        buffer.AddRange(Shapes.FreeformLine.Line(origin, target));
    }
}

public class Circle : Verse.DrawStyle
{
    public override void Update(IntVec3 origin, IntVec3 target, List<IntVec3> buffer)
    {
        buffer.Clear();
        buffer.AddRange(BasicShapes.Circle(origin, target));
    }
}

public class CircleFilled : Verse.DrawStyle
{
    public override void Update(IntVec3 origin, IntVec3 target, List<IntVec3> buffer)
    {
        buffer.Clear();
        buffer.AddRange(BasicShapes.CircleFilled(origin, target));
    }
}

public class Hexagon : Verse.DrawStyle
{
    public override void Update(IntVec3 origin, IntVec3 target, List<IntVec3> buffer)
    {
        buffer.Clear();
        buffer.AddRange(BasicShapes.Hexagon(origin, target));
    }
}

public class HexagonFilled : Verse.DrawStyle
{
    public override void Update(IntVec3 origin, IntVec3 target, List<IntVec3> buffer)
    {
        buffer.Clear();
        buffer.AddRange(BasicShapes.HexagonFilled(origin, target));
    }
}

public class MidpointHexagon : Verse.DrawStyle
{
    public override void Update(IntVec3 origin, IntVec3 target, List<IntVec3> buffer)
    {
        buffer.Clear();
        buffer.AddRange(BasicShapes.MidpointHexagon(origin, target));
    }
}

public class MidpointHexagonFilled : Verse.DrawStyle
{
    public override void Update(IntVec3 origin, IntVec3 target, List<IntVec3> buffer)
    {
        buffer.Clear();
        buffer.AddRange(BasicShapes.MidpointHexagonFilled(origin, target));
    }
}