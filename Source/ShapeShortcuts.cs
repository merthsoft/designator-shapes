using System;
using System.Collections.Generic;
using System.Linq;
using Merthsoft.DesignatorShapes.Defs;
using Merthsoft.DesignatorShapes.Shapes;
using RimWorld;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes;

internal static class ShapeMemory
{
    internal static string KeyFor(Designator tool) => RememberedShapeStore.Key(tool?.GetType(), tool is Designator_Build build ? build.PlacingDef?.defName : null);
    internal static Dictionary<string, string> Entries => Current.Game?.GetComponent<HistoryComponent>()?.RememberedShapes;
    internal static DesignatorShapeDef Recall(Designator tool)
    {
        var key = KeyFor(tool); var entries = Entries;
        var recalled = DesignatorShapes.Settings.RememberShapePerDesignator && key != null && entries != null && entries.TryGetValue(key, out var shape)
            ? DefDatabase<DesignatorShapeDef>.GetNamedSilentFail(shape) : null;
        return ShapeEligibility.IsDrawable(recalled) ? recalled : null;
    }
    internal static void Remember(Designator tool, DesignatorShapeDef shape)
    {
        if (!DesignatorShapes.Settings.RememberShapePerDesignator || !ShapeEligibility.IsDrawable(shape)) return;
        var key = KeyFor(tool); var entries = Entries;
        RememberedShapeStore.Put(entries, key, shape.defName);
    }
}

internal static class ShapeShortcuts
{
    internal static bool InputBlocked => GUIUtility.keyboardControl != 0 || Find.WindowStack.AnySearchWidgetFocused
        || Find.WindowStack.Windows.Any(w => w.absorbInputAroundWindow && w is not MainTabWindow);
    internal static bool Handle(DesignatorManager manager)
    {
        var e = Event.current; var settings = DesignatorShapes.Settings;
        if (e == null || InputBlocked || manager.SelectedDesignator == null || Find.CurrentMap == null) return true;
        if (settings.CancelStrokeKeepsTool && (manager.Dragger.Dragging || FloodFillConfirmation.HasPendingPress)
            && ((e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape) || (e.type == EventType.MouseDown && e.button == 1)))
        { CancelStroke(manager); e.Use(); return false; }
        if (!settings.EnableKeyboardInput || e.type != EventType.KeyDown) return true;
        bool keyMatches = e.keyCode == settings.InterfaceToggleKey
            || (settings.InterfaceToggleKey == KeyCode.LeftAlt && e.keyCode == KeyCode.RightAlt);
        bool altKey = e.keyCode is KeyCode.LeftAlt or KeyCode.RightAlt;
        if (settings.ToggleableInterface && settings.RestoreAltToggle && keyMatches && e.control == settings.InterfaceToggleControl
            && (altKey || e.alt == settings.InterfaceToggleAlt) && e.shift == settings.InterfaceToggleShift)
        {
            if (manager.Dragger.Dragging) CancelStroke(manager);
            DesignatorShapes.ShowControls = !DesignatorShapes.ShowControls; e.Use(); return false;
        }
        if (!DesignatorShapes.ShowControls || !e.control || !e.alt || e.shift
            || Ui.ShortcutConflicts.HasNativeConflict(e)) return true;
        var binding = settings.ShapeShortcuts.FirstOrDefault(p => p.Value != KeyCode.None && p.Value == e.keyCode);
        if (binding.Key == null) return true;
        var shape = DefDatabase<DesignatorShapeDef>.GetNamedSilentFail(binding.Key);
        if (!ShapeEligibility.IsDrawable(shape)) return true;
        if (manager.Dragger.Dragging) CancelStroke(manager);
        DesignatorShapes.SelectTool(shape); e.Use(); return false;
    }
    internal static void CancelStroke(DesignatorManager manager)
    {
        FloodFillConfirmation.CancelPendingPress();
        manager.Dragger.EndDrag(); manager.Dragger.DragCells.Clear();
        FreeformLine.FreeMemory(); HistoryManager.FinishBuilding();
    }
}
