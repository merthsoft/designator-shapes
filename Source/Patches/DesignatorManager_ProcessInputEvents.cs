using Harmony;
using RimWorld;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes.Patches {
    [HarmonyPatch(typeof(DesignatorManager), "ProcessInputEvents")]
    static class DesignatorManager_ProcessInputEvents {
        public static void Prefix(DesignatorManager __instance) {
            if (Event.current.type == EventType.KeyDown && Event.current.alt && DesignatorShapes.Settings.RestoreAltToggle) {
                DesignatorShapes.ShowControls = !DesignatorShapes.ShowControls;
            }

            if (!DesignatorShapes.ShowControls) { return; }

            if (__instance.SelectedDesignator == null) { return; }

            if (!__instance.SelectedDesignator.CanRemainSelected()) {
                __instance.Deselect();
                return;
            }

            if (__instance.SelectedDesignator.DraggableDimensions == 0) { return; }

            if (Event.current.type == EventType.KeyDown) {
                var key = Event.current.keyCode;

                if (key == KeyBindingDefOf.Designator_RotateLeft.MainKey) {
                    rotateShape(Event.current, 1);
                } else if (key == KeyBindingDefOf.Designator_RotateRight.MainKey) {
                    rotateShape(Event.current, -1);
                } else if (key == KeyBindingDefOf.Command_ItemForbid.MainKey) {
                    DesignatorShapes.FillCorners = !DesignatorShapes.FillCorners;
                } else if (key == KeyCode.Equals) {
                    DesignatorShapes.IncreaseThickness();
                } else if (key == KeyCode.Minus) {
                    DesignatorShapes.DecreaseThickness();
                }
            }

        }

        public static void Postfix(DesignatorManager __instance) {
            if (!DesignatorShapes.ShowControls) { return; }

            if (__instance.SelectedDesignator == null
                || DesignatorShapes.CurrentTool == null
                || __instance.Dragger == null) {
                return;
            }

            if (!DesignatorShapes.CurrentTool.AllowDragging && __instance.Dragger.Dragging) {
                __instance.Dragger.SetInstanceField("startDragCell", UI.MouseCell());
            }
        }

        private static void rotateShape(Event ev, int amount) {
            if (DesignatorShapes.Settings.DisableRotation) { return; }

            if (DesignatorShapes.Rotate(amount)) {
                ev.Use();
            }
        }
    }
}
