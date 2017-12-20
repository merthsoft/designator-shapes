using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Merthsoft.DesignatorShapes.Defs;

namespace Merthsoft.DesignatorShapes.Designators {
    public class Designator_Shape : Designator_Build {
        public DesignatorShapeDef def { get; set; }

        public override bool Visible => true;

        public Designator_Shape(BuildableDef entDef) : this(entDef as DesignatorShapeDef) { }

        public Designator_Shape(DesignatorShapeDef def) : base(def) {
            this.def = def;
            LongEventHandler.ExecuteWhenFinished(() => icon = ContentFinder<Texture2D>.Get(def.uiIconPath));
            defaultDesc = def.description;
            defaultLabel = def.label;
            defaultDesc = def.description;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 loc) {
            return new AcceptanceReport("Cannot designate this.");
        }

        public override void ProcessInput(Event ev) {
            DesignatorShapes.SelectTool(def);
        }
    }
}
