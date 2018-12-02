using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes.Patches {
    [HarmonyPatch(typeof(DesignationDragger), "UpdateDragCellsIfNeeded")]
    public static class DesignationDragger_UpdateDragCellsIfNeeded {
        public static void Prefix(DesignationDragger __instance) {
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

            var points = DesignatorShapes.CurrentTool?.DrawMethod(start, sizeOrEnd, DesignatorShapes.Rotation);

            foreach (var vec in points) {
                if (vec == null) { continue; }
                __instance.InvokeMethod("TryAddDragCell", vec);
            }
        }
    }
}
