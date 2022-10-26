﻿using HarmonyLib;

using Merthsoft.DesignatorShapes.Utils;

using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Merthsoft.DesignatorShapes.Patches
{
    [HarmonyPatch(typeof(MapInterface), "MapInterfaceOnGUI_AfterMainTabs")]
    public static class MapInterface_MapInterfaceOnGUI_AfterMainTabs
    {
        public static void Postfix(MapInterface __instance)
        {
            if (DesignatorShapes.Settings.RestoreAltToggle && !DesignatorShapes.ShowControls)
                return;
            if (Find.CurrentMap == null
                || Find.DesignatorManager.SelectedDesignator == null
                || WorldRendererUtility.WorldRenderedNow)
                return;

            DesignatorShapes.ShapeControls?.ShapeControlsOnGUI();
        }
    }
}
