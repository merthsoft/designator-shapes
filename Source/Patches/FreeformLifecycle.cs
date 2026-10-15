using HarmonyLib;
using Verse;
using Merthsoft.DesignatorShapes.Shapes;
namespace Merthsoft.DesignatorShapes.Patches;

[HarmonyPatch(typeof(DesignationDragger), "StartDrag")]
internal static class StartFreeformStroke { static void Prefix() => FreeformLine.FreeMemory(); }
[HarmonyPatch(typeof(DesignationDragger), "EndDrag")]
internal static class EndFreeformStroke { static void Postfix() => FreeformLine.FreeMemory(); }
[HarmonyPatch(typeof(DesignatorManager), "set_SelectedStyle")]
internal static class ChangeFreeformStyle { static void Prefix() => FreeformLine.FreeMemory(); }
[HarmonyPatch(typeof(Game), "InitNewGame")]
internal static class NewFreeformGame { static void Prefix() => FreeformLine.FreeMemory(); }
[HarmonyPatch(typeof(Game), "LoadGame")]
internal static class LoadFreeformGame { static void Prefix() => FreeformLine.FreeMemory(); }
[HarmonyPatch(typeof(DesignationDragger), "UpdateCellBuffer")]
internal static class UpdateNativeFreeform
{
    static bool Prefix(DesignationDragger __instance, IntVec3 ___startDragCell)
    {
        if (DesignatorShapes.ShowControls || Find.DesignatorManager.SelectedStyle?.DrawStyleWorker is not Shapes.DrawStyle.FreeformLine style) return true;
        style.Update(___startDragCell, UI.MouseCell(), __instance.CellBuffer); return false;
    }
}
