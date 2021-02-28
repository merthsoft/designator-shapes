using HarmonyLib;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes.Patches
{
    [HarmonyPatch(typeof(DesignatorManager), "ProcessInputEvents")]
    internal static class DesignatorManager_ProcessInputEvents
    {
        public static void Prefix(DesignatorManager __instance)
        {
            if (!DesignatorShapes.Settings.EnableKeyboardInput)
                return;

            if (Event.current.type == EventType.KeyDown && Event.current.alt && DesignatorShapes.Settings.RestoreAltToggle)
                DesignatorShapes.ShowControls = !DesignatorShapes.ShowControls;

            if (!DesignatorShapes.ShowControls)
                return;
            if (__instance.SelectedDesignator == null)
                return;
            if (!__instance.SelectedDesignator.CanRemainSelected())
            {
                __instance.Deselect();
                return;
            }

            if (__instance.SelectedDesignator.DraggableDimensions == 0)
                return;
            if (Event.current.type == EventType.KeyDown)
            {
                var key = Event.current.keyCode;

                if (key == DesignatorShapes.Settings.Keys[DesignatorSettings.RotateLeftKeyIndex])
                    RotateShape(Event.current, 1);
                else if (key == DesignatorShapes.Settings.Keys[DesignatorSettings.RotateRightKeyIndex])
                    RotateShape(Event.current, -1);
                else if (key == DesignatorShapes.Settings.Keys[DesignatorSettings.FillCornersToggleKeyIndex])
                    DesignatorShapes.FillCorners = !DesignatorShapes.FillCorners;
                else if (key == DesignatorShapes.Settings.Keys[DesignatorSettings.IncreaseThicknessKeyIndex])
                    DesignatorShapes.IncreaseThickness();
                else if (key == DesignatorShapes.Settings.Keys[DesignatorSettings.DecreaseThicknessKeyIndex])
                    DesignatorShapes.DecreaseThickness();
            }
        }

        public static void Postfix(DesignatorManager __instance)
        {
            if (!DesignatorShapes.ShowControls)
                return;
            if (__instance.SelectedDesignator == null
                || DesignatorShapes.CurrentTool == null
                || __instance.Dragger == null)
                return;

            if (!DesignatorShapes.CurrentTool.AllowDragging && __instance.Dragger.Dragging)
                __instance.Dragger.SetInstanceField("startDragCell", UI.MouseCell());
        }

        private static void RotateShape(Event ev, int amount)
        {
            if (DesignatorShapes.Rotate(amount))
                ev.Use();
        }
    }
}
