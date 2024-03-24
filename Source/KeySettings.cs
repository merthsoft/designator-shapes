using System.Collections.Generic;
using UnityEngine;

namespace Merthsoft.DesignatorShapes;

public static class KeySettings
{
    public static int RotateLeftKeyIndex = 0;
    public static int RotateRightKeyIndex = 1;
    public static int FillCornersToggleKeyIndex = 2;
    public static int IncreaseThicknessKeyIndex = 3;
    public static int DecreaseThicknessKeyIndex = 4;
    public static int UndoKeyIndex = 5;
    public static int RedoKeyIndex = 6;

    public static string[] KeyLabels =
    [
        "Merthsoft_DesignatorShapes_KeyLabel_RotateLeft",
        "Merthsoft_DesignatorShapes_KeyLabel_RotateRight",
        "Merthsoft_DesignatorShapes_KeyLabel_FillCorners",
        "Merthsoft_DesignatorShapes_KeyLabel_IncreaseThickness",
        "Merthsoft_DesignatorShapes_KeyLabel_DecreaseThickness",
        "Merthsoft_DesignatorShapes_KeyLabel_Undo",
        "Merthsoft_DesignatorShapes_KeyLabel_Redo",
    ];


    public static List<KeyCode> DefaultKeys;
}