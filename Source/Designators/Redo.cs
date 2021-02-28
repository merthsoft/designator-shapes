using System;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes.Designators
{
    public class Redo : Designator
    {
        public Redo()
        {
            defaultLabel = "redo";
            defaultDesc = nameof(Redo);
            icon = Icons.Redo;
            useMouseIcon = true;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 loc) => new("Cannot designate this.");

        public override void ProcessInput(Event ev)
        {
            HistoryManager.Redo();
            ev.Use();
        }
    }
}
