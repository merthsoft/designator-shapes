using HarmonyLib;
using Verse;

namespace Merthsoft.DesignatorShapes.Patches;

[HarmonyPatch(typeof(Game), "FinalizeInit")]
public static class Game_FinalizeInit
{
    public static void Postfix() =>
        //var harmony = DesignatorShapes.HarmonyInstance;
        //var architectTab = MainButtonDefOf.Architect.TabWindow;
        //var original = architectTab.GetType().GetMethod("ExtraOnGUI");
        //var prefix = typeof(DesignatorShapes).GetMethod("LoadDefs");

        //harmony.Patch(original, new HarmonyMethod(prefix), null);
        DesignatorShapes.LoadDefs();
}