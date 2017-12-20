using System;
using Verse;

namespace Merthsoft.DesignatorShapes {
    public class GlobalSettings : ModSettings {
        public bool ShowShapesPanelOnDesignationSelection = true;

        public override void ExposeData() {
            base.ExposeData();

            Scribe_Values.Look(ref ShowShapesPanelOnDesignationSelection, "ShowShapesPanelOnDesignationSelection", true);
        }
    }
}
