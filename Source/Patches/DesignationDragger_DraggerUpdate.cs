using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Verse;

namespace Merthsoft.DesignatorShapes.Patches;

[HarmonyPatch(typeof(DesignationDragger), "DraggerUpdate")]
internal class DesignationDragger_DraggerUpdate
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var inst in instructions)
        {
            // Custom geometry supplies its own cells below. Native palette workers,
            // including hover previews for fixed-size tools, retain vanilla highlighting.
            if (inst.opcode == OpCodes.Callvirt && inst.operand is MethodInfo { Name: "RenderHighlight" })
            {
                yield return new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(DesignationDragger_DraggerUpdate), nameof(RenderNativeHighlight)))
                    .MoveLabelsFrom(inst).MoveBlocksFrom(inst);
                continue;
            }

            yield return inst;
        }
    }

    private static void RenderNativeHighlight(Designator designator, List<IntVec3> cells)
    {
        if (!DesignatorShapes.UseCustomGeometry) designator?.RenderHighlight(cells);
    }

    public static void Postfix(DesignationDragger __instance)
    {
        if (!DesignatorShapes.UseCustomGeometry || !__instance.GetInstanceField<bool>("dragging"))
            return;

        var cells = __instance.DragCells;
        var selDes = Find.DesignatorManager.SelectedDesignator;
        if (selDes == null) return;

        var numSelectedCells = 0;
        var tmpHighlightCells = new List<IntVec3>();

        foreach (IntVec3 intVec in cells)
        {
            if (selDes.CanDesignateCell(intVec))
            {
                tmpHighlightCells.Add(intVec);
                numSelectedCells++;
            }
        }
        __instance.SetInstanceField<int>("numSelectedCells", numSelectedCells);

        selDes.RenderHighlight(tmpHighlightCells);
    }
}
