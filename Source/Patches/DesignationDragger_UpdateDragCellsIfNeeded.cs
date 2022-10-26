using System;
using System.Collections.Generic;
using System.Linq;

using HarmonyLib;

using Merthsoft.DesignatorShapes.Utils;

using UnityEngine;

using Verse;
using Verse.Sound;

namespace Merthsoft.DesignatorShapes.Patches
{
    [HarmonyPatch(typeof(DesignationDragger), "UpdateDragCellsIfNeeded")]
    public static class DesignationDragger_UpdateDragCellsIfNeeded
    {
        public static bool Prefix(DesignationDragger __instance)
        {
            if (!DesignatorShapes.ShowControls)
                return false;
            if (__instance == null)
                return false;
            if (Time.frameCount == (int)__instance.GetInstanceField<object>("lastUpdateFrame"))
                return false;

            if (DesignatorShapes.CurrentTool == null)
                return false;
            __instance.SetInstanceField("lastUpdateFrame", Time.frameCount);
            __instance.DragCells.Clear();
            __instance.SetInstanceField<string>("failureReasonInt", null);

            var start = (IntVec3)__instance.GetInstanceField<object>("startDragCell");
            var sizeOrEnd = DesignatorShapes.CurrentTool.useSizeInputs ? ShapeControls.InputVec : UI.MouseCell();

            var points = DesignatorShapes.CurrentTool?.DrawMethod(start, sizeOrEnd);

            foreach (var vec in points)
            {
                if (vec == null)
                    continue;
                __instance.InvokeMethod("TryAddDragCell", new IntVec3(vec.x, start.y, vec.z));
            }

            return false;
        }

        private static string FormatPoint(IntVec3 point)
        {
            return $"{point.x}:{point.y}:{point.z}";
        }
    }

    [HarmonyPatch(typeof(DesignationDragger), "DraggerUpdate")]
    public static class DesignationDragger_DraggerUpdate
    {
        public static bool Prefix(DesignationDragger __instance)
        {
            var dragging = (bool)__instance.GetInstanceField<object>("dragging");

            if (!dragging)
                return false;

            var SelDes = (Designator)__instance.GetInstanceProperty<object>("SelDes");
            var lastFrameDragCellsDrawn = (int)__instance.GetInstanceField<object>("lastFrameDragCellsDrawn");
            var sustainer = (Sustainer)__instance.GetInstanceField<object>("sustainer");
            var lastDragRealTime = (float)__instance.GetInstanceField<object>("lastDragRealTime");

            List<IntVec3> dragCells = __instance.DragCells;
            SelDes.RenderHighlight(dragCells);
            if (dragCells.Count != lastFrameDragCellsDrawn)
            {
                lastDragRealTime = Time.realtimeSinceStartup;

                __instance.SetInstanceField<object>("lastDragRealTime", lastDragRealTime);
                __instance.SetInstanceField<object>("lastFrameDragCellsDrawn", dragCells.Count);

                if (SelDes.soundDragChanged != null)
                    SelDes.soundDragChanged.PlayOneShotOnCamera();
            }
            if (sustainer == null || sustainer.Ended)
            {
                if (SelDes.soundDragSustain == null)
                    return false;
                
                __instance.SetInstanceField<object>("sustainer",
                    SelDes.soundDragSustain.TrySpawnSustainer(SoundInfo.OnCamera(MaintenanceType.PerFrame)));
            }
            else
            {
                sustainer.externalParams["TimeSinceDrag"] = Time.realtimeSinceStartup - lastDragRealTime;
                sustainer.Maintain();
            }
            return false;
        }

    }
}
