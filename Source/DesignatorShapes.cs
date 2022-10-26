using HarmonyLib;
using Merthsoft.DesignatorShapes.Defs;
using Merthsoft.DesignatorShapes.Dialogs;
using Merthsoft.DesignatorShapes.Utils;

using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes
{
    [StaticConstructorOnStartup]
    public class DesignatorShapes : Mod
    {
        private static bool defsLoaded = false;

        public static DesignatorShapeDef CurrentTool { get; set; }

        public static DesignatorShapeDef CachedTool { get; set; }

        public static int Rotation { get; internal set; }

        private static bool showControls = true;

        public static bool ShowControls
        {
            get => !Settings.ToggleableInterface || showControls;

            set
            {
                if (Settings.ToggleableInterface)
                    showControls = value;
            }
        }

        private static DesignatorSettings settings;

        public static DesignatorSettings Settings => settings ??= LoadedModManager.GetMod<DesignatorShapes>().GetSettings<DesignatorSettings>();

        public static Harmony HarmonyInstance { get; private set; }

        public override string SettingsCategory() => "Designator Shapes";

        public static float SunLampRadius;
        public static float TradeBeaconRadius;

        public static ShapeControls ShapeControls;
        public static bool FillCorners = true;

        private static int thickness = 1;

        public static int Thickness
        {
            get => thickness;
            private set => thickness = value switch { var v when v < -50 => 50, var v when v > 50 => 50, _ => value, };
        }

        private static Vector2 scrollPosition = Vector2.zero;

        public DesignatorShapes(ModContentPack content) : base(content)
        {
            CoreLogger.LogStartAndEndLow(() =>
            {
                if (GetSettings<DesignatorSettings>() == null)
                    Log.Error("Unable to load DesignatorSettings.");
            }, "Dynamic constructor");
        }

        static DesignatorShapes()
        {
            CoreLogger.LogStartAndEnd(() =>
            {
                HarmonyInstance = new Harmony("Merthsoft.DesignatorShapes");
                HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
                Rotation = 0;
            }, "Static constructor");
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            CoreLogger.LogStartAndEndLow(() =>
            {
                LoadDefs();

                var ls = new Listing_Standard();
                var outRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height - inRect.y);
                var viewRect = new Rect(0, 0, outRect.width - 16, Text.LineHeight * 30);

                ls.Begin(viewRect);
                Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

                var maxBuffer = Settings.FloodFillCellLimit.ToString();
                ls.Label("Maximum cells to select in flood fill");
                ls.TextFieldNumeric(ref Settings.FloodFillCellLimit, ref maxBuffer);
                ls.GapLine();

                ls.CheckboxLabeled("Use sub-menu navigation.", ref Settings.UseSubMenus);
                ls.CheckboxLabeled("Auto-select shapes when opening designation panels.", ref Settings.AutoSelectShape);
                ls.CheckboxLabeled("Reset the shape when you resume the game.", ref Settings.ResetShapeOnResume);
                ls.CheckboxLabeled("Pause on flood fill selected.", ref Settings.PauseOnFloodFillSelect);
                ls.CheckboxLabeled("Allow collapsing the interface.", ref Settings.ToggleableInterface);
                ls.CheckboxLabeled("Enable keyboard inputs", ref Settings.EnableKeyboardInput);
                ls.CheckboxLabeled("Hide when architect menu is hidden.", ref Settings.HideWhenNoOpenTab);

                ls.CheckboxLabeled("Draw background", ref Settings.DrawBackground);
                ls.Label($"Icon size: {Settings.IconSize}");
                Settings.IconSize = (int)ls.Slider(Settings.IconSize, 20, 80);

                ls.Label("Window position, set to -1, -1 to use default location.");

                ls.Label("Window X:");
                var buffer = Settings.WindowX.ToString();
                ls.IntEntry(ref Settings.WindowX, ref buffer);

                ls.Label("Window Y:");
                buffer = Settings.WindowY.ToString();
                ls.IntEntry(ref Settings.WindowY, ref buffer);

                ls.CheckboxLabeled("Lock the window in place do prevent mis-drags.", ref Settings.LockPanelInPlace);

                ls.GapLine();


                if (Settings.EnableKeyboardInput)
                {
                    if (Settings.ToggleableInterface)
                        ls.CheckboxLabeled("Allow toggling the interface with the alt-key.", ref Settings.RestoreAltToggle);
                    ls.Label("Key bindings:");

                    for (var keyIndex = 0; keyIndex < Settings.Keys.Count; keyIndex++)
                        DrawKeyInput(ls, keyIndex);

                    ls.GapLine();
                }

                Widgets.EndScrollView();
                ls.End();
                Settings.Write();

                ShapeControls = new(Settings.WindowX, Settings.WindowY, Settings.IconSize);
            }, "DoSettingsWindowContents");
        }

        private void DrawKeyInput(Listing_Standard ls, int keyIndex)
        {
            var keyLabel = DesignatorSettings.KeyLabels[keyIndex];
            var buttonLabel = Settings.Keys[keyIndex].ToStringReadable();
            if (ls.ButtonTextLabeled(keyLabel, buttonLabel))
                SettingButtonClicked(keyIndex);
        }

        private void SettingButtonClicked(int keyIndex)
        {
            if (Event.current.button == 0)
            {
                Find.WindowStack.Add(new KeyBinding(keyIndex));
                Event.current.Use();
                return;
            }
            if (Event.current.button == 1)
            {
                List<FloatMenuOption> list = new()
                {
                    new FloatMenuOption("ResetBinding".Translate(), delegate ()
                    {
                        Settings.Keys[keyIndex] = DesignatorSettings.DefaultKeys[keyIndex];
                    }, MenuOptionPriority.Default, null, null, 0f, null, null),
                    new FloatMenuOption("ClearBinding".Translate(), delegate ()
                    {
                        Settings.Keys[keyIndex] = KeyCode.None;
                    }, MenuOptionPriority.Default, null, null, 0f, null, null)
                };
                Find.WindowStack.Add(new FloatMenu(list));
            }
        }

        public static void LoadDefs()
        {
            CoreLogger.LogStartAndEndLow(() =>
            {
                if (!defsLoaded)
                {
                    var shapeDefs = DefDatabase<DesignatorShapeDef>.AllDefsListForReading;
                    defsLoaded = true;

                    var sunlampDef = DefDatabase<ThingDef>.AllDefs.FirstOrDefault(d => d.defName == "Lighting_CeilingGrowLight")
                                     ?? DefDatabase<ThingDef>.AllDefs.FirstOrDefault(d => d.defName == "SunLamp");
                    if (sunlampDef != null)
                        SunLampRadius = sunlampDef.specialDisplayRadius;

                    var tradeRadiusInfo = AccessTools.Field(typeof(Building_OrbitalTradeBeacon), "TradeRadius");
                    if (tradeRadiusInfo != null)
                        TradeBeaconRadius = (float)tradeRadiusInfo.GetValue(null);

                    ShapeControls = new ShapeControls(Settings?.WindowX ?? 0, Settings?.WindowY ?? 0, Settings?.IconSize ?? 40);

                    DesignatorSettings.DefaultKeys = new(new[]
                    {
                    KeyBindingDefOf.Designator_RotateLeft?.MainKey ?? KeyCode.Q,
                    KeyBindingDefOf.Designator_RotateRight?.MainKey ?? KeyCode.E,
                    KeyBindingDefOf.Command_ItemForbid?.MainKey ?? KeyCode.F,
                    KeyCode.Equals,
                    KeyCode.Minus,
                });

                    if (Settings.Keys == null || Settings.Keys?.Count == 0)
                        Settings.Keys = new(DesignatorSettings.DefaultKeys);
                }
            }, "LoadDefs");
        }

        public static bool Rotate(int amount)
        {
            return CoreLogger.LogStartAndEndLow(() =>
            {
                if (CurrentTool.numRotations == 0)
                    return false;
                Rotation += amount;
                if (Rotation < 0)
                    Rotation = CurrentTool.numRotations + Rotation;
                else
                    Rotation %= CurrentTool.numRotations;
                return true;
            }, "Rotate");
        }

        public static void IncreaseThickness()
        {
            CoreLogger.LogStartAndEndLow(() =>
            {
                if (Thickness == -2)
                    // If we're coming from -2,
                    Thickness = 1;     // skip straight to 1, because -1 and 0 are meaningless thicknesses
                else
                    Thickness++;
                Log.Message($"Thickness: {Thickness}");
            }, "IncreaseThickness");
        }

        public static void DecreaseThickness()
        {
            CoreLogger.LogStartAndEndLow(() =>
            {
                if (Thickness == 1)
                    // If we're coming from 1,
                    Thickness = -2;   // skip straight to -2, because -1 and 0 are meaningless thicknesses
                else
                    Thickness--;
                Log.Message($"Thickness: {Thickness}");
            }, "DecreaseThickness");
        }

        internal static void SelectTool(DesignatorShapeDef def, bool announce = true)
        {
            CoreLogger.LogStartAndEnd(() =>
            {
                if (def != null)
                    CachedTool = def;
                if (announce && CurrentTool != def)
                    Messages.Message($"{def.LabelCap} designation shape selected.", MessageTypeDefOf.SilentInput);
                CurrentTool = def;
                Rotation = 0;

                if (def.pauseOnSelection && Settings.PauseOnFloodFillSelect)
                    Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
            }, "SelectTool");
        }
    }
}