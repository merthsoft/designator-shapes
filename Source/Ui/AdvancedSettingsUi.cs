using System;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Merthsoft.DesignatorShapes.Defs;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes.Ui;

internal static class ShortcutConflicts
{
    internal static bool HasNativeConflict(Event e)
        => HasNativeConflict(e, DefDatabase<KeyBindingDef>.AllDefsListForReading, KeyPrefs.KeyPrefsData);

    internal static bool HasNativeConflict(Event e, IEnumerable<KeyBindingDef> defs, KeyPrefsData prefs)
    {
        if (e.keyCode == KeyCode.None) return false;
        // Native bindings store a key, not a modifier chord. Our handler consumes
        // explicit chords before native commands run, so Ctrl+Alt+A can coexist
        // with native A. A modifier used as the key itself is still a plain binding.
        if ((e.control && e.keyCode is not (KeyCode.LeftControl or KeyCode.RightControl))
            || (e.alt && e.keyCode is not (KeyCode.LeftAlt or KeyCode.RightAlt))
            || (e.shift && e.keyCode is not (KeyCode.LeftShift or KeyCode.RightShift))) return false;
        foreach (var def in defs)
        {
            // Compare stored bindings without depending on live window/focus state.
            var a = def.defaultKeyCodeA;
            var b = def.defaultKeyCodeB;
            if (prefs?.keyPrefs != null && prefs.keyPrefs.TryGetValue(def, out var binding))
            { a = binding.keyBindingA; b = binding.keyBindingB; }
            // Native bindings ignore Ctrl/Alt/Shift, but suppress Command chords
            // unless the command itself is bound to a Command key.
            if (e.command && a is not (KeyCode.LeftCommand or KeyCode.RightCommand)
                && b is not (KeyCode.LeftCommand or KeyCode.RightCommand)) continue;
            if (e.keyCode == a || e.keyCode == b) return true;
        }
        return false;
    }
}

internal enum ShortcutCaptureAction { Wait, Apply, Cancel }

internal sealed class ShortcutCaptureInput
{
    private KeyCode pendingModifier;
    private int heldModifierMask;
    internal KeyCode Key;
    internal bool Control, Alt, Shift;
    internal static bool IsModifier(KeyCode key) => key is KeyCode.LeftAlt or KeyCode.RightAlt
        or KeyCode.LeftControl or KeyCode.RightControl or KeyCode.LeftShift or KeyCode.RightShift;
    private static KeyCode ModifierFamily(KeyCode key) => key switch
    {
        KeyCode.RightAlt => KeyCode.LeftAlt,
        KeyCode.RightControl => KeyCode.LeftControl,
        KeyCode.RightShift => KeyCode.LeftShift,
        _ => key
    };

    internal ShortcutCaptureAction ReadHeldModifiers(bool control, bool alt, bool shift, bool toggle)
        => ReadHeldModifierKeys(control, false, alt, false, shift, false, toggle);

    internal ShortcutCaptureAction ReadHeldModifierKeys(bool leftControl, bool rightControl, bool leftAlt, bool rightAlt,
        bool leftShift, bool rightShift, bool toggle)
    {
        bool control = leftControl || rightControl, alt = leftAlt || rightAlt, shift = leftShift || rightShift;
        int mask = (leftControl ? 1 : 0) | (rightControl ? 2 : 0) | (leftAlt ? 4 : 0)
            | (rightAlt ? 8 : 0) | (leftShift ? 16 : 0) | (rightShift ? 32 : 0);
        int pressed = mask & ~heldModifierMask;
        heldModifierMask = mask;
        if (!toggle) return ShortcutCaptureAction.Wait;
        if (pressed != 0)
        {
            Key = (pressed & 8) != 0 ? KeyCode.RightAlt : (pressed & 4) != 0 ? KeyCode.LeftAlt
                : (pressed & 32) != 0 ? KeyCode.RightShift : (pressed & 16) != 0 ? KeyCode.LeftShift
                : (pressed & 2) != 0 ? KeyCode.RightControl : KeyCode.LeftControl;
            Control = control && ModifierFamily(Key) != KeyCode.LeftControl;
            Alt = alt && ModifierFamily(Key) != KeyCode.LeftAlt;
            Shift = shift && ModifierFamily(Key) != KeyCode.LeftShift;
            pendingModifier = Key;
            return ShortcutCaptureAction.Wait;
        }
        bool pendingHeld = ModifierFamily(pendingModifier) switch
        {
            KeyCode.LeftControl => control,
            KeyCode.LeftAlt => alt,
            KeyCode.LeftShift => shift,
            _ => true
        };
        if (!pendingHeld) { pendingModifier = KeyCode.None; return ShortcutCaptureAction.Apply; }
        return ShortcutCaptureAction.Wait;
    }

    internal static Event NormalizeEvent(Event e, bool control, bool alt, bool shift)
        => new Event { type = e.type, keyCode = e.keyCode,
            modifiers = (control ? EventModifiers.Control : 0) | (alt ? EventModifiers.Alt : 0)
                | (shift ? EventModifiers.Shift : 0), alt = alt, shift = shift };

    internal ShortcutCaptureAction Read(Event e, bool toggle)
    {
        var key = e.keyCode;
        if (e.type == EventType.KeyDown && key == KeyCode.Escape) return ShortcutCaptureAction.Cancel;
        if (e.type == EventType.KeyDown && key == KeyCode.Backspace)
        { Key = KeyCode.None; Control = Alt = Shift = false; return ShortcutCaptureAction.Apply; }
        // Some backends report modifier changes without a keyCode. With no side
        // information, use the left key (the canonical binding for either side).
        if (toggle && key == KeyCode.None && e.type == EventType.KeyDown)
        {
            if (e.alt && !e.control && !e.shift) key = KeyCode.LeftAlt;
            else if (e.control && !e.alt && !e.shift) key = KeyCode.LeftControl;
            else if (e.shift && !e.alt && !e.control) key = KeyCode.LeftShift;
        }
        if (toggle && e.type == EventType.KeyUp && pendingModifier != KeyCode.None
            && (ModifierFamily(key) == ModifierFamily(pendingModifier) || key == KeyCode.None))
        { pendingModifier = KeyCode.None; return ShortcutCaptureAction.Apply; }
        if (e.type != EventType.KeyDown || key == KeyCode.None) return ShortcutCaptureAction.Wait;
        if (!toggle && (IsModifier(key) || !e.control || !e.alt || e.shift)) return ShortcutCaptureAction.Wait;
        Key = key;
        Control = toggle && e.control && key is not (KeyCode.LeftControl or KeyCode.RightControl);
        Alt = toggle && e.alt && key is not (KeyCode.LeftAlt or KeyCode.RightAlt);
        Shift = toggle && e.shift && key is not (KeyCode.LeftShift or KeyCode.RightShift);
        if (toggle && IsModifier(key))
        { pendingModifier = key; return ShortcutCaptureAction.Wait; }
        pendingModifier = KeyCode.None;
        return ShortcutCaptureAction.Apply;
    }
}

internal sealed class ShortcutCapture : Window
{
    private static readonly KeyCode[] KeyboardKeys = Enum.GetValues<KeyCode>().Distinct()
        .Where(k => k != KeyCode.None && (int)k < (int)KeyCode.Mouse0 && !ShortcutCaptureInput.IsModifier(k)).ToArray();
    private readonly Action<KeyCode, bool, bool, bool> apply;
    private readonly bool modifiers;
    private readonly string shapeDefName;
    private readonly ShortcutCaptureInput input = new();
    internal static string PromptKey(bool toggle) => toggle
        ? "Merthsoft_DesignatorShapes_Settings_ToggleShortcutCapture"
        : "Merthsoft_DesignatorShapes_Settings_ShortcutCapture";
    public override Vector2 InitialSize => new(440f, 180f);
    internal ShortcutCapture(Action<KeyCode, bool, bool, bool> apply, bool modifiers, string shapeDefName = null) { this.apply = apply; this.modifiers = modifiers; this.shapeDefName = shapeDefName; absorbInputAroundWindow = true; }
    public override void DoWindowContents(Rect rect)
    {
        Widgets.Label(rect, PromptKey(modifiers).Translate());
        var e = Event.current;
        bool lc = Input.GetKey(KeyCode.LeftControl), rc = Input.GetKey(KeyCode.RightControl);
        bool la = Input.GetKey(KeyCode.LeftAlt), ra = Input.GetKey(KeyCode.RightAlt);
        bool ls = Input.GetKey(KeyCode.LeftShift), rs = Input.GetKey(KeyCode.RightShift);
        var captureEvent = ShortcutCaptureInput.NormalizeEvent(e, e.control || lc || rc, e.alt || la || ra, e.shift || ls || rs);
        var action = input.Read(captureEvent, modifiers);
        // A backend can omit the key event or its modifier flags. Poll actual keyboard
        // presses too; exclude mouse buttons so opening the dialog cannot bind its click.
        if (action == ShortcutCaptureAction.Wait && e.type == EventType.Repaint)
        {
            var pressed = KeyboardKeys.FirstOrDefault(Input.GetKeyDown);
            if (pressed != KeyCode.None)
            {
                captureEvent.type = EventType.KeyDown; captureEvent.keyCode = pressed;
                action = input.Read(captureEvent, modifiers);
            }
        }
        if (action == ShortcutCaptureAction.Wait)
            action = input.ReadHeldModifierKeys(lc, rc, la, ra, ls, rs, modifiers);
        if (action == ShortcutCaptureAction.Wait) return;
        if (action == ShortcutCaptureAction.Cancel) { if (e.isKey || e.isMouse) e.Use(); Close(); return; }
        var key = input.Key;
        // A modifier tap is distinct from native held-modifier controls.
        if (key != KeyCode.None && !(modifiers && ShortcutCaptureInput.IsModifier(key)) && ShortcutConflicts.HasNativeConflict(captureEvent))
        { Messages.Message("Merthsoft_DesignatorShapes_Settings_ShortcutConflict".Translate(), MessageTypeDefOf.RejectInput, false); if (e.isKey || e.isMouse) e.Use(); return; }
        bool ctrl = input.Control, alt = input.Alt, shift = input.Shift;
        var settings = DesignatorShapes.Settings;
        bool collision = key != KeyCode.None && (modifiers
            ? ctrl && alt && !shift && settings.ShapeShortcuts.Values.Contains(key)
            : (settings.RestoreAltToggle && settings.InterfaceToggleKey == key && settings.InterfaceToggleControl && settings.InterfaceToggleAlt && !settings.InterfaceToggleShift) || settings.ShapeShortcuts.Any(p => p.Key != shapeDefName && p.Value == key));
        if (collision) { Messages.Message("Merthsoft_DesignatorShapes_Settings_ShortcutConflict".Translate(), MessageTypeDefOf.RejectInput, false); if (e.isKey || e.isMouse) e.Use(); return; }
        apply(key, ctrl, alt, shift);
        DesignatorShapes.Settings.Write(); if (e.isKey || e.isMouse) e.Use(); Close();
    }
}

internal static class AdvancedSettingsUi
{
    private static string shapeSearch = "", barrierSearch = "";
    private static Vector2 shapeScroll, barrierScroll;
    internal static void Draw(Listing_Standard ls)
    {
        var settings = DesignatorShapes.Settings;
        ls.GapLine();
        ls.CheckboxLabeled("Merthsoft_DesignatorShapes_Settings_RememberPerDesignator".Translate(), ref settings.RememberShapePerDesignator,
            "Merthsoft_DesignatorShapes_Settings_RememberPerDesignatorTooltip".Translate());
        ls.CheckboxLabeled("Merthsoft_DesignatorShapes_Settings_CancelStroke".Translate(), ref settings.CancelStrokeKeepsTool);
        if (settings.EnableKeyboardInput && settings.ToggleableInterface)
        {
            ls.CheckboxLabeled("Merthsoft_DesignatorShapes_Settings_ToggleShortcut".Translate(), ref settings.RestoreAltToggle);
            if (settings.RestoreAltToggle && ls.ButtonText("Merthsoft_DesignatorShapes_Settings_ToggleBinding".Translate() + ": " +
                (settings.InterfaceToggleControl ? "Ctrl+" : "") + (settings.InterfaceToggleAlt ? "Alt+" : "") + (settings.InterfaceToggleShift ? "Shift+" : "") + settings.InterfaceToggleKey))
                Find.WindowStack.Add(new ShortcutCapture((key, ctrl, alt, shift) => { settings.InterfaceToggleKey = key; settings.InterfaceToggleControl = ctrl; settings.InterfaceToggleAlt = alt; settings.InterfaceToggleShift = shift; }, true));
        }
        ls.Label("Merthsoft_DesignatorShapes_Settings_ShapeShortcuts".Translate());
        shapeSearch = ls.TextEntry(shapeSearch);
        var shapes = DefDatabase<DesignatorShapeDef>.AllDefsListForReading.Where(d => ShapeEligibility.IsDrawable(d)
            && d.LabelCap.ToString().IndexOf(shapeSearch, StringComparison.OrdinalIgnoreCase) >= 0).OrderBy(d => d.label).ToList();
        var outer = ls.GetRect(180f); var view = new Rect(0, 0, outer.width - 20, shapes.Count * 30f);
        Widgets.BeginScrollView(outer, ref shapeScroll, view);
        for (int i = 0; i < shapes.Count; i++)
        {
            var shape = shapes[i]; settings.ShapeShortcuts.TryGetValue(shape.defName, out var key);
            if (Widgets.ButtonText(new Rect(0, i * 30f, view.width, 28f), shape.LabelCap + ": " + (key == KeyCode.None ? "-" : "Ctrl+Alt+" + key)))
                Find.WindowStack.Add(new ShortcutCapture((chosen, _, _, _) =>
                {
                    foreach (var duplicate in settings.ShapeShortcuts.Where(p => p.Value == chosen).Select(p => p.Key).ToList()) settings.ShapeShortcuts.Remove(duplicate);
                    if (chosen == KeyCode.None) settings.ShapeShortcuts.Remove(shape.defName); else settings.ShapeShortcuts[shape.defName] = chosen;
                }, false, shape.defName));
        }
        Widgets.EndScrollView();
        ls.GapLine();
        ls.CheckboxLabeled("Merthsoft_DesignatorShapes_Settings_ConfirmDestructive".Translate(), ref settings.ConfirmDestructiveFloodFill,
            "Merthsoft_DesignatorShapes_Settings_ConfirmDestructiveTooltip".Translate());
        ls.CheckboxLabeled("Merthsoft_DesignatorShapes_Settings_AdditionalBarriers".Translate(), ref settings.EnableCustomFloodFillBarriers,
            "Merthsoft_DesignatorShapes_Settings_AdditionalBarriersTooltip".Translate());
        if (!settings.EnableCustomFloodFillBarriers) return;
        ls.Label("Merthsoft_DesignatorShapes_Settings_SearchBuildings".Translate()); barrierSearch = ls.TextEntry(barrierSearch);
        var defs = DefDatabase<ThingDef>.AllDefsListForReading.Where(d => d.category == ThingCategory.Building && d.entityDefToBuild == null
            && (d.label ?? d.defName).IndexOf(barrierSearch, StringComparison.OrdinalIgnoreCase) >= 0).OrderBy(d => d.label ?? d.defName).ToList();
        outer = ls.GetRect(240f); view = new Rect(0, 0, outer.width - 20, defs.Count * 28f);
        Widgets.BeginScrollView(outer, ref barrierScroll, view);
        for (int i = 0; i < defs.Count; i++)
        {
            var def = defs[i]; bool selected = settings.AdditionalFloodFillBarrierDefs.Contains(def.defName), before = selected;
            Widgets.CheckboxLabeled(new Rect(0, i * 28f, view.width, 28f), (string.IsNullOrWhiteSpace(def.label) ? def.defName : def.LabelCap.ToString()) + " (" + def.defName + ")", ref selected);
            if (selected != before) { if (selected) settings.AdditionalFloodFillBarrierDefs.Add(def.defName); else settings.AdditionalFloodFillBarrierDefs.RemoveAll(id => id == def.defName); }
        }
        Widgets.EndScrollView();
    }
}
