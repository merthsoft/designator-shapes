using Verse;

namespace Merthsoft.DesignatorShapes.Designators {
    class FillSettings : Designator {
        public FillSettings() {
            defaultLabel = "settings";
            defaultDesc = "";
            icon = Icons.Settings;
            useMouseIcon = true;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 loc) => new AcceptanceReport("Cannot designate this.");

        public override void ProcessInput(UnityEngine.Event ev) {
            
        }
    }
}
