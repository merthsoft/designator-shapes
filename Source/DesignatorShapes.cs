using HarmonyLib;
using Merthsoft.DesignatorShapes.Defs;
using Merthsoft.DesignatorShapes.Shapes;
using Merthsoft.DesignatorShapes.Ui;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes;

[StaticConstructorOnStartup]
public class DesignatorShapes : Mod
{
    private static bool defsLoaded = false;

    public static DesignatorShapeDef CurrentTool { get; set; }

    public static DesignatorShapeDef CachedTool { get; set; }

    public static int Rotation { get; internal set; }

    private static bool showControls = true;

    public static bool ShowControls
    {
        get => !Settings.ToggleableInterface || showControls;

        set
        {
            if (Settings.ToggleableInterface)
                showControls = value;
        }
    }

    private static DesignatorSettings settings;

    public static DesignatorSettings Settings => settings ??= LoadedModManager.GetMod<DesignatorShapes>().GetSettings<DesignatorSettings>();

    public static Harmony HarmonyInstance { get; private set; }

    public override string SettingsCategory()
        => "Merthsoft_DesignatorShapes".Translate();

    public static float SunLampRadius;
    public static float TradeBeaconRadius;

    public static ShapeControlsWindow ShapeControls;
    public static bool FillCorners = true;

    private static int thickness = 1;

    public static int Thickness
    {
        get => thickness;
        private set => thickness = value switch { var v when v < -50 => 50, var v when v > 50 => 50, _ => value, };
    }

    private static Vector2 scrollPosition = Vector2.zero;

    public DesignatorShapes(ModContentPack content) : base(content)
    {
        if (GetSettings<DesignatorSettings>() == null)
            Log.Error("Unable to load DesignatorSettings.");
    }

    static DesignatorShapes()
    {
        HarmonyInstance = new Harmony("Merthsoft.DesignatorShapes");
        HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        Rotation = 0;
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        LoadDefs();

        var ls = new Listing_Standard();
        var outRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height - inRect.y);
        var viewRect = new Rect(0, 0, outRect.width - 16, (Text.LineHeight + 2) * (27 + KeySettings.DefaultKeys.Count));

        ls.Begin(viewRect);
        Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

        if (ls.ButtonText("Merthsoft_DesignatorShapes_Settings_ShowShapeDictionary".Translate()))
        {
            Find.WindowStack.Add(new ShapeDictionary());
            Event.current.Use();
        }

        ls.CheckboxLabeled("Merthsoft_DesignatorShapes_Settings_AnnounceToolSelection".Translate(), ref settings.AnnounceToolSelection);

        var maxBuffer = Settings.FloodFillCellLimit.ToString();
        ls.Label("Merthsoft_DesignatorShapes_Settings_MaxCells".Translate());
        ls.TextFieldNumeric(ref Settings.FloodFillCellLimit, ref maxBuffer);
        ls.GapLine();

        ls.CheckboxLabeled("Merthsoft_DesignatorShapes_Settings_AutoSelectShapes".Translate(), ref Settings.AutoSelectShape);
        ls.CheckboxLabeled("Merthsoft_DesignatorShapes_Settings_ResetShapeOnResume".Translate(), ref Settings.ResetShapeOnResume);
        ls.CheckboxLabeled("Merthsoft_DesignatorShapes_Settings_PauseOnFloodFill".Translate(), ref Settings.PauseOnFloodFillSelect);
        ls.CheckboxLabeled("Merthsoft_DesignatorShapes_Settings_HideWhenArchitectMenuIsHidden".Translate(), ref Settings.HideWhenNoOpenTab);

        ls.CheckboxLabeled("Merthsoft_DesignatorShapes_Settings_DrawBackground".Translate(), ref Settings.DrawBackground);
        ls.Label("Merthsoft_DesignatorShapes_Settings_IconSize".Translate(Settings.IconSize));
        Settings.IconSize = (int)ls.Slider(Settings.IconSize, 20, 80);

        ls.Label("Merthsoft_DesignatorShapes_Settings_WindowPosition".Translate());

        var buffer = Settings.WindowX.ToString();
        ls.TextFieldNumericLabeled("Merthsoft_DesignatorShapes_Settings_WindowX".Translate(), ref Settings.WindowX, ref buffer, -1);

        buffer = Settings.WindowY.ToString();
        ls.TextFieldNumericLabeled("Merthsoft_DesignatorShapes_Settings_WindowY".Translate(), ref Settings.WindowY, ref buffer, -1);

        ls.CheckboxLabeled("Merthsoft_DesignatorShapes_Settings_LockWindow".Translate(), ref Settings.LockPanelInPlace);

        ls.GapLine();

        ls.CheckboxLabeled("Merthsoft_DesignatorShapes_Settings_EnableKeyboardInput".Translate(), ref Settings.EnableKeyboardInput);
        ls.CheckboxLabeled("Merthsoft_DesignatorShapes_Settings_AllowCollapsing".Translate(), ref Settings.ToggleableInterface);
        if (Settings.EnableKeyboardInput)
        {
            if (Settings.ToggleableInterface)
                ls.CheckboxLabeled("\t" + "Merthsoft_DesignatorShapes_Settings_AltToggle".Translate(), ref Settings.RestoreAltToggle);
            ls.GapLine();
            ls.CheckboxLabeled("Merthsoft_DesignatorShapes_Settings_EnableRotation".Translate(), ref Settings.EnableRotationKeys);
            ls.Label("Merthsoft_DesignatorShapes_Settings_KeyBindings".Translate());

            for (var keyIndex = 0; keyIndex < Settings.Keys.Count; keyIndex++)
                DrawKeyInput(ls, keyIndex);
        }

        Widgets.EndScrollView();
        ls.End();
        Settings.Write();

        ShapeControls = new(Settings.WindowX, Settings.WindowY, Settings.IconSize);
    }

    private void DrawKeyInput(Listing_Standard ls, int keyIndex)
    {
        var keyLabel = KeySettings.KeyLabels[keyIndex].Translate();
        var buttonLabel = Settings.Keys[keyIndex].ToStringReadable();
        if (ls.ButtonTextLabeled(keyLabel, buttonLabel, TextAnchor.MiddleCenter))
            SettingButtonClicked(keyIndex);
        ls.Gap(2);
    }

    private void SettingButtonClicked(int keyIndex)
    {
        if (Event.current.button == 0)
        {
            Find.WindowStack.Add(new Ui.KeyBinding(keyIndex));
            Event.current.Use();
            return;
        }
        if (Event.current.button == 1)
        {
            List<FloatMenuOption> list = new()
            {
                new FloatMenuOption("ResetBinding".Translate(), delegate ()
                {
                    Settings.Keys[keyIndex] = KeySettings.DefaultKeys[keyIndex];
                }, MenuOptionPriority.Default, null, null, 0f, null, null),
                new FloatMenuOption("ClearBinding".Translate(), delegate ()
                {
                    Settings.Keys[keyIndex] = KeyCode.None;
                }, MenuOptionPriority.Default, null, null, 0f, null, null)
            };
            Find.WindowStack.Add(new FloatMenu(list));
        }
    }

    public static void LoadDefs()
    {
        if (!defsLoaded)
        {
            KeySettings.DefaultKeys = new([
                KeyBindingDefOf.Designator_RotateLeft?.MainKey ?? KeyCode.LeftBracket,
                KeyBindingDefOf.Designator_RotateRight?.MainKey ?? KeyCode.RightBracket,
                KeyBindingDefOf.Command_ItemForbid?.MainKey ?? KeyCode.F,
                KeyCode.Equals,
                KeyCode.Minus,
                KeyCode.Z,
                KeyCode.Y,
            ]);

            if (Settings.Keys == null)
                Settings.Keys = new(KeySettings.DefaultKeys);

            for (int i = 1; i <= KeySettings.DefaultKeys.Count; i++)
                if (Settings.Keys.Count < i)
                    Settings.Keys.Add(KeySettings.DefaultKeys[i - 1]);

            var shapeDefs = DefDatabase<DesignatorShapeDef>.AllDefsListForReading;
            defsLoaded = true;

            var sunlampDef = DefDatabase<ThingDef>.AllDefs.FirstOrDefault(d => d.defName == "Lighting_CeilingGrowLight")
                             ?? DefDatabase<ThingDef>.AllDefs.FirstOrDefault(d => d.defName == "SunLamp");
            if (sunlampDef != null)
                SunLampRadius = sunlampDef.specialDisplayRadius;

            var tradeRadiusInfo = AccessTools.Field(typeof(Building_OrbitalTradeBeacon), "TradeRadius");
            if (tradeRadiusInfo != null)
                TradeBeaconRadius = (float)tradeRadiusInfo.GetValue(null);

            ShapeControls = new ShapeControlsWindow(Settings?.WindowX ?? 0, Settings?.WindowY ?? 0, Settings?.IconSize ?? 40);
        }
    }

    public static bool RotateLeft()
        => Rotate(1);

    public static bool RotateRight()
        => Rotate(-1);

    private static bool Rotate(int amount)
    {
        if (CurrentTool.numRotations == 0)
            return false;
        Rotation += amount;
        if (Rotation < 0)
            Rotation = CurrentTool.numRotations + Rotation;
        else
            Rotation %= CurrentTool.numRotations;
        return true;
    }

    public static void IncreaseThickness()
    {
        if (Thickness == -2)
            // If we're coming from -2,
            Thickness = 1;     // skip straight to 1, because -1 and 0 are meaningless thicknesses
        else
            Thickness++;
    }

    public static void DecreaseThickness()
    {
        if (Thickness == 1)
            // If we're coming from 1,
            Thickness = -2;   // skip straight to -2, because -1 and 0 are meaningless thicknesses
        else
            Thickness--;
    }

    internal static void SelectTool(DesignatorShapeDef def)
    {
        FreeformLine.FreeMemory();
        if (def != null)
            CachedTool = def;
        if (Settings.AnnounceToolSelection && CurrentTool != def)
            Messages.Message("Merthsoft_DesignatorShapes_ToolSelected".Translate(def.LabelCap), MessageTypeDefOf.SilentInput, historical: false);
        CurrentTool = def;
        Rotation = 0;

        if (def.pauseOnSelection && Settings.PauseOnFloodFillSelect)
            Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
    }
}
