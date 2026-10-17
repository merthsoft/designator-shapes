#nullable enable
using System.Reflection;
using Merthsoft.DesignatorShapes;
using Verse;
using UnityEngine;

var asm = typeof(DesignatorSettings).Assembly;
var flags = BindingFlags.Static | BindingFlags.NonPublic;
var matcher = asm.GetType("Merthsoft.DesignatorShapes.Ui.ShortcutConflicts")!
    .GetMethods(flags).Single(m => m.Name == "HasNativeConflict" && m.GetParameters().Length == 3);
int checks = 0;
void Check(bool ok, string message) { checks++; if (!ok) throw new Exception(message); }
bool Conflict(Event e, KeyBindingDef[] defs, KeyPrefsData? prefs = null)
    => (bool)matcher.Invoke(null, new object?[] { e, defs, prefs })!;
Event Key(KeyCode key, EventModifiers mods = 0) => new() { type = EventType.KeyDown, keyCode = key, modifiers = mods, alt = (mods & EventModifiers.Alt) != 0, shift = (mods & EventModifiers.Shift) != 0 };
var def = new KeyBindingDef { defName = "Test", defaultKeyCodeA = KeyCode.A, defaultKeyCodeB = KeyCode.B };
var defs = new[] { def };
Check(!Conflict(Key(KeyCode.F12), defs), "Missing preferences must not reject unrelated keys");
Check(Conflict(Key(KeyCode.A), defs), "Default primary slot");
Check(Conflict(Key(KeyCode.B), defs), "Default secondary slot");
Check(!Conflict(Key(KeyCode.None), defs), "Clearing never conflicts");
var prefs = new KeyPrefsData { keyPrefs = new() { [def] = new(KeyCode.C, KeyCode.D) } };
Check(!Conflict(Key(KeyCode.A), defs, prefs), "Remapping replaces defaults");
Check(Conflict(Key(KeyCode.C), defs, prefs), "Saved primary slot");
Check(Conflict(Key(KeyCode.D), defs, prefs), "Saved secondary slot");
foreach (var mods in new[] { EventModifiers.Control, EventModifiers.Alt, EventModifiers.Shift, EventModifiers.Control | EventModifiers.Alt })
    Check(!Conflict(Key(KeyCode.C, mods), defs, prefs), "Explicit chord coexists with native plain key: " + mods);
Check(!Conflict(Key(KeyCode.C, EventModifiers.Command), defs, prefs), "Native Command suppression");
prefs.keyPrefs[def] = new(KeyCode.LeftCommand, KeyCode.C);
Check(Conflict(Key(KeyCode.C, EventModifiers.Command), defs, prefs), "Native Command-key exception");
prefs.keyPrefs[def] = new(KeyCode.None, KeyCode.None);
Check(!Conflict(Key(KeyCode.A), defs, prefs), "Explicit clearing must not restore default");
prefs.keyPrefs.Clear();
Check(Conflict(Key(KeyCode.B), defs, prefs), "Missing entry uses defaults");

foreach (KeyCode key in Enum.GetValues<KeyCode>().Distinct()) {
    Check(Conflict(Key(key), defs, prefs)
        == (key != KeyCode.None && (key == KeyCode.A || key == KeyCode.B)), "Plain key matches only effective slots: " + key);
    Check(!Conflict(Key(key, EventModifiers.Control | EventModifiers.Alt), defs, prefs), "Ctrl+Alt chord remains assignable: " + key);
}
foreach (var pair in new[] { (KeyCode.LeftAlt, EventModifiers.Alt), (KeyCode.RightAlt, EventModifiers.Alt),
    (KeyCode.LeftControl, EventModifiers.Control), (KeyCode.RightControl, EventModifiers.Control),
    (KeyCode.LeftShift, EventModifiers.Shift), (KeyCode.RightShift, EventModifiers.Shift) }) {
    prefs.keyPrefs[def] = new(pair.Item1, KeyCode.None);
    Check(Conflict(Key(pair.Item1, pair.Item2), defs, prefs), "Standalone modifier retains native conflict: " + pair.Item1);
}
Console.WriteLine($"PASS {checks} shortcut checks using production DLL and actual game binding/event types; no live game or config changes");
