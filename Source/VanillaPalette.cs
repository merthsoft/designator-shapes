using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Merthsoft.DesignatorShapes.Defs;
using Merthsoft.DesignatorShapes.Shapes;
using UnityEngine;
using Verse;
namespace Merthsoft.DesignatorShapes;

public class PaletteShapeDef : DrawStyleDef
{
    public DesignatorShapeDef shape;
}

public class PaletteShapeWorker : DrawStyle
{
    internal static DesignatorShapeDef SelectedShape => Find.UIRoot is RimWorld.UIRoot_Play && Find.DesignatorManager.SelectedDesignator != null
        ? (Find.DesignatorManager?.SelectedStyle as PaletteShapeDef)?.shape : null;
    public override bool SingleCell => SelectedShape?.AllowDragging == false;
    public override void Update(IntVec3 origin, IntVec3 target, List<IntVec3> buffer)
    {
        buffer.Clear();
        var shape = SelectedShape;
        if (shape == null) return;
        if (shape.useSizeInputs) target = Ui.ShapeControlsWindow.InputVec;
        else (origin, target) = DrawingBounds.Current(shape, origin, target);
        buffer.AddRange(shape.DrawMethod(origin, target));
    }
    public override void Draw()
    {
        if (!DesignatorShapes.ShowControls && SelectedShape?.ShowSizeInputs == true)
            Find.WindowStack.ImmediateWindow(53187650, new Rect(275, UI.screenHeight - 185, 300, 44), WindowLayer.SubSuper,
                () => DesignatorShapes.ShapeControls.DrawNativeSizeInputs(new Rect(8, 8, 284, 28)), true, false, 0);
    }
}

internal static class VanillaPalette
{
    // Only these pre-existing additions default to enabled. Built-in and standalone
    // entries are never removed; generated styles have their own names.
    internal static readonly Dictionary<string,string> Legacy = new()
    {
        ["LineFreeform"] = "LineFreeform", ["Circle"] = "Circle", ["CircleFilled"] = "CircleFilled",
        ["SunLamp"] = "SunLamp", ["SunLampOutline"] = "SunLampOutline",
        ["TradeBeacon"] = "TradeBeacon", ["TradeBeaconOutline"] = "TradeBeaconOutline"
    };
    private static readonly Dictionary<DesignatorShapeDef,DrawStyleDef> styles = new();
    private static readonly Dictionary<DrawStyleCategoryDef,List<DrawStyleDef>> defaults = new();
    internal static bool Enabled(DesignatorShapeDef shape) => DesignatorShapes.Settings.VanillaPaletteShapes.TryGetValue(shape.defName, out var enabled)
        ? enabled : Legacy.ContainsKey(shape.defName);

    internal static void SetEnabled(DesignatorShapeDef shape, bool enabled)
    {
        DesignatorShapes.Settings.VanillaPaletteShapes[shape.defName] = enabled;
        Apply();
        DesignatorShapes.Settings.Write();
    }

    internal static void Apply()
    {
        foreach (var shape in DefDatabase<DesignatorShapeDef>.AllDefsListForReading.Where(ShapeEligibility.IsDrawable))
        {
            if (styles.ContainsKey(shape)) continue;
            DrawStyleDef style = Legacy.TryGetValue(shape.defName, out var legacy) ? DefDatabase<DrawStyleDef>.GetNamedSilentFail(legacy) : null;
            if (style == null)
            {
                // Shape textures may still be queued by PostLoad when styles are first created.
                // Load from the asset path instead of retaining an early null texture.
                style = new PaletteShapeDef { defName = "Merthsoft_Shapes_" + shape.defName, shape = shape,
                    label = shape.label, description = shape.description,
                    uiIcon = Icons.GetIcon(shape.uiIconPath),
                    drawStyleType = typeof(PaletteShapeWorker), drawOutline = true, canSnap = DrawingBounds.CanSnap(shape) };
                DefDatabase<DrawStyleDef>.Add(style);
            }
            styles.Add(shape, style);
        }
        foreach (var category in DefDatabase<DrawStyleCategoryDef>.AllDefsListForReading)
        {
            if (category.styles == null) continue;
            if (!defaults.ContainsKey(category)) defaults[category] = category.styles.ToList();
            var baseline = defaults[category];
            // Keep restricted single-style tools (e.g. foundations) restricted.
            if (!baseline.Any(s => s.defName is "EmptyRectangle" or "FilledOval" or "AngledLine")) continue;
            foreach (var pair in styles)
            {
                bool wanted = Enabled(pair.Key) && (!Legacy.ContainsKey(pair.Key.defName) || baseline.Contains(pair.Value));
                if (wanted && !category.styles.Contains(pair.Value)) category.styles.Add(pair.Value);
                else if (!wanted) category.styles.RemoveAll(s => s == pair.Value);
            }
        }
        if (Find.UIRoot is not RimWorld.UIRoot_Play) return;
        var manager = Find.DesignatorManager;
        var available = manager?.SelectedDesignator?.DrawStyleCategory?.styles;
        if (available != null && manager.SelectedStyle != null && styles.Values.Contains(manager.SelectedStyle) && !available.Contains(manager.SelectedStyle))
        {
            manager.Dragger?.EndDrag();
            manager.SelectedStyle = available.FirstOrDefault();
        }
    }
}

[HarmonyPatch(typeof(DesignationDragger), "UpdateCellBuffer")]
internal static class UpdatePaletteBuffer
{
    static bool Prefix(DesignationDragger __instance, IntVec3 ___startDragCell)
    {
        var shape = PaletteShapeWorker.SelectedShape;
        if (DesignatorShapes.ShowControls || shape == null) return true;
        // Fixed sizes and spacing can change while paused without moving the mouse.
        var origin = shape.AllowDragging ? ___startDragCell : UI.MouseCell();
        Find.DesignatorManager.SelectedStyle.DrawStyleWorker.Update(origin, UI.MouseCell(), __instance.CellBuffer);
        return false;
    }
}

[HarmonyPatch(typeof(DesignatorManager), "set_SelectedStyle")]
internal static class SelectPaletteShape
{
    static void Postfix(DrawStyleDef value)
    {
        if (DesignatorShapes.ShowControls || value is not PaletteShapeDef style) return;
        DesignatorShapes.CurrentTool = style.shape;
        DesignatorShapes.Rotation = 0;
        if (style.shape.pauseOnSelection && DesignatorShapes.Settings.PauseOnFloodFillSelect && Find.TickManager != null)
            Find.TickManager.CurTimeSpeed = TimeSpeed.Paused;
    }
}

[HarmonyPatch(typeof(DesignatorManager), "ChangeDrawStyle")]
internal static class ChangeNativePaletteStyle
{
    // Native Q/E cycling is separate from the configurable custom rotation keys.
    static bool Prefix() => !DesignatorShapes.UseCustomGeometry;
}
