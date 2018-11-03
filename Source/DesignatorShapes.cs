using Harmony;
using Merthsoft.DesignatorShapes.Defs;
using Merthsoft.DesignatorShapes.Designators;
using RimWorld;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes {
    [StaticConstructorOnStartup]
    public class DesignatorShapes : Mod {
        private static bool defsLoaded = false;

        public static DesignatorShapeDef CurrentTool { get; set; }
        public static int Rotation { get; internal set; }

        public static Texture2D Icon_Settings { get; private set; }
        public static Texture2D Icon_Undo { get; private set; }
        public static Texture2D Icon_Redo { get; private set; }

        public static DesignatorSettings Settings { get; private set; }
        public static HarmonyInstance HarmonyInstance { get; private set; }

        public override string SettingsCategory() => "Designator Shapes";

        public static float SunLampRadius;
        public static float TradeBeaconRadius;

        public DesignatorShapes(ModContentPack content) : base(content) {
            Settings = GetSettings<DesignatorSettings>();
        }

        static DesignatorShapes() {
            HarmonyInstance = HarmonyInstance.Create("Merthsoft.DesignatorShapes");
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

            LongEventHandler.ExecuteWhenFinished(() => {
                Icon_Undo = getIcon("UI/Commands/undo");
                Icon_Redo = getIcon("UI/Commands/redo");
            });

            Rotation = 0;
        }

        public override void DoSettingsWindowContents(Rect inRect) {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            listing_Standard.CheckboxLabeled("Show shapes panel when designation is selected", ref Settings.ShowShapesPanelOnDesignationSelection);
            listing_Standard.CheckboxLabeled("Move shapes tab to end of list", ref Settings.MoveDesignationTabToEndOfList);

            var maxBuffer = Settings.FloodFillCellLimit.ToString();
            listing_Standard.TextFieldNumericLabeled<int>("Maximum cells to select in flood fill", ref Settings.FloodFillCellLimit, ref maxBuffer);

            listing_Standard.CheckboxLabeled("*EXPERIMENTAL* Show Undo and Redo buttons", ref Settings.ShowUndoAndRedoButtons);

            listing_Standard.End();
            Settings.Write();

            if (Settings.MoveDesignationTabToEndOfList) {
                DefDatabase<DesignationCategoryDef>.GetNamed("Shapes").order = 1;
            } else {
                DefDatabase<DesignationCategoryDef>.GetNamed("Shapes").order = 9999;
            }

            resolveShapes();

            var archWindow = (MainTabWindow_Architect)MainButtonDefOf.Architect.TabWindow;
            archWindow.InvokeMethod("CacheDesPanels");
        }

        public static void LoadDefs() {
            if (!defsLoaded) {
                resolveShapes();
                defsLoaded = true;
            }

            if (Settings.MoveDesignationTabToEndOfList) {
                DefDatabase<DesignationCategoryDef>.GetNamed("Shapes").order = 1;
                var archWindow = (MainTabWindow_Architect)MainButtonDefOf.Architect.TabWindow;
                archWindow.InvokeMethod("CacheDesPanels");
            }
        }

        private static void resolveShapes() {
            var shapes = Merthsoft.DesignatorShapes.Defs.DesignationCategoryDefOf.Shapes;
            var shapeDefs = DefDatabase<DesignatorShapeDef>.AllDefsListForReading;

            var numShapeDefs = shapeDefs.Count;

            shapes.ResolveReferences();
            if (Settings.ShowUndoAndRedoButtons) {
                shapes.AllResolvedDesignators.Add(new Undo());
                shapes.AllResolvedDesignators.Add(new Redo());
            }

            shapeDefs.ForEach(d => shapes.AllResolvedDesignators.Add(new Designator_Shape(d)));
            Log.Message($"{numShapeDefs} shape defs added: {string.Join(",", shapeDefs.Select(s => s.defName).ToArray())}");

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

        public static Texture2D getIcon(string line) {
            return ContentFinder<Texture2D>.Get(line, true);
        }

        internal static void SelectTool(DesignatorShapeDef def, bool announce = true) {
            if (announce && CurrentTool != def) {
                Messages.Message($"{def.LabelCap} designation shape selected.", MessageTypeDefOf.SilentInput);
            }
            CurrentTool = def;
            Rotation = 0;
        }
    }
}
