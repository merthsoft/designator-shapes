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

internal sealed class ShortcutCapture : Window
{
    private readonly Action<KeyCode, bool, bool, bool> apply;
    private readonly bool modifiers;
    private readonly string shapeDefName;
    public override Vector2 InitialSize => new(440f, 180f);
    internal ShortcutCapture(Action<KeyCode, bool, bool, bool> apply, bool modifiers, string shapeDefName = null) { this.apply = apply; this.modifiers = modifiers; this.shapeDefName = shapeDefName; absorbInputAroundWindow = true; }
    public override void DoWindowContents(Rect rect)
    {
        Widgets.Label(rect, "Merthsoft_DesignatorShapes_Settings_ShortcutCapture".Translate());
        var e = Event.current;
        if (e.type != EventType.KeyDown || e.keyCode == KeyCode.None) return;
        if (e.keyCode == KeyCode.Escape) { e.Use(); Close(); return; }
        if (!modifiers && e.keyCode is KeyCode.LeftAlt or KeyCode.RightAlt or KeyCode.LeftControl or KeyCode.RightControl or KeyCode.LeftShift or KeyCode.RightShift) return;
        var key = e.keyCode == KeyCode.Backspace ? KeyCode.None : e.keyCode;
        if (!modifiers && key != KeyCode.None && (!e.control || !e.alt || e.shift)) return;
        if (key != KeyCode.None && ShortcutConflicts.HasNativeConflict(e))
        { Messages.Message("Merthsoft_DesignatorShapes_Settings_ShortcutConflict".Translate(), MessageTypeDefOf.RejectInput, false); e.Use(); return; }
        bool ctrl = modifiers && e.control, alt = modifiers && e.alt && key is not (KeyCode.LeftAlt or KeyCode.RightAlt), shift = modifiers && e.shift;
        var settings = DesignatorShapes.Settings;
        bool collision = key != KeyCode.None && (modifiers
            ? ctrl && alt && !shift && settings.ShapeShortcuts.Values.Contains(key)
            : (settings.RestoreAltToggle && settings.InterfaceToggleKey == key && settings.InterfaceToggleControl && settings.InterfaceToggleAlt && !settings.InterfaceToggleShift) || settings.ShapeShortcuts.Any(p => p.Key != shapeDefName && p.Value == key));
        if (collision) { Messages.Message("Merthsoft_DesignatorShapes_Settings_ShortcutConflict".Translate(), MessageTypeDefOf.RejectInput, false); e.Use(); return; }
        apply(key, ctrl, alt, shift);
        DesignatorShapes.Settings.Write(); e.Use(); Close();
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
