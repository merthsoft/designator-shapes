using System;
using System.Linq;
using Merthsoft.DesignatorShapes.Defs;
using Merthsoft.DesignatorShapes.Shapes;
using RimWorld;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes;

internal static class FloodFillConfirmation
{
    private static Designator pressedTool;
    private static Map pressedMap;
    internal static bool IsDestructive(Designator tool) => tool is Designator_Cancel or Designator_Plan_Remove
        or Designator_Mine or Designator_Deconstruct or Designator_PlantsCut or Designator_RemoveFloor;
    internal static bool HasPendingPress => pressedTool != null;
    internal static void CancelPendingPress() { pressedTool = null; pressedMap = null; }
    private static bool OwnFillActive => (DesignatorShapes.UseCustomGeometry && DesignatorShapes.CurrentTool == DesignatorShapeDefOf.FloodFill) || (!DesignatorShapes.ShowControls && PaletteShapeWorker.SelectedShape == DesignatorShapeDefOf.FloodFill);
    internal static bool BeforeInput(DesignatorManager manager)
    {
        var e = Event.current; if (e == null) return true;
        if (ShapeShortcuts.InputBlocked) { CancelPendingPress(); return true; }
        var tool = manager.SelectedDesignator; var map = Find.CurrentMap; var style = manager.SelectedStyle;
        if (e.type == EventType.MouseDown)
        {
            CancelPendingPress();
            if (e.button == 0 && OwnFillActive && DesignatorShapes.Settings.ConfirmDestructiveFloodFill && IsDestructive(tool))
            {
                pressedTool = tool; pressedMap = map;
                // A null vanilla style commits on MouseDown; defer that route too.
                if (style == null) { e.Use(); return false; }
            }
        }
        if (e.type != EventType.MouseUp || e.button != 0) return true;
        bool started = pressedTool == tool && pressedMap == map;
        CancelPendingPress();
        if (!DesignatorShapes.Settings.ConfirmDestructiveFloodFill || !OwnFillActive || !IsDestructive(tool)
            || !(manager.Dragger.Dragging || started && (style == null || style.DrawStyleWorker.SingleCell))) return true;
        if (map == null || !UI.MouseCell().InBounds(map))
        { ShapeShortcuts.CancelStroke(manager); e.Use(); return false; }
        var seed = UI.MouseCell();
        var preview = style == null ? FloodFill.Fill(UI.MouseCell(), UI.MouseCell()) : manager.Dragger.DragCells;
        var cells = preview.Where(c => c.InBounds(map) && tool.CanDesignateCell(c).Accepted).Distinct().ToArray();
        var shape = DesignatorShapes.CurrentTool;
        ShapeShortcuts.CancelStroke(manager); e.Use();
        if (cells.Length == 0) return false;
        var pending = new PendingFloodFillAction(
            () => Find.CurrentMap == map && manager.SelectedDesignator == tool && manager.SelectedStyle == style && OwnFillActive && DesignatorShapes.CurrentTool == shape,
            () =>
            {
                var allowed = FloodFill.Fill(seed, seed).ToHashSet();
                var accepted = cells.Where(c => allowed.Contains(c) && c.InBounds(map) && tool.CanDesignateCell(c).Accepted).ToArray();
                if (accepted.Length > 0) tool.DesignateMultiCell(accepted);
            });
        Find.WindowStack.Add(new FloodFillConfirmationDialog(
            "Merthsoft_DesignatorShapes_ConfirmDestructiveText".Translate(tool.LabelCap, cells.Length), pending.Confirm, pending.Cancel));
        return false;
    }
}
