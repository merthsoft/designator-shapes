using HarmonyLib;
using Verse;

namespace Merthsoft.DesignatorShapes.Patches;

[HarmonyPatch(typeof(Designator), "DoExtraGuiControls")]
internal static class Designator_DoExtraGuiControls
{
    public static bool Prefix()
        => !DesignatorShapes.ShowControls;
}
