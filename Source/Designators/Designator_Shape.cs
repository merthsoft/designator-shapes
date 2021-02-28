using Merthsoft.DesignatorShapes.Defs;
using RimWorld;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes.Designators
{
    public class Designator_Shape : Designator_Build {
        public DesignatorShapeDef Def { get; set; }

        public override bool Visible => true;

        public Designator_Shape(BuildableDef entDef) : this(entDef as DesignatorShapeDef) { }

        public Designator_Shape(DesignatorShapeDef def) : base(def) {
            this.Def = def;
            LongEventHandler.ExecuteWhenFinished(() => icon = ContentFinder<Texture2D>.Get(def.uiIconPath));
            defaultDesc = def.description;
            defaultLabel = def.label;
            defaultDesc = def.description;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 loc) {
            return new AcceptanceReport("Cannot designate this.");
        }

        public override void ProcessInput(Event ev) {
            DesignatorShapes.SelectTool(Def);
        }
    }
}
