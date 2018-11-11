using System;
using Verse;

namespace Merthsoft.DesignatorShapes {
    public class DesignatorSettings : ModSettings {
        public bool ShowShapesPanelOnDesignationSelection = true;
        public bool MoveDesignationTabToEndOfList = false;
        public int FloodFillCellLimit = 1500;
        [Obsolete] public bool ShowUndoAndRedoButtons = false;
        public bool UseOldUi = false;
        public bool UseSubMenus = true;
        public bool AutoSelectShape = false;
        public bool ResetShapeOnResume = false;

        public override void ExposeData() {
            base.ExposeData();

            Scribe_Values.Look(ref ShowShapesPanelOnDesignationSelection, nameof(ShowShapesPanelOnDesignationSelection), true);
            Scribe_Values.Look(ref MoveDesignationTabToEndOfList, nameof(MoveDesignationTabToEndOfList), false);
            Scribe_Values.Look(ref FloodFillCellLimit, nameof(FloodFillCellLimit), 1500);
            Scribe_Values.Look(ref UseOldUi, nameof(UseOldUi), false);
            Scribe_Values.Look(ref UseSubMenus, nameof(UseSubMenus), true);
            Scribe_Values.Look(ref AutoSelectShape, nameof(AutoSelectShape), false);
            Scribe_Values.Look(ref ResetShapeOnResume, nameof(ResetShapeOnResume), false);
        }
    }
}
