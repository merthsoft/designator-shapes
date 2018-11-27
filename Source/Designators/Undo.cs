using System;
using Verse;

namespace Merthsoft.DesignatorShapes.Designators {
    public class Undo : Designator {
        public Undo() {
            defaultLabel = "undo";
            defaultDesc = "Undo";
            icon = Icons.Undo;
            useMouseIcon = true;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 loc) => new AcceptanceReport("Cannot designate this.");

        public override void ProcessInput(UnityEngine.Event ev) {
            HistoryManager.Undo();
            ev.Use();
        }
    }
}
