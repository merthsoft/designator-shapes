using HarmonyLib;
using Merthsoft.DesignatorShapes.Shapes;
using Verse;

namespace Merthsoft.DesignatorShapes.Patches;

[HarmonyPatch(typeof(Designator), nameof(Finalize), new[] { typeof(bool) })]
internal class Designator_Finalize
{
    public static void Postfix(bool somethingSucceeded)
    {
        FreeformLine.FreeMemory();
        if (!DesignatorShapes.ShowControls)
            return;
        HistoryManager.FinishBuilding();
    }
}
