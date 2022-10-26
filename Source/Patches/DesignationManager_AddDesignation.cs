using HarmonyLib;

using Merthsoft.DesignatorShapes.Utils;

using Verse;

namespace Merthsoft.DesignatorShapes.Patches
{
    [HarmonyPatch(typeof(DesignationManager), "AddDesignation", new[] { typeof(Designation)})]
    public class DesignationManager_AddDesignation
    {
        public static void Postfix(ref Designation newDes)
        {
            if (!DesignatorShapes.ShowControls)
                return;
            HistoryManager.AddEntry(newDes);
            CoreLogger.Log("DesignationManager_AddDesignation");
        }
    }
}
