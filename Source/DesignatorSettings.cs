using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes
{
    public class DesignatorSettings : ModSettings
    {
        public static int RotateLeftKeyIndex = 0;
        public static int RotateRightKeyIndex = 1;
        public static int FillCornersToggleKeyIndex = 2;
        public static int IncreaseThicknessKeyIndex = 3;
        public static int DecreaseThicknessKeyIndex = 4;
        public static string[] KeyLabels = new[]
        {
            "Rotate shape left",
            "Rotate shape right",
            "Fill corners",
            "Increase thickness",
            "Decrease thickness",
        };


        public static List<KeyCode> DefaultKeys;

        public bool RestoreAltToggle = false;
        public bool ToggleableInterface = false;
        public int FloodFillCellLimit = 1500;
        public bool UseSubMenus = true;
        public bool AutoSelectShape = false;
        public bool ResetShapeOnResume = false;
        public bool DrawBackground = true;
        public int IconSize = 40;
        public int WindowX = -1;
        public int WindowY = -1;

        public bool PauseOnFloodFillSelect = true;
        public bool EnableKeyboardInput = true;
        public List<KeyCode> Keys = new();

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref RestoreAltToggle, nameof(RestoreAltToggle), false);
            Scribe_Values.Look(ref ToggleableInterface, nameof(ToggleableInterface), false);
            Scribe_Values.Look(ref FloodFillCellLimit, nameof(FloodFillCellLimit), 1500);
            Scribe_Values.Look(ref UseSubMenus, nameof(UseSubMenus), true);
            Scribe_Values.Look(ref AutoSelectShape, nameof(AutoSelectShape), false);
            Scribe_Values.Look(ref ResetShapeOnResume, nameof(ResetShapeOnResume), false);
            Scribe_Values.Look(ref DrawBackground, nameof(DrawBackground), false);
            Scribe_Values.Look(ref IconSize, nameof(IconSize), 40);
            Scribe_Values.Look(ref WindowX, nameof(WindowX), -1);
            Scribe_Values.Look(ref WindowY, nameof(WindowY), -1);
            Scribe_Values.Look(ref PauseOnFloodFillSelect, nameof(PauseOnFloodFillSelect), true);
            Scribe_Values.Look(ref EnableKeyboardInput, nameof(EnableKeyboardInput), true);
            Scribe_Collections.Look(ref Keys, nameof(Keys), LookMode.Value);

            if (Keys == null || Keys.Count == 0)
                Keys = new();

            DesignatorShapes.ShapeControls = new ShapeControls(WindowX, WindowY, IconSize);
        }
    }
}
