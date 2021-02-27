using HarmonyLib;
using Merthsoft.DesignatorShapes.Defs;
using Verse;

namespace Merthsoft.DesignatorShapes.Patches
{
    [HarmonyPatch(typeof(DesignatorManager), "Select", new[] { typeof(Designator) })]
    class DesignatorManager_Select {
        public static void Prefix(ref Designator __state) {
            if (!DesignatorShapes.ShowControls) { return; }

            __state = Find.DesignatorManager.SelectedDesignator;
        }

        public static void Postfix(DesignatorManager __instance, ref Designator __state) {
            if (!DesignatorShapes.ShowControls) { return; }
            
            var selectedDesignator = __instance.SelectedDesignator;
            if (selectedDesignator == null) { return; }
            if (__state == selectedDesignator) { return; }
                
            DesignatorShapeDef shape;
            var announce = true;

            if (DesignatorShapes.Settings.AutoSelectShape || DesignatorShapes.CachedTool == null) {
                shape = selectedDesignator.DraggableDimensions switch
                {
                    2 => DesignatorShapeDefOf.RectangleFilled,
                    _ => DesignatorShapeDefOf.Rectangle,
                };
            } else {
                shape = DesignatorShapes.CachedTool;
            }

            DesignatorShapes.SelectTool(shape, announce);
        }
    }
}
