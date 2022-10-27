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
			// Skip the main call to RenderHighlight
			if (inst.opcode == OpCodes.Callvirt && (inst.operand as MethodInfo)?.Name == "RenderHighlight")
			{
				yield return new CodeInstruction(OpCodes.Pop);
				yield return new CodeInstruction(OpCodes.Pop);
				continue;
			}
			
			yield return inst;
        }
	}
	
	public static void Postfix(DesignationDragger __instance)
    {
        if (!(bool)__instance.GetInstanceField("dragging"))
            return;

		var cells = __instance.GetInstanceField<List<IntVec3>>("dragCells");
		var selDes = Find.DesignatorManager.SelectedDesignator;

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
