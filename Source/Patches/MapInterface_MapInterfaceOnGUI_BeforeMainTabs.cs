using Harmony;
using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Merthsoft.DesignatorShapes.Patches {
    [HarmonyPatch(typeof(MapInterface), "MapInterfaceOnGUI_BeforeMainTabs")]
    public static class MapInterface_MapInterfaceOnGUI_BeforeMainTabs {
        public static void Postfix(MapInterface __instance) {
            if (Find.CurrentMap == null 
                || Find.DesignatorManager.SelectedDesignator == null
                || WorldRendererUtility.WorldRenderedNow) {
                return;
            }

            DesignatorShapes.ShapeControls?.ShapeControlsOnGUI();
        }
    }
}
