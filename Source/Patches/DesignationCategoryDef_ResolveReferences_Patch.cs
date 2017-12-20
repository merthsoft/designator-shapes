using System;
using Harmony;
using Merthsoft.DesignatorShapes.Defs;
using Merthsoft.DesignatorShapes.Designators;
using Verse;

namespace Merthsoft.DesignatorShapes.Patches {
    [HarmonyPatch(typeof(DesignationCategoryDef), "ResolveReferences")]
    public class DesignationCategoryDef_ResolveReferences_Patch {
        public static void Postfix(DesignationCategoryDef __instance) {
            if (__instance.defName == "Shapes") {
                var shapeDefs = DefDatabase<DesignatorShapeDef>.AllDefsListForReading;
                shapeDefs.ForEach(d => __instance.AllResolvedDesignators.Add(new Designator_Shape(d)));
            }
        }
    }
}