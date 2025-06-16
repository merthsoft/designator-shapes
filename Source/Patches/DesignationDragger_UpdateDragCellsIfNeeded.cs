using HarmonyLib;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes.Patches;

[HarmonyPatch(typeof(DesignationDragger), "UpdateDragCellsIfNeeded")]
public static class DesignationDragger_UpdateDragCellsIfNeeded
{
    public static bool Prefix(DesignationDragger __instance)
    {
        if (!DesignatorShapes.ShowControls)
            return true;
        if (__instance == null)
            return false;
        if (Time.frameCount == (int)__instance.GetInstanceField("lastUpdateFrame"))
            return false;

        if (DesignatorShapes.CurrentTool == null)
            return true;
        __instance.SetInstanceField("lastUpdateFrame", Time.frameCount);
        __instance.DragCells.Clear();
        __instance.SetInstanceField<string>("failureReasonInt", null);

        var start = (IntVec3)__instance.GetInstanceField("startDragCell");
        var sizeOrEnd = DesignatorShapes.CurrentTool.useSizeInputs ? Merthsoft.DesignatorShapes.Ui.ShapeControlsWindow.InputVec : UI.MouseCell();

        var points = DesignatorShapes.CurrentTool?.DrawMethod(start, sizeOrEnd);

        foreach (var vec in points)
        {
            __instance.InvokeMethod("TryAddDragCell", vec);
        }

        return false;
    }
}
