using System;
using Verse;

namespace Merthsoft.DesignatorShapes {
    public class GlobalSettings : ModSettings {
        public bool ShowShapesPanelOnDesignationSelection = true;
        public bool MoveDesignationTabToEndOfList = false;
        public int FloodFillCellLimit = 1500;

        public override void ExposeData() {
            base.ExposeData();

            Scribe_Values.Look(ref ShowShapesPanelOnDesignationSelection, "ShowShapesPanelOnDesignationSelection", true);
            Scribe_Values.Look(ref MoveDesignationTabToEndOfList, "MoveDesignationTabToEndOfList", false);
            Scribe_Values.Look(ref FloodFillCellLimit, "FloodFillCellLimit", 1500);
        }
    }
}
