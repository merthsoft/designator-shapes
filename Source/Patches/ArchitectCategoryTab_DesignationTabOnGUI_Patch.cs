using System;
using System.Linq;
using Harmony;
using RimWorld;
using Verse;

namespace Merthsoft.DesignatorShapes.Patches {
    [HarmonyPatch(typeof(ArchitectCategoryTab), "DesignationTabOnGUI")]
    public class ArchitectCategoryTab_DesignationTabOnGUI_Patch {
        public static void Prefix(ArchitectCategoryTab __instance) {
            if (__instance.def.defName == "Shapes") {
                (__instance.def as DesignationCategoryDef).ResolveReferences();
            }
        }
    }
}
