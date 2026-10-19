using HarmonyLib;
using RimWorld;

namespace Merthsoft.DesignatorShapes.Patches;

[HarmonyPatch(typeof(GenConstruct), "PlaceBlueprintForBuild")]
internal class GenConstruct_PlaceBlueprintForBuild
{
    public static void Postfix(Blueprint_Build __result)
    {
        if (!DesignatorShapes.UseCustomGeometry && !Shapes.CustomPolygon.Committing)
            return;
        HistoryManager.AddEntry(__result);
    }
}
