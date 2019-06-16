using Harmony;
using Merthsoft.DesignatorShapes.Defs;
using RimWorld;
using System.Collections;
using System.Linq;
using Verse;

namespace Merthsoft.DesignatorShapes.Patches {
    [HarmonyPatch(typeof(DesignatorManager), "Select", new[] { typeof(Designator) })]
    class DesignatorManager_Select {
        public static void Prefix(ref Designator __state) {
            if (!DesignatorShapes.ShowControls) { return; }

            __state = Find.DesignatorManager.SelectedDesignator;
        }

        public static void Postfix(DesignatorManager __instance, Designator des, ref Designator __state) {
            if (!DesignatorShapes.ShowControls) { return; }
            
            var selectedDesignator = __instance.SelectedDesignator;
            if (selectedDesignator == null) { return; }
            if (__state == selectedDesignator) { return; }
                
            DesignatorShapeDef shape = null;
            var announce = true;

            if (DesignatorShapes.Settings.AutoSelectShape || DesignatorShapes.CachedTool == null) {
                switch (selectedDesignator.DraggableDimensions) {
                    default:
                    case 1:
                        shape = DesignatorShapeDefOf.Rectangle;
                        break;
                    case 2:
                        shape = DesignatorShapeDefOf.RectangleFilled;
                        break;
                }
            } else {
                shape = DesignatorShapes.CachedTool;
            }

            DesignatorShapes.SelectTool(shape, announce);

            if (DesignatorShapes.Settings.UseOldUi && DesignatorShapes.Settings.ShowShapesPanelOnDesignationSelection) {
                var archWindow = (MainTabWindow_Architect)MainButtonDefOf.Architect.TabWindow;
                var panels = archWindow?.GetInstanceField("desPanelsCached") as IEnumerable;

                if (panels != null) {
                    archWindow.selectedDesPanel = panels.Cast<ArchitectCategoryTab>().FirstOrDefault(p => p.def.defName == "Shapes");
                }
            }
        }
    }
}
