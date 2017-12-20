using Verse;

namespace Merthsoft.DesignatorShapes.Designators {
    class FillSettings : Designator {
        public FillSettings() {
            defaultLabel = "settings";
            defaultDesc = "";
            icon = DesignatorShapes.Icon_Settings;
            useMouseIcon = true;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 loc) {
            return new AcceptanceReport("Cannot designate this.");
        }

        public override void ProcessInput(UnityEngine.Event ev) {
            
        }
    }
}
