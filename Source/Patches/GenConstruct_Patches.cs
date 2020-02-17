using HarmonyLib;
using RimWorld;

namespace Merthsoft.DesignatorShapes.Patches {
    [HarmonyPatch(typeof(GenConstruct), "PlaceBlueprintForBuild")]
    class GenConstruct_PlaceBlueprintForBuild {
        public static void Postfix(Blueprint_Build __result) {
            if (!DesignatorShapes.ShowControls) { return; }

            HistoryManager.AddEntry(__result);
        }
    }
}
