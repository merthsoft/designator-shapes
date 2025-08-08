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

    public bool BoundingBoxBasedOnShape = false;

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref RestoreAltToggle, nameof(RestoreAltToggle), false);
        Scribe_Values.Look(ref HideCompletelyOnAltToggle, nameof(HideCompletelyOnAltToggle), false);
        Scribe_Values.Look(ref ToggleableInterface, nameof(ToggleableInterface), true);
        Scribe_Values.Look(ref FloodFillCellLimit, nameof(FloodFillCellLimit), 1500);
        Scribe_Values.Look(ref AutoSelectShape, nameof(AutoSelectShape), false);
        Scribe_Values.Look(ref ResetShapeOnResume, nameof(ResetShapeOnResume), false);
        Scribe_Values.Look(ref DrawBackground, nameof(DrawBackground), false);
        Scribe_Values.Look(ref IconSize, nameof(IconSize), 40);
        Scribe_Values.Look(ref WindowX, nameof(WindowX), -1);
        Scribe_Values.Look(ref WindowY, nameof(WindowY), -1);
        Scribe_Values.Look(ref PauseOnFloodFillSelect, nameof(PauseOnFloodFillSelect), true);
        Scribe_Values.Look(ref EnableKeyboardInput, nameof(EnableKeyboardInput), true);
        Scribe_Collections.Look(ref Keys, nameof(Keys), LookMode.Value);
        Scribe_Values.Look(ref HideWhenNoOpenTab, nameof(HideWhenNoOpenTab), false);
        Scribe_Values.Look(ref LockPanelInPlace, nameof(LockPanelInPlace), false);
        Scribe_Values.Look(ref AnnounceToolSelection, nameof(AnnounceToolSelection), true);
        Scribe_Values.Look(ref EnableRotationKeys, nameof(EnableRotationKeys), false);
        Scribe_Values.Look(ref BoundingBoxBasedOnShape, "BoundingBoxBasedOnShape", false);

        if (Keys == null || Keys.Count == 0)
            Keys = [];

        for (int i = 1; i <= KeySettings.DefaultKeys?.Count; i++)
            if (Keys.Count < i)
                Keys.Add(KeySettings.DefaultKeys[i - 1]);

        DesignatorShapes.ShapeControls = new Ui.ShapeControlsWindow(WindowX, WindowY, IconSize);
    }
}
