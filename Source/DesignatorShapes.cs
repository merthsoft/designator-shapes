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

namespace Merthsoft.DesignatorShapes {
    [StaticConstructorOnStartup]
    public class DesignatorShapes : Mod {
        public static DesignatorShapeDef CurrentTool { get; set; }
        public static int Rotation { get; internal set; }

        public static Texture2D Icon_Settings;

        public static GlobalSettings globalSettings;
        public static HarmonyInstance Harmony;

        public override string SettingsCategory() => "Designator Shapes";

        public DesignatorShapes(ModContentPack content) : base(content) {
            Log.Message("Designator Shapes instance initialization started");
            var timestamp = DateTime.Now;

            globalSettings = GetSettings<GlobalSettings>();

            var duration = DateTime.Now - timestamp;
            Log.Message($"Designator Shapes instance initialization completed in {duration.TotalMilliseconds} ms");
        }

        static DesignatorShapes() {
            Log.Message("Designator Shapes static initialization started");
            var timestamp = DateTime.Now;

            Harmony = HarmonyInstance.Create("com.Merthsoft.DesignatorShapes");
            Harmony.PatchAll(Assembly.GetExecutingAssembly());

            //Icon_Settings = getIcon("UI/Commands/settings");

            Rotation = 0;

            var duration = DateTime.Now - timestamp;
            Log.Message($"Designator Shapes static initialization completed in {duration.TotalMilliseconds} ms");
        }

        public override void DoSettingsWindowContents(Rect inRect) {
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            listing_Standard.CheckboxLabeled("Show shapes panel when designation is selected", ref globalSettings.ShowShapesPanelOnDesignationSelection);
            listing_Standard.End();
            globalSettings.Write();
        }

        public static void LoadDefs() {
            var shapes = Merthsoft.DesignatorShapes.Defs.DesignationCategoryDefOf.Shapes;
            var shapeDefs = DefDatabase<DesignatorShapeDef>.AllDefsListForReading;

            var numShapeDefs = shapeDefs.Count;
            var numShapeDes = shapes.AllResolvedDesignators?.Count ?? 0;

            if (numShapeDefs > numShapeDes) {
                shapes.ResolveReferences();
                shapeDefs.ForEach(d => shapes.AllResolvedDesignators.Add(new Designator_Shape(d)));

                var sb = new StringBuilder("Shapes resolved: ");
                shapes.AllResolvedDesignators.ForEach(d => sb.Append($"{d.LabelCap}, "));
                sb.Length -= 2;
                Log.Message(sb.ToString());
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
            DesignatorShapes.CurrentTool = def;
            Rotation = 0;
        }
    }
}
