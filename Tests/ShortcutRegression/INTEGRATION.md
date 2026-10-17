# Shortcut binding fix integration

Base: `4e14bb4` (v1.7.4). Branch: `shortcut-binding-fix`.

The capture dialog and shape dispatcher queried every `KeyBindingDef.KeyDownEvent`.
RimWorld's native binding data stores two plain keys and its event getter ignores
Ctrl/Alt/Shift. Consequently a shape chord such as Ctrl+Alt+A was rejected merely
because native A was bound, even for a command in a different context. The same
check also prevented a saved shape chord from executing.

`ShortcutConflicts` now distinguishes explicit modifier chords from native plain
keys. Plain assignments still compare both effective native binding slots; saved
preferences override defaults, including explicit unbinding. Standalone modifier
keys still count as plain bindings. This intentionally lets the custom shortcut
consume a modified chord before native designator input. Existing duplicate shape
and toggle checks, shape Ctrl+Alt/no-Shift requirement, Escape cancellation,
Backspace clearing, and capture application/persistence remain unchanged.

Integration touches only `Source/Ui/AdvancedSettingsUi.cs` and one predicate in
`Source/ShapeShortcuts.cs`. Cherry-pick the source/test commit onto workshop-reports
and rebuild the 1.7 DLL there, retaining its drag/Alt changes. No binary is included
because rebuilding this older base would overwrite unrelated integration work.
No 1.6 files, installed mods, game configuration, or other checkouts were changed.

Validation:

```powershell
$env:DOTNET_CLI_TELEMETRY_OPTOUT='1'
dotnet build Source/Merthsoft.DesignatorShapes.csproj
dotnet run --project Tests/ShortcutRegression/ShortcutRegression.csproj
```

The standalone regression runner loads the built production DLL and installed
RimWorld `KeyBindingDef`, `KeyBindingData`, `KeyPrefsData`, and Unity Event types.
All 691 checks passed. It checks both slots, remapping, cleared/missing preferences, all key codes with
and without Ctrl+Alt, standalone modifier conflicts, and Command suppression.
Build has the pre-existing SYSLIB0051 serialization warning.

Limitations: no live game input or dialog test was performed. After integration,
verify Ctrl+Alt+A can be assigned and selects its shape; plain A is still rejected
when native A is bound; duplicate shape/toggle chords reject; Escape preserves the
old binding; Backspace clears; shape modifier-only and Shift chords do not apply;
and the configurable interface toggle still uses its selected modifiers. Also
verify no earlier game/global handler acts on the chord before the designator
prefix consumes it. Arbitrary third-party custom chords are not discoverable from
native key-only preference data (same scope as before).
