using HarmonyLib;
using RimWorld;
using Verse;

namespace Merthsoft.DesignatorShapes.Patches;

[HarmonyPatch(typeof(PlaySettings), nameof(PlaySettings.DoPlaySettingsGlobalControls))]
public static class PlaySettings_DoPlaySettingsGlobalControls
{
    public static void Postfix(WidgetRow row)
    {
        row.ToggleableIcon(
            toggleable: ref DesignatorShapes.Settings.BoundingBoxBasedOnShape,
            tex: DesignatorShapes.BoundingBoxTexture,
            tooltip: "".Translate());
    }
}
