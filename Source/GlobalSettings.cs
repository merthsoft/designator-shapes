using System;
using Verse;

namespace Merthsoft.DesignatorShapes {
    public class DesignatorSettings : ModSettings {
        public bool ShowShapesPanelOnDesignationSelection = true;
        public bool MoveDesignationTabToEndOfList = false;
        public int FloodFillCellLimit = 1500;
        public bool ShowUndoAndRedoButtons = false;

        public override void ExposeData() {
            base.ExposeData();

            Scribe_Values.Look(ref ShowShapesPanelOnDesignationSelection, "ShowShapesPanelOnDesignationSelection", true);
            Scribe_Values.Look(ref MoveDesignationTabToEndOfList, "MoveDesignationTabToEndOfList", false);
            Scribe_Values.Look(ref FloodFillCellLimit, "FloodFillCellLimit", 1500);
            Scribe_Values.Look(ref ShowUndoAndRedoButtons, "ShowUndoAndRedoButtons", false);
        }
    }
}
