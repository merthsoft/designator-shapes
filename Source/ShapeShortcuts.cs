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
    private static bool toggleWasHeld;
    private static int lastShapeShortcutFrame = -1;
    private static KeyCode lastShapeShortcutKey;
    internal static bool InputBlocked => GUIUtility.keyboardControl != 0 || Find.WindowStack.AnySearchWidgetFocused
        || Find.WindowStack.Windows.Any(w => w.absorbInputAroundWindow && w is not MainTabWindow);
    internal static bool Handle(DesignatorManager manager)
    {
        var e = Event.current; var settings = DesignatorShapes.Settings;
        // Modifier-only presses can arrive with keyCode=None in Waylith.
        // Track the held state once across root/window GUI passes instead.
        bool altHeld = e != null && (e.alt || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt));
        bool control = e != null && (e.control || Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl));
        bool shift = e != null && (e.shift || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        bool modifierBinding = Ui.ShortcutCaptureInput.IsModifier(settings.InterfaceToggleKey);
        bool toggleHeld = modifierBinding && (Input.GetKey(settings.InterfaceToggleKey)
            || (e?.type == EventType.KeyDown && e.keyCode == settings.InterfaceToggleKey));
        bool togglePressed = toggleHeld && !toggleWasHeld;
        toggleWasHeld = toggleHeld;
        if (e == null || InputBlocked || Find.CurrentMap == null) return true;
        if (settings.EnableKeyboardInput && e.type == EventType.KeyDown && control && !altHeld && !shift)
        {
            if (e.keyCode == settings.Keys[KeySettings.UndoKeyIndex])
            { HistoryManager.Undo(); e.Use(); return false; }
            if (e.keyCode == settings.Keys[KeySettings.RedoKeyIndex])
            { HistoryManager.Redo(); e.Use(); return false; }
        }
        if (manager.SelectedDesignator == null) return true;
        if (settings.CancelStrokeKeepsTool && (manager.Dragger.Dragging || FloodFillConfirmation.HasPendingPress)
            && ((e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape) || (e.type == EventType.MouseDown && e.button == 1)))
        { CancelStroke(manager); e.Use(); return false; }
        if (!settings.EnableKeyboardInput) return true;
        bool altBinding = settings.InterfaceToggleKey is KeyCode.LeftAlt or KeyCode.RightAlt;
        bool controlBinding = settings.InterfaceToggleKey is KeyCode.LeftControl or KeyCode.RightControl;
        bool shiftBinding = settings.InterfaceToggleKey is KeyCode.LeftShift or KeyCode.RightShift;
        bool keyMatches = modifierBinding ? togglePressed
            : e.type == EventType.KeyDown && e.keyCode == settings.InterfaceToggleKey;
        if (settings.ToggleableInterface && settings.RestoreAltToggle && keyMatches && (controlBinding || control == settings.InterfaceToggleControl)
            && (altBinding || altHeld == settings.InterfaceToggleAlt) && (shiftBinding || shift == settings.InterfaceToggleShift))
        {
            if (manager.Dragger.Dragging) CancelStroke(manager);
            DesignatorShapes.ShowControls = !DesignatorShapes.ShowControls;
            if (e.isKey || e.isMouse) e.Use();
            return false;
        }
        if (DesignatorShapes.ShowControls && !control && !altHeld && !shift && e.type == EventType.KeyDown
            && settings.Keys.Count > KeySettings.PreviousShapeKeyIndex
            && settings.Keys[KeySettings.PreviousShapeKeyIndex] != KeyCode.None
            && e.keyCode == settings.Keys[KeySettings.PreviousShapeKeyIndex]
            && ShapeEligibility.IsDrawable(DesignatorShapes.PreviousTool))
        {
            if (manager.Dragger.Dragging) CancelStroke(manager);
            DesignatorShapes.SelectTool(DesignatorShapes.PreviousTool);
            e.Use(); return false;
        }
        if (!DesignatorShapes.ShowControls || !control || !altHeld || shift) return true;
        var binding = settings.ShapeShortcuts.FirstOrDefault(p => p.Value != KeyCode.None
            && ((e.type == EventType.KeyDown && p.Value == e.keyCode)
                || (e.type == EventType.Repaint && Input.GetKeyDown(p.Value))));
        if (binding.Key == null) return true;
        if (lastShapeShortcutFrame == Time.frameCount && lastShapeShortcutKey == binding.Value) return true;
        var shape = DefDatabase<DesignatorShapeDef>.GetNamedSilentFail(binding.Key);
        if (!ShapeEligibility.IsDrawable(shape)) return true;
        lastShapeShortcutFrame = Time.frameCount; lastShapeShortcutKey = binding.Value;
        if (manager.Dragger.Dragging) CancelStroke(manager);
        DesignatorShapes.SelectTool(shape);
        if (e.isKey || e.isMouse) e.Use();
        return false;
    }
    internal static void CancelStroke(DesignatorManager manager)
    {
        FloodFillConfirmation.CancelPendingPress();
        manager.Dragger.EndDrag(); manager.Dragger.DragCells.Clear();
        FreeformLine.FreeMemory(); HistoryManager.FinishBuilding();
    }
}
