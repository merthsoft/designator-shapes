using System;
using Harmony;
using Verse;

namespace Merthsoft.DesignatorShapes.Patches {
    [HarmonyPatch(typeof(DesignationManager), "AddDesignation")]
    public class DesignationManager_AddDesignation {
        public static void Postfix(Designation newDes, DesignationManager __instance) {
            HistoryManager.AddEntry(newDes);
        }
    }
}
