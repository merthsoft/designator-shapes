using Verse;

namespace Merthsoft.DesignatorShapes;

internal class HistoryComponent : GameComponent
{
    public static int TickRate = 50;
    private int tickNumber = TickRate;

    public HistoryComponent(Game _)
    {
    }

    public override void GameComponentTick()
    {
        if (tickNumber != 0)
        {
            tickNumber--;
            return;
        }
        tickNumber = TickRate;

        HistoryManager.Clear();

        if (DesignatorShapes.Settings.ResetShapeOnResume)
            DesignatorShapes.CachedTool = null;
    }

    public override void ExposeData()
    {
        base.ExposeData();

        var cachedTool = DesignatorShapes.CachedTool;
        Scribe_Defs.Look(ref cachedTool, nameof(DesignatorShapes.CachedTool));
        DesignatorShapes.CachedTool = cachedTool;
    }
}
