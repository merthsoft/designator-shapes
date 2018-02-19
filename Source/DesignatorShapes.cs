using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Merthsoft.DesignatorShapes.Defs;
using RimWorld;
using Merthsoft.DesignatorShapes.Designators;
using Harmony;
using System.Reflection;
using Merthsoft;

namespace Merthsoft.DesignatorShapes {
    [StaticConstructorOnStartup]
    public class DesignatorShapes : Mod {
        public static DesignatorShapeDef CurrentTool { get; set; }
        public static int Rotation { get; internal set; }

        public static Texture2D Icon_Settings;
        public static Texture2D Icon_Undo;

        public static GlobalSettings GlobalSettings;
        public static HarmonyInstance Harmony;

        public override string SettingsCategory() => "Designator Shapes";

        public DesignatorShapes(ModContentPack content) : base(content) {
            GlobalSettings = GetSettings<GlobalSettings>();
        }

        static DesignatorShapes() {
            Harmony = HarmonyInstance.Create("com.Merthsoft.DesignatorShapes");
            Harmony.PatchAll(Assembly.GetExecutingAssembly());

            //Icon_Settings = getIcon("UI/Commands/settings");
            LongEventHandler.ExecuteWhenFinished(() => Icon_Undo = getIcon("UI/Commands/undo"));

            Rotation = 0;
        }

        public override void DoSettingsWindowContents(Rect inRect) {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            listing_Standard.CheckboxLabeled("Show shapes panel when designation is selected", ref GlobalSettings.ShowShapesPanelOnDesignationSelection);
            listing_Standard.CheckboxLabeled("Move shapes tab to end of list", ref GlobalSettings.MoveDesignationTabToEndOfList);

            var maxBuffer = GlobalSettings.FloodFillCellLimit.ToString();
            listing_Standard.TextFieldNumericLabeled<int>("Maximum cells to select in flood fill", ref GlobalSettings.FloodFillCellLimit, ref maxBuffer);

            listing_Standard.End();
            GlobalSettings.Write();

            if (GlobalSettings.MoveDesignationTabToEndOfList) {
                DefDatabase<DesignationCategoryDef>.GetNamed("Shapes").order = 1;
            } else {
                DefDatabase<DesignationCategoryDef>.GetNamed("Shapes").order = 9999;
            }

            var archWindow = (MainTabWindow_Architect)MainButtonDefOf.Architect.TabWindow;
            archWindow.InvokeMethod("CacheDesPanels");
        }

        public static void LoadDefs() {
            var shapes = Merthsoft.DesignatorShapes.Defs.DesignationCategoryDefOf.Shapes;
            var shapeDefs = DefDatabase<DesignatorShapeDef>.AllDefsListForReading;

            var numShapeDefs = shapeDefs.Count;
            var numShapeDes = shapes.AllResolvedDesignators?.Count ?? 0;

            if (numShapeDefs > numShapeDes) {
                shapes.ResolveReferences();
                shapeDefs.ForEach(d => shapes.AllResolvedDesignators.Add(new Designator_Shape(d)));
            }

            var sb = new StringBuilder("Shapes resolved: ");
            shapes.AllResolvedDesignators.ForEach(d => sb.Append($"{d.LabelCap}, "));
            sb.Length -= 2;
            Log.Message(sb.ToString());

            if (GlobalSettings.MoveDesignationTabToEndOfList) {
                DefDatabase<DesignationCategoryDef>.GetNamed("Shapes").order = 1;
                var archWindow = (MainTabWindow_Architect)MainButtonDefOf.Architect.TabWindow;
                archWindow.InvokeMethod("CacheDesPanels");
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
