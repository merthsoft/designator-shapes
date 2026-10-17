using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using static Merthsoft.DesignatorShapes.KeySettings;

namespace Merthsoft.DesignatorShapes.Patches;

[HarmonyPatch(typeof(WindowStack), "HandleEventsHighPriority")]
internal static class ShapeWindowKeyboardInput
{
    static void Prefix()
    {
        bool visible = Find.CurrentMap != null && Find.DesignatorManager.SelectedDesignator != null
            && !WorldRendererUtility.WorldSelected
            && (DesignatorShapes.ShowControls || !DesignatorShapes.Settings.HideCollapsedInterface)
            && (!DesignatorShapes.Settings.HideWhenNoOpenTab || Find.MainTabsRoot.OpenTab != null);
        DesignatorShapes.ShapeControls?.HandleDragInput(visible);
        if (visible)
        {
            DesignatorShapes.ShapeControls?.HandleKeyboardInput();
        }
        if (Find.CurrentMap != null && !WorldRendererUtility.WorldSelected)
            ShapeShortcuts.Handle(Find.DesignatorManager);
    }
}

[HarmonyPatch(typeof(MapInterface), "MapInterfaceOnGUI_AfterMainTabs")]
public static class MapInterface_MapInterfaceOnGUI_AfterMainTabs
{
    public static void Postfix(MapInterface __instance)
    {
        if (Find.CurrentMap == null
            || Find.DesignatorManager.SelectedDesignator == null
            || WorldRendererUtility.WorldSelected)
            return;

        if (DesignatorShapes.ShowControls || !DesignatorShapes.Settings.HideCollapsedInterface)
            DesignatorShapes.ShapeControls?.ShapeControlsOnGUI();

        if (!DesignatorShapes.Settings.EnableKeyboardInput || ShapeShortcuts.InputBlocked)
            return;

        var current = Event.current;

        if (!DesignatorShapes.UseCustomGeometry)
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

            if (!(current.control || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
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
