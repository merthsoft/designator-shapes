using HarmonyLib;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes.Patches {
    [HarmonyPatch(typeof(DesignationDragger), "UpdateDragCellsIfNeeded")]
    public static class DesignationDragger_UpdateDragCellsIfNeeded {
        public static void Prefix(DesignationDragger __instance) {
            if (!DesignatorShapes.ShowControls) { return; }

            if (__instance == null) { return; }

            if (Time.frameCount == (int)__instance.GetInstanceField<object>("lastUpdateFrame")) {
                return;
            }

            if (DesignatorShapes.CurrentTool == null) { return; }

            __instance.SetInstanceField("lastUpdateFrame", Time.frameCount);
            __instance.DragCells.Clear();
            __instance.SetInstanceField<string>("failureReasonInt", null);

            var start = (IntVec3)__instance.GetInstanceField<object>("startDragCell");
            var sizeOrEnd = DesignatorShapes.CurrentTool.useSizeInputs ? ShapeControls.InputVec : UI.MouseCell();

            var points = DesignatorShapes.CurrentTool?.DrawMethod(start, sizeOrEnd);

            foreach (var vec in points) {
                if (vec == null) { continue; }
                __instance.InvokeMethod("TryAddDragCell", vec);
            }
        }
    }
}
