using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using static Merthsoft.DesignatorShapes.KeySettings;

namespace Merthsoft.DesignatorShapes.Patches;

[HarmonyPatch(typeof(MapInterface), "MapInterfaceOnGUI_AfterMainTabs")]
public static class MapInterface_MapInterfaceOnGUI_AfterMainTabs
{
    public static void Postfix(MapInterface __instance)
    {
        if (Find.CurrentMap == null
            || Find.DesignatorManager.SelectedDesignator == null
            || WorldRendererUtility.WorldSelected)
            return;

        if (DesignatorShapes.ShowControls)
            DesignatorShapes.ShapeControls?.ShapeControlsOnGUI();
        else if (DesignatorShapes.Settings.RestoreAltToggle && !DesignatorShapes.Settings.HideCompletelyOnAltToggle)
            DesignatorShapes.ShapeControls?.ShapeControlsOnGUI();

        if (!DesignatorShapes.Settings.EnableKeyboardInput)
            return;

        var current = Event.current;

        if (current.type == EventType.KeyDown && current.alt && DesignatorShapes.Settings.RestoreAltToggle)
        {
            DesignatorShapes.ShowControls = !DesignatorShapes.ShowControls;
            Event.current.Use();
            return;
        }

        if (!DesignatorShapes.ShowControls)
            return;

        var designatorManager = __instance.designatorManager;
        if (designatorManager.SelectedDesignator == null)
            return;

        if (!designatorManager.SelectedDesignator.CanRemainSelected())
        {
            designatorManager.Deselect();
            return;
        }

        if (current.type == EventType.KeyDown)
        {
            var key = current.keyCode;
            var keyMap = DesignatorShapes.Settings.Keys;

            if (current.control)
            {
                if (key == keyMap[UndoKeyIndex])
                {
                    HistoryManager.Undo();
                    current.Use();
                }
                else if (key == keyMap[RedoKeyIndex])
                {
                    HistoryManager.Redo();
                    current.Use();
                }
            }
            else
            {
                if (key == keyMap[RotateLeftKeyIndex] && DesignatorShapes.Settings.EnableRotationKeys)
                {
                    if (DesignatorShapes.RotateLeft())
                        current.Use();
                }
                else if (key == keyMap[RotateRightKeyIndex] && DesignatorShapes.Settings.EnableRotationKeys)
                {
                    if (DesignatorShapes.RotateRight())
                        current.Use();
                }
                else if (key == keyMap[FillCornersToggleKeyIndex])
                {
                    DesignatorShapes.FillCorners = !DesignatorShapes.FillCorners;
                    current.Use();
                }
                else if (key == keyMap[IncreaseThicknessKeyIndex])
                {
                    DesignatorShapes.IncreaseThickness();
                    current.Use();
                }
                else if (key == keyMap[DecreaseThicknessKeyIndex])
                {
                    DesignatorShapes.DecreaseThickness();
                    current.Use();
                }
            }
        }
    }
}
