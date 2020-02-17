using HarmonyLib;
using Verse;

namespace Merthsoft.DesignatorShapes.Patches {
    [HarmonyPatch(typeof(Designator), "Finalize", new[] { typeof(bool) })]
    class Designator_Finalize {
        public static void Postfix(bool somethingSucceeded) {
            if (!DesignatorShapes.ShowControls) { return; }

            HistoryManager.FinishBuilding();
        }
    }
}
