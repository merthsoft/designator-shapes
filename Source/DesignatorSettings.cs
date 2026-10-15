using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes;

public class DesignatorSettings : ModSettings
{
    public const bool DefaultRestoreAltToggle = false;
    public bool RestoreAltToggle = DefaultRestoreAltToggle;
    public bool ToggleableInterface = true;
    public bool HideCompletelyOnAltToggle = false;
    public int FloodFillCellLimit = 1500;
    public bool RestrictFloodFillToRevealedCells = false;
    public bool RememberShapePerDesignator = false;
    public bool CancelStrokeKeepsTool = false;
    public bool EnableCustomFloodFillBarriers = false;
    public bool ConfirmDestructiveFloodFill = false;
    public List<string> AdditionalFloodFillBarrierDefs = new();
    public Dictionary<string,KeyCode> ShapeShortcuts = new();
    public Dictionary<string,bool> VanillaPaletteShapes = new();
    public KeyCode InterfaceToggleKey = KeyCode.LeftAlt;
    public bool InterfaceToggleControl = false, InterfaceToggleAlt = false, InterfaceToggleShift = false;
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

    public bool HideCollapsedInterface => ToggleableInterface
        && EnableKeyboardInput && RestoreAltToggle && InterfaceToggleKey != KeyCode.None && HideCompletelyOnAltToggle;

    public BoundingBoxMode BoundingBoxMode = BoundingBoxMode.Vanilla;
    public bool BoundingBoxBasedOnShape = false;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref VanillaPaletteShapes, nameof(VanillaPaletteShapes), LookMode.Value, LookMode.Value);
        VanillaPaletteShapes ??= new();
        Scribe_Values.Look(ref RememberShapePerDesignator,nameof(RememberShapePerDesignator),false);
        Scribe_Values.Look(ref CancelStrokeKeepsTool,nameof(CancelStrokeKeepsTool),false);
        Scribe_Values.Look(ref EnableCustomFloodFillBarriers,nameof(EnableCustomFloodFillBarriers),false);
        Scribe_Values.Look(ref ConfirmDestructiveFloodFill,nameof(ConfirmDestructiveFloodFill),false);
        Scribe_Values.Look(ref InterfaceToggleKey,nameof(InterfaceToggleKey),KeyCode.LeftAlt);
        Scribe_Values.Look(ref InterfaceToggleControl,nameof(InterfaceToggleControl),false);
        Scribe_Values.Look(ref InterfaceToggleAlt,nameof(InterfaceToggleAlt),false);
        Scribe_Values.Look(ref InterfaceToggleShift,nameof(InterfaceToggleShift),false);
        Scribe_Collections.Look(ref ShapeShortcuts,nameof(ShapeShortcuts),LookMode.Value,LookMode.Value);
        Scribe_Collections.Look(ref AdditionalFloodFillBarrierDefs,nameof(AdditionalFloodFillBarrierDefs),LookMode.Value);
        ShapeShortcuts ??= new();AdditionalFloodFillBarrierDefs ??= new();

        // Note: We're using strings instead of nameof() just in case we want to change
        //       the variable name in the future, it won't change the save entry.
        Scribe_Values.Look(ref RestoreAltToggle, "RestoreAltToggle", DefaultRestoreAltToggle);
        Scribe_Values.Look(ref HideCompletelyOnAltToggle, "HideCompletelyOnAltToggle", false);
        Scribe_Values.Look(ref ToggleableInterface, "ToggleableInterface", true);
        Scribe_Values.Look(ref FloodFillCellLimit, "FloodFillCellLimit", 1500);
        Scribe_Values.Look(ref RestrictFloodFillToRevealedCells, "RestrictFloodFillToRevealedCells", false);
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
