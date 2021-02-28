using System;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes.Designators
{
    public class Undo : Designator
    {
        public Undo()
        {
            defaultLabel = "undo";
            defaultDesc = nameof(Undo);
            icon = Icons.Undo;
            useMouseIcon = true;
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 loc) => new("Cannot designate this.");

        public override void ProcessInput(Event ev)
        {
            HistoryManager.Undo();
            ev.Use();
        }
    }
}
