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
        if (!AccessTools.Property(typeof(DesignationDragger), "Dragging").GetValue(__instance).AsBool())
            return true;

        if (DesignatorShapes.ShowControls)
        {
            DesignatorShapes.DrawDraggerOutline(__instance);
            return false;
        }

        return true;
    }
}