using HarmonyLib;
using Verse;

namespace Merthsoft.DesignatorShapes.Patches;

[HarmonyPatch(typeof(DesignatorManager), "ProcessInputEvents")]
internal static class DesignatorManager_ProcessInputEvents
{
    public static bool Prefix(DesignatorManager __instance) => ShapeShortcuts.Handle(__instance) && FloodFillConfirmation.BeforeInput(__instance);

    public static void Postfix(DesignatorManager __instance)
    {
        if (!DesignatorShapes.UseCustomGeometry)
            return;
        if (__instance.SelectedDesignator == null
            || DesignatorShapes.CurrentTool == null
            || __instance.Dragger == null)
            return;

        if (!DesignatorShapes.CurrentTool.AllowDragging && __instance.Dragger.Dragging)
            __instance.Dragger.SetInstanceField("startDragCell", UI.MouseCell());
    }
}
