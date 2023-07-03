using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes.Dialogs;
internal class ShapeDictionary : Window
{
    public static float LabelHeight => Text.LineHeight;
    public static float IconSize => Text.LineHeight * 2 + LineHeight;
    public static float LineHeight => 3;

    private const int IconPadding = 16;

    private ShapeMenuController menuController = new() { DictionaryMode = true };
    private Vector2 ScrollPosition;

    private void DrawIcon(ActionIcon icon, Rect rect)
    {
        Widgets.Label(rect.Clone(addX: IconSize + IconPadding, addY: LineHeight), icon.label);
        Widgets.Label(rect.Clone(addX: IconSize + IconPadding, addY: LineHeight + LabelHeight), icon.description);

        if (Widgets.ButtonImage(rect.Clone(width: IconSize, height: IconSize), icon.icon, Color.white, icon.highlightColor ?? GenUI.MouseoverColor))
            icon.action?.Invoke();
    }

    private void DrawIcons(Rect rect, List<ActionIcon> icons)
    {
        for (int i = 0; i < icons.Count; i++)
            DrawIcon(icons[i], new Rect(rect.x, i * (IconSize + IconPadding) + rect.y, rect.width, rect.height));
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
            Rect viewRect = new(0, 0, rect.width - 16, icons.Count * (IconSize + IconPadding));
            Widgets.BeginScrollView(rect.Clone(addX: 3, addY: LabelHeight + LineHeight, addHeight: -2*(LabelHeight + LineHeight)), ref ScrollPosition, viewRect);
            Text.Font = GameFont.Small;
            DrawIcons(viewRect, icons);
            Widgets.EndScrollView();
        } catch (Exception ex)
        {
            Log.ErrorOnce(ex.ToString(), ex.Message.GetHashCode());
        }
    }
}
