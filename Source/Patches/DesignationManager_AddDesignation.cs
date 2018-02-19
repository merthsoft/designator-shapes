using System;
using Harmony;
using Verse;

namespace Merthsoft.DesignatorShapes.Patches {
    [HarmonyPatch(typeof(DesignationManager), "AddDesignation")]
    public class DesignationManager_AddDesignation {
        public static void Postfix(Designation newDes, DesignationManager __instance) {
            //if (newDes.def.targetType == TargetType.Cell && __instance.DesignationAt(newDes.target.Cell, newDes.def) != null) {
            //    return;
            //}
            //if (newDes.def.targetType == TargetType.Thing && __instance.DesignationOn(newDes.target.Thing, newDes.def) != null) {
            //    return;
            //}

            HistoryManager.AddEntry(newDes);
        }
    }
}
