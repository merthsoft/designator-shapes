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
    public static class DesignationDragger_UpdateDragCellsIfNeeded_Patch {
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
            var end = UI.MouseCell();

            var points = DesignatorShapes.CurrentTool?.drawMethod(start, end);
            points = points?.Rotate(start, end, DesignatorShapes.Rotation);

            foreach (var vec in points) {
                if (vec == null) { continue; }
                __instance.InvokeMethod("TryAddDragCell", vec);
            }
        }
    }
}
