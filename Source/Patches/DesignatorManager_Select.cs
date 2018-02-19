using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Verse;
using Merthsoft.DesignatorShapes.Designators;
using Merthsoft.DesignatorShapes.Defs;
using System.Collections;

namespace Merthsoft.DesignatorShapes.Patches {
    [HarmonyPatch(typeof(DesignatorManager), "Select", new[] { typeof(Designator) })]
    class DesignatorManager_Select {
        public static void Prefix(ref Designator __state) {
            __state = Find.DesignatorManager.SelectedDesignator;
        }

        public static void Postfix(DesignatorManager __instance, Designator des, ref Designator __state) {
            var selectedDesignator = __instance.SelectedDesignator;
            if (selectedDesignator == null) { return; }
            if (__state == selectedDesignator) { return; }
                
            DesignatorShapeDef shape = null;
            var announce = true;

            switch (selectedDesignator.DraggableDimensions) {
                default:
                    shape = DesignatorShapeDefOf.Line;
                    announce = false;
                    break;
                case 1:
                    shape = DesignatorShapeDefOf.Rectangle;
                    break;
                case 2:
                    shape = DesignatorShapeDefOf.RectangleFilled;
                    break;
            }

            DesignatorShapes.SelectTool(shape, announce);

            if (DesignatorShapes.GlobalSettings.ShowShapesPanelOnDesignationSelection) {
                var archWindow = (MainTabWindow_Architect)MainButtonDefOf.Architect.TabWindow;
                var panels = archWindow?.GetInstanceField("desPanelsCached") as IEnumerable;

                if (panels != null) {
                    archWindow.selectedDesPanel = panels.Cast<ArchitectCategoryTab>().FirstOrDefault(p => p.def.defName == "Shapes");
                }
            }
        }
    }
}
