using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes;

public class DesignatorSettings : ModSettings
{
    public bool RestoreAltToggle = false;
    public bool ToggleableInterface = false;
    public int FloodFillCellLimit = 1500;
    [Obsolete] public bool UseSubMenus = true;
    public bool AutoSelectShape = false;
    public bool ResetShapeOnResume = false;
    public bool DrawBackground = true;
    public int IconSize = 40;
    public int WindowX = -1;
    public int WindowY = -1;

    public bool PauseOnFloodFillSelect = true;
    public bool EnableKeyboardInput = true;
    public List<KeyCode> Keys = new();

    [Obsolete] public bool UseOldUi = false;
    [Obsolete] public bool MoveDesignationTabToEndOfList = false;
    [Obsolete] public bool ShowShapesPanelOnDesignationSelection = true;

    public bool HideWhenNoOpenTab = false;
    public bool LockPanelInPlace = false;

    public bool AnnounceToolSelection = true;

    public bool DisableRotationKeys = false;

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref RestoreAltToggle, nameof(RestoreAltToggle), false);
        Scribe_Values.Look(ref ToggleableInterface, nameof(ToggleableInterface), false);
        Scribe_Values.Look(ref FloodFillCellLimit, nameof(FloodFillCellLimit), 1500);
#pragma warning disable CS0612 // Type or member is obsolete
        Scribe_Values.Look(ref UseOldUi, nameof(UseOldUi), false);
        Scribe_Values.Look(ref ShowShapesPanelOnDesignationSelection, nameof(ShowShapesPanelOnDesignationSelection), true);
        Scribe_Values.Look(ref MoveDesignationTabToEndOfList, nameof(MoveDesignationTabToEndOfList), false);
        Scribe_Values.Look(ref UseSubMenus, nameof(UseSubMenus), true);
#pragma warning restore CS0612 // Type or member is obsolete
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
        Scribe_Values.Look(ref DisableRotationKeys, nameof(DisableRotationKeys), false);

        if (Keys == null || Keys.Count == 0)
            Keys = new();

        for (int i = 1; i <= KeySettings.DefaultKeys.Count; i++)
            if (Keys.Count < i)
                Keys.Add(KeySettings.DefaultKeys[i - 1]);

        DesignatorShapes.ShapeControls = new Ui.ShapeControlsWindow(WindowX, WindowY, IconSize);
    }
}
