using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes.Ui;
internal class ShapeDictionary : Window
{
    public static float LabelHeight => Text.LineHeight;
    public static float IconSize => Text.LineHeight * 2 + LineHeight;
    public static float LineHeight => 3;

    private const int IconPadding = 16;

    private readonly ShapeMenuDriver menuController = new() { DictionaryMode = true };
    private Vector2 ScrollPosition;

    private static float ContentHeight(ActionIcon icon, float width) => Math.Max(IconSize,
        LineHeight + LabelHeight + Text.CalcHeight(icon.description ?? "", width - IconSize - IconPadding));
    private static float RowHeight(ActionIcon icon, float width) => ContentHeight(icon, width) + IconPadding
        + (ShapeEligibility.IsDrawable(icon.shape) ? 31 : 0);

    private void DrawIcon(ActionIcon icon, Rect rect)
    {
        Widgets.Label(rect.Clone(addX: IconSize + IconPadding, addY: LineHeight), icon.label);
        Widgets.Label(new Rect(rect.x + IconSize + IconPadding, rect.y + LineHeight + LabelHeight,
            rect.width - IconSize - IconPadding, ContentHeight(icon, rect.width) - LineHeight - LabelHeight), icon.description);
        if (ShapeEligibility.IsDrawable(icon.shape))
        {
            bool enabled = VanillaPalette.Enabled(icon.shape), before = enabled;
            var checkbox = new Rect(rect.x + IconSize + IconPadding, rect.y + ContentHeight(icon, rect.width) + 6,
                rect.width - IconSize - IconPadding, 25);
            Widgets.CheckboxLabeled(checkbox,
                "Merthsoft_DesignatorShapes_IncludeVanillaPalette".Translate(), ref enabled);
            TooltipHandler.TipRegion(checkbox,
                "Merthsoft_DesignatorShapes_IncludeVanillaPaletteTooltip".Translate());
            if (enabled != before) VanillaPalette.SetEnabled(icon.shape, enabled);
        }

        if (Widgets.ButtonImage(rect.Clone(width: IconSize, height: IconSize), icon.icon, Color.white, icon.highlightColor ?? GenUI.MouseoverColor))
        {
            var previousGroup = menuController.SelectedGroup;
            icon.action?.Invoke();
            if (menuController.SelectedGroup != previousGroup) ScrollPosition = Vector2.zero;
        }
    }

    private void DrawIcons(Rect rect, List<ActionIcon> icons)
    {
        float y = rect.y;
        foreach (var icon in icons)
        {
            var height = RowHeight(icon, rect.width);
            DrawIcon(icon, new Rect(rect.x, y, rect.width, height));
            y += height;
        }
    }

    public override void DoWindowContents(Rect rect)
    {
        if (Widgets.CloseButtonFor(rect))
            Close();

        Text.Font = GameFont.Medium;

        Widgets.Label(rect.Clone(addX: 3), "Merthsoft_DesignatorShapes_Settings_ShowShapeDictionary".Translate());
        Widgets.DrawLineHorizontal(rect.x + 3, rect.y + LabelHeight, rect.width - 16);

        try
        {
            var icons = menuController.GenerateIcons();
            var headerHeight = LabelHeight + LineHeight;
            Text.Font = GameFont.Small;
            Rect viewRect = new(0, 0, rect.width - 16, icons.Sum(icon => RowHeight(icon, rect.width - 16)));
            Widgets.BeginScrollView(rect.Clone(addX: 3, addY: headerHeight, addHeight: -2 * headerHeight), ref ScrollPosition, viewRect);
            DrawIcons(viewRect, icons);
            Widgets.EndScrollView();
        }
        catch (Exception ex)
        {
            Log.ErrorOnce(ex.ToString(), ex.Message.GetHashCode());
        }
    }
}
