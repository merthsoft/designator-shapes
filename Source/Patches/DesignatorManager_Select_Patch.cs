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

namespace Merthsoft.DesignatorShapes.Patches {
    [HarmonyPatch(typeof(DesignatorManager), "Select", new[] { typeof(Designator) })]
    class DesignatorManager_Select_Patch {
        public static void Postfix(DesignatorManager __instance, Designator des) {
            var selectedDesignator = __instance.SelectedDesignator;
            if (selectedDesignator == null) { return; }

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

            var archWindow = (MainTabWindow_Architect)MainButtonDefOf.Architect.TabWindow;
            var panels = archWindow?.GetInstanceField<List<ArchitectCategoryTab>>("desPanelsCached");
            if (panels != null && DesignatorShapes.globalSettings.ShowShapesPanelOnDesignationSelection) {
                archWindow.selectedDesPanel = panels.Find(p => p.def.defName == "Shapes");
            }
        }
    }
}
