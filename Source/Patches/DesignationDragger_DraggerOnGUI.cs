using HarmonyLib;
using Merthsoft.DesignatorShapes;
using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes.Patches;

[HarmonyPatch(typeof(DesignationDragger), nameof(DesignationDragger.DraggerOnGUI))]
public static class DesignationDragger_DraggerOnGUI
{
    public static bool Prefix(DesignationDragger __instance)
    {
        if (__instance == null)
            return true;

        if (!__instance.Dragging)
            return true;

        if (DesignatorShapes.ShowControls)
        {
            DesignatorShapes.DrawDraggerOutline(__instance);
            return false;
        }

        return true;
    }
}