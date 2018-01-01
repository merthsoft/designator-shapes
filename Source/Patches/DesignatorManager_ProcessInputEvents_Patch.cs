using Harmony;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes.Patches {
    [HarmonyPatch(typeof(DesignatorManager), "ProcessInputEvents")]
    static class DesignatorManager_ProcessInputEvents_Patch {
        public static void Prefix(DesignatorManager __instance) {
            if (__instance.SelectedDesignator == null) { return; }

            if (!__instance.SelectedDesignator.CanRemainSelected()) {
                __instance.Deselect();
                return;
            }

            if (__instance.SelectedDesignator.DraggableDimensions == 0) { return; }

            var place = __instance.SelectedDesignator as Designator_Place;
            var placeDef = place?.PlacingDef as ThingDef;
            var rotatable = placeDef?.rotatable;
            if (rotatable.HasValue && rotatable.Value) {
                return;
            }

            if (Event.current.type == EventType.keyDown) {
                switch (Event.current.keyCode) {
                    case KeyCode.Q:
                        rotateShapePositive(Event.current);
                        break;
                    case KeyCode.E:
                        rotateShapeNegative(Event.current);
                        break;
                }
            }
        }

        public static void Postfix(DesignatorManager __instance) {
            if (__instance.SelectedDesignator == null) { return; }

            if (DesignatorShapes.CurrentTool == null) { return; }

            if (__instance.Dragger == null) { return; }

            if (!DesignatorShapes.CurrentTool.draggable && __instance.Dragger.Dragging) {
                __instance.Dragger.SetInstanceField("startDragCell", UI.MouseCell());
            }
        }

        private static void rotateShapePositive(Event ev) {
            DesignatorShapes.Rotate(1);
            ev.Use();
        }

        private static void rotateShapeNegative(Event ev) {
            DesignatorShapes.Rotate(-1);
            ev.Use();
        }
    }
}
