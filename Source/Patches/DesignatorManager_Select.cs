using HarmonyLib;
using Merthsoft.DesignatorShapes.Defs;
using RimWorld;
using System.Linq;
using Verse;

namespace Merthsoft.DesignatorShapes.Patches;

[HarmonyPatch(typeof(DesignatorManager), "Select", new[] { typeof(Designator) })]
internal class DesignatorManager_Select
{
    public static void Prefix(ref Designator __state)
    {
        if (!DesignatorShapes.ShowControls)
            return;
        __state = Find.DesignatorManager.SelectedDesignator;
    }

    public static void Postfix(DesignatorManager __instance, ref Designator __state)
    {
        if (!DesignatorShapes.ShowControls)
            return;
        var selectedDesignator = __instance.SelectedDesignator;
        if (selectedDesignator == null)
            return;
        if (__state == selectedDesignator)
            return;

        var shape = DesignatorShapes.Settings.AutoSelectShape || DesignatorShapes.CachedTool == null
                    ? selectedDesignator.DrawStyleCategory?.defName switch 
                        {
                            "Walls" => DesignatorShapeDefOf.Rectangle,
                            "Conduits" => DesignatorShapeDefOf.Line,
                            "Defenses" => DesignatorShapeDefOf.Rectangle,
                            null => DesignatorShapes.CachedTool ?? DesignatorShapeDefOf.RectangleFilled,
                            _ => DesignatorShapeDefOf.RectangleFilled,
                        }
                    : DesignatorShapes.CachedTool;
        DesignatorShapes.SelectTool(shape);
    }
}
