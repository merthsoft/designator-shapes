using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Merthsoft.DesignatorShapes.Patches {
    [HarmonyPatch(typeof(GenConstruct), "PlaceBlueprintForBuild")]
    class GenConstruct_PlaceBlueprintForBuild {
        public static void Postfix(Blueprint_Build __result) {
            HistoryManager.AddEntry(__result);
        }
    }
}
