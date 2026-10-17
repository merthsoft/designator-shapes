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
var inputType = asm.GetType("Merthsoft.DesignatorShapes.Ui.ShortcutCaptureInput")!;
var read = inputType.GetMethod("Read", BindingFlags.Instance | BindingFlags.NonPublic)!;
object Input() => Activator.CreateInstance(inputType, true)!;
string Read(object input, Event e, bool toggle = true) => read.Invoke(input, new object[] {e,toggle})!.ToString()!;
object Field(object input, string field) => inputType.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(input)!;
void Binding(object input, KeyCode key, bool ctrl = false, bool alt = false, bool shift = false) {
    Check((KeyCode)Field(input,"Key") == key && (bool)Field(input,"Control") == ctrl
        && (bool)Field(input,"Alt") == alt && (bool)Field(input,"Shift") == shift, "Captured binding " + key);
}
Event Release(KeyCode key) { var e=Key(key); e.type=EventType.KeyUp; return e; }
foreach (var pair in new[] { (KeyCode.LeftAlt, EventModifiers.Alt), (KeyCode.RightAlt, EventModifiers.Alt),
    (KeyCode.LeftControl, EventModifiers.Control), (KeyCode.RightControl, EventModifiers.Control),
    (KeyCode.LeftShift, EventModifiers.Shift), (KeyCode.RightShift, EventModifiers.Shift) }) {
    var input=Input();
    Check(Read(input,Key(pair.Item1,pair.Item2))=="Wait", "Modifier waits for release " + pair.Item1);
    Check(Read(input,Release(pair.Item1))=="Apply", "Modifier release applies " + pair.Item1);
    Binding(input,pair.Item1);
    input=Input();
    Check(Read(input,Key(pair.Item1,pair.Item2),false)=="Wait" && Read(input,Release(pair.Item1),false)=="Wait", "Shape never captures modifier-only " + pair.Item1);
}
foreach(var pair in new[]{(EventModifiers.Alt,KeyCode.LeftAlt),(EventModifiers.Control,KeyCode.LeftControl),(EventModifiers.Shift,KeyCode.LeftShift)}) {
    var input=Input();
    Check(Read(input,Key(KeyCode.None,pair.Item1))=="Wait", "Keyless modifier waits");
    Check(Read(input,Release(KeyCode.None))=="Apply", "Keyless modifier release applies"); Binding(input,pair.Item2);
}
var keylessAlt=Input(); Read(keylessAlt,Key(KeyCode.None,EventModifiers.Alt));
Check(Read(keylessAlt,Release(KeyCode.RightAlt))=="Apply", "Keyless modifier down accepts sided release"); Binding(keylessAlt,KeyCode.LeftAlt);
var capture=Input();
Read(capture,Key(KeyCode.LeftControl,EventModifiers.Control));
Read(capture,Key(KeyCode.LeftAlt,EventModifiers.Control|EventModifiers.Alt));
Check(Read(capture,Key(KeyCode.A,EventModifiers.Control|EventModifiers.Alt))=="Apply", "Toggle chord not captured prematurely");
Binding(capture,KeyCode.A,true,true);
capture=Input(); Read(capture,Key(KeyCode.LeftAlt,EventModifiers.Alt));
Check(Read(capture,Key(KeyCode.Escape))=="Cancel", "Escape cancels pending modifier");
foreach(var toggle in new[]{false,true}) {
    capture=Input();
    Check(Read(capture,Key(KeyCode.Backspace,EventModifiers.Control|EventModifiers.Alt),toggle)=="Apply", "Backspace clears either mode");
    Binding(capture,KeyCode.None);
    Check(Read(Input(),Key(KeyCode.Escape),toggle)=="Cancel", "Escape cancels either mode");
}
capture=Input();
Check(Read(capture,Key(KeyCode.A),false)=="Wait", "Shape requires Ctrl+Alt");
Check(Read(capture,Key(KeyCode.A,EventModifiers.Control|EventModifiers.Alt|EventModifiers.Shift),false)=="Wait", "Shape excludes Shift");
Check(Read(capture,Key(KeyCode.A,EventModifiers.Control|EventModifiers.Alt),false)=="Apply", "Shape chord applies"); Binding(capture,KeyCode.A);
capture=Input();
Check(Read(capture,Key(KeyCode.F12))=="Apply", "Plain toggle key applies"); Binding(capture,KeyCode.F12);
var prompt=asm.GetType("Merthsoft.DesignatorShapes.Ui.ShortcutCapture")!.GetMethod("PromptKey",flags)!;
var togglePrompt=(string)prompt.Invoke(null,new object[]{true})!;
var shapePrompt=(string)prompt.Invoke(null,new object[]{false})!;
var dir=new DirectoryInfo(AppContext.BaseDirectory); while(dir!=null && !Directory.Exists(Path.Combine(dir.FullName,"Languages"))) dir=dir.Parent;
var translations=System.Xml.Linq.XDocument.Load(Path.Combine(dir!.FullName,"Languages/English/Keyed/DesignatorShapes_Keys.xml")).Root!;
Check(!translations.Element(togglePrompt)!.Value.Contains("Ctrl+Alt") && translations.Element(togglePrompt)!.Value.Contains("press and release"), "Toggle prompt describes modifier tap without shape chord requirement");
Check(translations.Element(shapePrompt)!.Value.Contains("Ctrl+Alt"), "Shape prompt retains required chord");
Console.WriteLine($"PASS {checks} shortcut checks using production DLL and actual game binding/event types; no live game or config changes");
