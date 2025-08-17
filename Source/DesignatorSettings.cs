using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes;

public class DesignatorSettings : ModSettings
{
    public bool RestoreAltToggle = false;
    public bool ToggleableInterface = true;
    public bool HideCompletelyOnAltToggle = false;
    public int FloodFillCellLimit = 1500;
    public bool AutoSelectShape = false;
    public bool ResetShapeOnResume = false;
    public bool DrawBackground = true;
    public int IconSize = 40;
    public int WindowX = -1;
    public int WindowY = -1;

    public bool PauseOnFloodFillSelect = true;
    public bool EnableKeyboardInput = true;
    public List<KeyCode> Keys = [];

    public bool HideWhenNoOpenTab = false;
    public bool LockPanelInPlace = false;

    public bool AnnounceToolSelection = true;

    public bool EnableRotationKeys = false;

    public BoundingBoxMode BoundingBoxMode = BoundingBoxMode.Vanilla;
    public bool BoundingBoxBasedOnShape = false;

    public override void ExposeData()
    {
        base.ExposeData();

        // Note: We're using strings instead of nameof() just in case we want to change
        //       the variable name in the future, it won't change the save entry.
        Scribe_Values.Look(ref RestoreAltToggle, "RestoreAltToggle", false);
        Scribe_Values.Look(ref HideCompletelyOnAltToggle, "HideCompletelyOnAltToggle", false);
        Scribe_Values.Look(ref ToggleableInterface, "ToggleableInterface", true);
        Scribe_Values.Look(ref FloodFillCellLimit, "FloodFillCellLimit", 1500);
        Scribe_Values.Look(ref AutoSelectShape, "AutoSelectShape", false);
        Scribe_Values.Look(ref ResetShapeOnResume, "ResetShapeOnResume", false);
        Scribe_Values.Look(ref DrawBackground, "DrawBackground", false);
        Scribe_Values.Look(ref IconSize, "IconSize", 40);
        Scribe_Values.Look(ref WindowX, "WindowX", -1);
        Scribe_Values.Look(ref WindowY, "WindowY", -1);
        Scribe_Values.Look(ref PauseOnFloodFillSelect, "PauseOnFloodFillSelect", true);
        Scribe_Values.Look(ref EnableKeyboardInput, "EnableKeyboardInput", true);
        Scribe_Collections.Look(ref Keys, "Keys", LookMode.Value);
        Scribe_Values.Look(ref HideWhenNoOpenTab, "HideWhenNoOpenTab", false);
        Scribe_Values.Look(ref LockPanelInPlace, "LockPanelInPlace", false);
        Scribe_Values.Look(ref AnnounceToolSelection, "AnnounceToolSelection", true);
        Scribe_Values.Look(ref EnableRotationKeys, "EnableRotationKeys", false);
        Scribe_Values.Look(ref BoundingBoxBasedOnShape, "BoundingBoxBasedOnShape", false);
        Scribe_Values.Look(ref BoundingBoxMode, "BoundingBoxMode", BoundingBoxMode.Vanilla);

        if (Keys == null || Keys.Count == 0)
            Keys = [];

        for (int i = 1; i <= KeySettings.DefaultKeys?.Count; i++)
            if (Keys.Count < i)
                Keys.Add(KeySettings.DefaultKeys[i - 1]);

        DesignatorShapes.ShapeControls = new Ui.ShapeControlsWindow(WindowX, WindowY, IconSize);
    }
}
