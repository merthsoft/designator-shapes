using HarmonyLib;
using Verse;

namespace Merthsoft.DesignatorShapes.Patches
{
    [HarmonyPatch(typeof(Designator), nameof(Finalize), new[] { typeof(bool) })]
    internal class Designator_Finalize
    {
        public static void Postfix(bool somethingSucceeded)
        {
            if (!DesignatorShapes.ShowControls)
                return;
            HistoryManager.FinishBuilding();
        }
    }
}
