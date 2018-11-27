using System;
using Verse;

namespace Merthsoft.DesignatorShapes.Designators {
    public class Redo : Designator {
        public Redo() {
            defaultLabel = "redo";
            defaultDesc = "Redo";
            icon = Icons.Redo;
            useMouseIcon = true;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 loc) => new AcceptanceReport("Cannot designate this.");

        public override void ProcessInput(UnityEngine.Event ev) {
            HistoryManager.Redo();
            ev.Use();
        }
    }
}
