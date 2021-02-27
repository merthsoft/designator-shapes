using HarmonyLib;
using Merthsoft.DesignatorShapes.Defs;
using Merthsoft.DesignatorShapes.Designators;
using Merthsoft.DesignatorShapes.Dialogs;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes {

    [StaticConstructorOnStartup]
    public class DesignatorShapes : Mod {
        private static bool defsLoaded = false;
        private static DesignationCategoryDef shapeCategoryDef;

        public static DesignatorShapeDef CurrentTool { get; set; }
        public static DesignatorShapeDef CachedTool { get; set; }
        public static int Rotation { get; internal set; }

        private static bool showControls = true;
        public static bool ShowControls {
            get {
                return !Settings.ToggleableInterface || showControls;
            }

            set {
                if (Settings.ToggleableInterface) {
                    showControls = value;
                }
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
        public static int Thickness {
            get {
                return thickness;
            }
            private set {
                thickness = value switch
                {
                    var v when v < -50 => 50,
                    var v when v > 50 => 50,
                    _ => value,
                };
            }
        }

        private static Rect viewRect = Rect.zero;
        private static Vector2 scrollPosition = Vector2.zero;

        public DesignatorShapes(ModContentPack content) : base(content) {
            if (GetSettings<DesignatorSettings>() == null) {
                Log.Error("Unable to load DesignatorSettings.");
            }
        }

        static DesignatorShapes() {
            HarmonyInstance = new Harmony("Merthsoft.DesignatorShapes");
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
            Rotation = 0;
        }

        public override void DoSettingsWindowContents(Rect inRect) {
            LoadDefs();

            Listing_Standard ls = new Listing_Standard();

            ls.Begin(inRect);
            var scrollRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height - 150);
            ls.BeginScrollView(scrollRect, ref scrollPosition, ref viewRect);

            var maxBuffer = Settings.FloodFillCellLimit.ToString();
            ls.TextFieldNumericLabeled("Maximum cells to select in flood fill", ref Settings.FloodFillCellLimit, ref maxBuffer);

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
            ls.GapLine();
            ls.CheckboxLabeled("Use sub-menu navigation.", ref Settings.UseSubMenus);
            ls.CheckboxLabeled("Auto-select shapes when opening designation panels.", ref Settings.AutoSelectShape);
            ls.CheckboxLabeled("Reset the shape when you resume the game.", ref Settings.ResetShapeOnResume);
            //ls.CheckboxLabeled("Pause on flood fill selected.", ref Settings.PauseOnFloodFill);
            ls.GapLine();
            ls.CheckboxLabeled("Allow collapsing the interface.", ref Settings.ToggleableInterface);
            ls.CheckboxLabeled("Enable keyboard inputs", ref Settings.EnableKeyboardInput);
            ls.GapLine();
            if (Settings.EnableKeyboardInput)
            {
                if (Settings.ToggleableInterface)
                    ls.CheckboxLabeled("Allow toggling the interface with the alt-key.", ref Settings.RestoreAltToggle);
                ls.Label("Key bindings:");
                
                for (var keyIndex = 0; keyIndex < Settings.Keys.Count; keyIndex++)
                    DrawKeyInput(ls, keyIndex);
            }

            ls.EndScrollView(ref viewRect);
            ls.End();
            Settings.Write();

            ResolveShapes();

            ShapeControls = new(Settings.WindowX, Settings.WindowY, Settings.IconSize);
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

        public static void LoadDefs() {
            if (!defsLoaded) {
                defsLoaded = true;
                shapeCategoryDef = DefDatabase<DesignationCategoryDef>.GetNamed("Shapes");
                ResolveShapes();
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

            var archWindow = MainButtonDefOf.Architect.TabWindow;

            if (!DefDatabase<DesignationCategoryDef>.AllDefs.Contains(shapeCategoryDef)) {
                DefDatabase<DesignationCategoryDef>.Add(shapeCategoryDef);
            }
            if (Settings.MoveDesignationTabToEndOfList) {
                shapeCategoryDef.order = 1;
            }

            archWindow.InvokeMethod("CacheDesPanels");
        }

        private static void ResolveShapes() {
            var shapes = Defs.DesignationCategoryDefOf.Shapes;
            var shapeDefs = DefDatabase<DesignatorShapeDef>.AllDefsListForReading;

            var numShapeDefs = shapeDefs.Count;

            shapes.ResolveReferences();

            shapes.AllResolvedDesignators.Add(new Undo());
            shapes.AllResolvedDesignators.Add(new Redo());

            shapeDefs.ForEach(d => shapes.AllResolvedDesignators.Add(new Designator_Shape(d)));

            var groups = DefDatabase<OverlayGroupDef>.AllDefsListForReading;
            var groupCache = new Dictionary<string, OverlayGroupDef>();
            groups.ForEach(g => {
                if (!groupCache.ContainsKey(g.defName)) {
                    groupCache[g.defName] = g;
                    g.ChildrenGroups?.Clear();
                }

                g.UiIcon = Icons.GetIcon(g.uiIconPath);

                if (g.closeUiIconPath != null) {
                    g.CloseUiIcon = Icons.GetIcon(g.closeUiIconPath);
                }

                g.Shapes = shapeDefs.Where(s => s.overlayGroup == g.defName).ToList();
                g.Shapes.ForEach(s => {
                    s.Group = g;
                    s.RootGroup = g?.ParentGroup ?? g;
                });

                if (g.parentGroupName != null) {
                    var parent = groupCache[g.parentGroupName];
                    g.ParentGroup = parent;

                    parent.ChildrenGroups.Add(g);
                }
            });

            var sunlampDef = DefDatabase<ThingDef>.AllDefs.FirstOrDefault(d => d.defName == "SunLamp");
            if (sunlampDef != null) {
                SunLampRadius = sunlampDef.specialDisplayRadius;
            }

            var tradeRadiusInfo = AccessTools.Field(typeof(Building_OrbitalTradeBeacon), "TradeRadius");
            if (tradeRadiusInfo != null) {
                TradeBeaconRadius = (float)tradeRadiusInfo.GetValue(null);
            }
        }

        public static bool Rotate(int amount) {
            if (CurrentTool.numRotations == 0) { return false; }

            Rotation += amount;
            if (Rotation < 0) {
                Rotation = CurrentTool.numRotations + Rotation;
            } else {
                Rotation %= CurrentTool.numRotations;
            }
            return true;
        }

        public static void IncreaseThickness() {
            if (Thickness == -2) { // If we're coming from -2,
                Thickness = 1;     // skip straight to 1, because -1 and 0 are meaningless thicknesses
            } else {
                Thickness++;
            }
            Log.Message($"Thickness: {Thickness}");
        }

        public static void DecreaseThickness() {
            if (Thickness == 1) { // If we're coming from 1,
                Thickness = -2;   // skip straight to -2, because -1 and 0 are meaningless thicknesses
            } else {
                Thickness--;
            }
            Log.Message($"Thickness: {Thickness}");
        }

        internal static void SelectTool(DesignatorShapeDef def, bool announce = true) {
            if (def != null) { CachedTool = def; }
            if (announce && CurrentTool != def) {
                Messages.Message($"{def.LabelCap} designation shape selected.", MessageTypeDefOf.SilentInput);
            }
            CurrentTool = def;
            Rotation = 0;

            //if (def.pauseOnSelection && Settings.PauseOnFloodFill)
            //    Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
        }
    }
}