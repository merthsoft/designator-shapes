using System;
using Harmony;
using RimWorld;
using Verse;

namespace Merthsoft.DesignatorShapes.Patches {
    [HarmonyPatch(typeof(Game), "FinalizeInit")]
    public static class Game_FinalizeInit_Patch {
        public static void Postfix() {
            var harmony = DesignatorShapes.Harmony;
            var architectTab = MainButtonDefOf.Architect.TabWindow;
            var original = architectTab.GetType().GetMethod("ExtraOnGUI");
            var prefix = typeof(DesignatorShapes).GetMethod("LoadDefs");

            harmony.Patch(original, new HarmonyMethod(prefix), null);
        }
    }
}
