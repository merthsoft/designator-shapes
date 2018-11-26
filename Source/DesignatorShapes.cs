using Harmony;
using Merthsoft.DesignatorShapes.Defs;
using Merthsoft.DesignatorShapes.Designators;
using RimWorld;
using System;
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

        public static Texture2D Icon_Settings { get; private set; }
        public static Texture2D Icon_Undo { get; private set; }
        public static Texture2D Icon_Redo { get; private set; }

        public static Texture2D Icon_UndoEnabled { get; private set; }
        public static Texture2D Icon_RedoEnabled { get; private set; }

        public static Texture2D Icon_UndoDisabled { get; private set; }
        public static Texture2D Icon_RedoDisabled { get; private set; }

        public static DesignatorSettings Settings => LoadedModManager.GetMod<DesignatorShapes>().GetSettings<DesignatorSettings>();
        public static HarmonyInstance HarmonyInstance { get; private set; }

        public override string SettingsCategory() => "Designator Shapes";

        public static float SunLampRadius;
        public static float TradeBeaconRadius;

        public static ShapeControls ShapeControls;
        public static bool FillCorners = true;

        public DesignatorShapes(ModContentPack content) : base(content) {
            if (GetSettings<DesignatorSettings>() == null) {
                Log.Error("Unable to load DesignatorSettings.");
            }
        }

        static DesignatorShapes() {
            HarmonyInstance = HarmonyInstance.Create("Merthsoft.DesignatorShapes");
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

            LongEventHandler.ExecuteWhenFinished(() => {
                Icon_Undo = GetIcon("UI/Commands/undo");
                Icon_Redo = GetIcon("UI/Commands/redo");

                Icon_UndoEnabled = GetIcon("UI/Commands/Smol/undo");
                Icon_RedoEnabled = GetIcon("UI/Commands/Smol/redo");


                Icon_UndoDisabled = GetIcon("UI/Commands/Smol/undo_disabled");
                Icon_RedoDisabled = GetIcon("UI/Commands/Smol/redo_disabled");
            });

            Rotation = 0;
        }

        public override void DoSettingsWindowContents(Rect inRect) {
            LoadDefs();

            Listing_Standard ls = new Listing_Standard();
            ls.Begin(inRect);

            var maxBuffer = Settings.FloodFillCellLimit.ToString();
            ls.TextFieldNumericLabeled<int>("Maximum cells to select in flood fill", ref Settings.FloodFillCellLimit, ref maxBuffer);

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

            ls.CheckboxLabeled("Use sub-menu navigation.", ref Settings.UseSubMenus);
            ls.CheckboxLabeled("Auto-select shapes when opening designation panels.", ref Settings.AutoSelectShape);
            ls.CheckboxLabeled("Reset the shape when you resume the game.", ref Settings.ResetShapeOnResume);

            ls.CheckboxLabeled("Use old UI", ref Settings.UseOldUi);

            if (Settings.UseOldUi) {
                ls.CheckboxLabeled("Show shapes panel when designation is selected", ref Settings.ShowShapesPanelOnDesignationSelection);
                ls.CheckboxLabeled("Move shapes tab to end of list", ref Settings.MoveDesignationTabToEndOfList);
            }

            ls.End();
            Settings.Write();

            resolveShapes();

            ShapeControls.WindowRect = new Rect(Settings.WindowX, Settings.WindowY, ShapeControls.Width, ShapeControls.Height);
        }

        public static void LoadDefs() {
            if (!defsLoaded) {
                shapeCategoryDef = shapeCategoryDef ?? DefDatabase<DesignationCategoryDef>.GetNamed("Shapes");
                defsLoaded = true;
                resolveShapes();
            }

            var archWindow = MainButtonDefOf.Architect.TabWindow;
            if (!Settings.UseOldUi) {
                typeof(DefDatabase<DesignationCategoryDef>).InvokeStaticMethod("Remove", shapeCategoryDef);
            } else {
                if (!DefDatabase<DesignationCategoryDef>.AllDefs.Contains(shapeCategoryDef)) {
                    DefDatabase<DesignationCategoryDef>.Add(shapeCategoryDef);
                }
                if (Settings.MoveDesignationTabToEndOfList) {
                    shapeCategoryDef.order = 1;
                }
            }

            archWindow.InvokeMethod("CacheDesPanels");
        }

        private static void resolveShapes() {
            var shapes = Defs.DesignationCategoryDefOf.Shapes;
            var shapeDefs = DefDatabase<DesignatorShapeDef>.AllDefsListForReading;

            var numShapeDefs = shapeDefs.Count;

            shapes.ResolveReferences();

            shapes.AllResolvedDesignators.Add(new Undo());
            shapes.AllResolvedDesignators.Add(new Redo());

            shapeDefs.ForEach(d => shapes.AllResolvedDesignators.Add(new Designator_Shape(d)));

            var groups = DefDatabase<OverlayGroupDef>.AllDefsListForReading;
            groups.ForEach(g => {
                g.uiIcon = GetIcon(g.uiIconPath);

                if (g.closeUiIconPath != null) {
                    g.closeUiIcon = GetIcon(g.closeUiIconPath);
                }

                g.Shapes = shapeDefs.Where(s => s.overlayGroup == g.defName).ToList();
                g.Shapes.ForEach(s => s.Group = g);
            });

            ShapeControls = new ShapeControls(Settings.WindowX, Settings.WindowY);

            var sunlampDef = DefDatabase<ThingDef>.AllDefs.FirstOrDefault(d => d.defName == "SunLamp");
            if (sunlampDef != null) {
                SunLampRadius = sunlampDef.specialDisplayRadius;
            }

            var tradeRadiusInfo = AccessTools.Field(typeof(Building_OrbitalTradeBeacon), "TradeRadius");
            if (tradeRadiusInfo != null) {
                TradeBeaconRadius = (float)tradeRadiusInfo.GetValue(null);
            }
        }

        public static void Rotate(int amount) {
            Rotation += amount;
            if (Rotation < 0) {
                Rotation = CurrentTool.numRotations + Rotation;
            } else {
                Rotation %= CurrentTool.numRotations;
            }
        }

        public static Texture2D GetIcon(string path) {
            return ContentFinder<Texture2D>.Get(path, true);
        }

        internal static void SelectTool(DesignatorShapeDef def, bool announce = true) {
            if (def != null) { CachedTool = def; }
            if (announce && CurrentTool != def) {
                Messages.Message($"{def.LabelCap} designation shape selected.", MessageTypeDefOf.SilentInput);
            }
            CurrentTool = def;
            Rotation = 0;
        }
    }
}
