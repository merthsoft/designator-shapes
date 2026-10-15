using HarmonyLib;
using Verse;

namespace Merthsoft.DesignatorShapes.Patches;

[HarmonyPatch(typeof(DesignationManager), "AddDesignation")]
public class DesignationManager_AddDesignation
{
    public static void Postfix(Designation newDes)
    {
        if (!DesignatorShapes.UseCustomGeometry)
            return;
        HistoryManager.AddEntry(newDes);
    }
}
