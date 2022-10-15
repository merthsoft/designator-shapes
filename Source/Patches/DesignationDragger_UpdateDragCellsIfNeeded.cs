using HarmonyLib;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes.Patches
{
    [HarmonyPatch(typeof(DesignationDragger), "UpdateDragCellsIfNeeded")]
    public static class DesignationDragger_UpdateDragCellsIfNeeded
    {
        public static void Prefix(DesignationDragger __instance)
        {
            if (!DesignatorShapes.ShowControls)
                return;
            if (__instance == null)
                return;
            if (Time.frameCount == (int)__instance.GetInstanceField<object>("lastUpdateFrame"))
                return;

            if (DesignatorShapes.CurrentTool == null)
                return;
            __instance.SetInstanceField("lastUpdateFrame", Time.frameCount);
            __instance.DragCells.Clear();
            __instance.SetInstanceField<string>("failureReasonInt", null);

            var start = (IntVec3)__instance.GetInstanceField<object>("startDragCell");
            IntVec3 end;
            if (DesignatorShapes.CurrentTool.useSizeInputs) {
                end = ShapeControls.InputVec;
            }
            else
            {
                end = UI.MouseCell();
                if (DesignatorShapes.SnapMode) {
                    end = DesignatorShapes.CurrentTool?.SnapMethod(start, end) ?? end;
                }
            }
            var points = DesignatorShapes.CurrentTool?.DrawMethod(start, end);

            foreach (var vec in points)
            {
                if (vec == null)
                    continue;
                __instance.InvokeMethod("TryAddDragCell", vec);
            }
        }
    }
}
