using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes.Ui;

public class ShapeControlsWindow
{
    private readonly ShapeMenuDriver controller = new();

    public Rect WindowRect { get; set; }

    public Rect DraggingWindowRect => new(WindowRect.x - 100, WindowRect.y - 100, WindowRect.width + 200, WindowRect.height + 200);

    public bool IsHorizontal { get; set; }

    private const int ID = 53187649;

    public static int IconSize { get; set; }

    public static int NumButtons => 8;

    public static int NumRows => 2;

    public static float LabelHeight => Text.LineHeight;

    public static float LineHeight => 3;

    public static int CollapseButtonSize => 15;

    private bool dragging;

    public static float Width => NumButtons / NumRows * IconSize;

    public static float Height => NumRows * IconSize + LabelHeight + LineHeight*2;

    private int inputWidth;
    private int inputHeight;

    public static IntVec3 InputVec => new(DesignatorShapes.ShapeControls.inputWidth, 0, DesignatorShapes.ShapeControls.inputHeight);

    public ShapeControlsWindow(int x, int y, int iconSize, bool dictionaryMode = false)
    {
        IsHorizontal = true;
        WindowRect = new(x, y, Width, Height);
        IconSize = iconSize;
    }

    public void ShapeControlsOnGUI()
    {
        if (Find.MainTabsRoot.OpenTab == null && DesignatorShapes.Settings.HideWhenNoOpenTab)
            return;

        if (WindowRect.x == -1)
        {
            var infoRect = ArchitectCategoryTab.InfoRect;
            WindowRect = new Rect(infoRect.x, infoRect.y - 110, Width, Height);
        }

        Find.WindowStack.ImmediateWindow(ID, dragging ? DraggingWindowRect : WindowRect, WindowLayer.SubSuper, DoWindow, false, dragging, 0);
    }

    private void DrawIcon(Rect rect, ActionIcon icon)
    {
        if (Widgets.ButtonImage(rect, icon.icon, Color.white, icon.highlightColor ?? GenUI.MouseoverColor))
            icon.action?.Invoke();
    }

    private void DrawIcons(Rect rect, List<ActionIcon> icons)
    {
        var x = 0;
        var y = 0;
        foreach (var icon in icons)
        {
            DrawIcon(new Rect(x * IconSize + rect.x, y * IconSize + rect.y, IconSize, IconSize), icon);

            x++;
            if (x == NumButtons / NumRows)
            {
                x = 0;
                y++;
            }

        }
    }

    private void DoWindow()
    {
        var offset = dragging ? 100 : 0;

        var rect = new Rect(offset, offset, Width, Height);
        if (DesignatorShapes.Settings.DrawBackground)
            Widgets.DrawWindowBackground(rect.Clone(height: DesignatorShapes.ShowControls ? Height : LabelHeight));

        Widgets.Label(rect.Clone(addX: 3), " " + "Merthsoft_DesignatorShapes_Shapes".Translate());

        if (DesignatorShapes.Settings.ToggleableInterface)
        {
            var buttonRect = rect.Clone(addX: Width - CollapseButtonSize - 2, addY: 2, width: CollapseButtonSize, height: CollapseButtonSize);
            var buttonLabel = DesignatorShapes.ShowControls ? "[-] " : "[+] ";
            if (Widgets.ButtonText(buttonRect, buttonLabel, drawBackground: false))
                DesignatorShapes.ShowControls = !DesignatorShapes.ShowControls;
        }

        if (DesignatorShapes.ShowControls)
        {
            Widgets.DrawLineHorizontal(rect.x + 3, rect.y + LabelHeight, rect.width - 6);
            rect.y += LabelHeight + LineHeight;
            rect.height -= LabelHeight;
            DrawIcons(rect, controller.GenerateIcons());

            if (DesignatorShapes.CurrentTool?.Group == controller.SelectedGroup
                && (DesignatorShapes.CurrentTool?.useSizeInputs ?? false))
            {
                var buffer = inputWidth.ToString();

                rect.y += IconSize + 10;
                rect.width /= 2;
                rect.width -= 25;
                rect.height = 20;

                Widgets.Label(rect, "Merthsoft_DesignatorShapes_SizeInputWidthLabel".Translate());
                rect.x += 20;
                rect.width -= 20;
                Widgets.TextFieldNumeric(rect, ref inputWidth, ref buffer);
                rect.x += rect.width + 25;
                Widgets.Label(rect, "Merthsoft_DesignatorShapes_SizeInputHeightLabel".Translate());
                rect.x += 20;
                buffer = inputHeight.ToString();
                Widgets.TextFieldNumeric(rect, ref inputHeight, ref buffer);

                if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Tab)
                {
                    GUI.FocusWindow(ID);
                    Event.current.Use();
                }
            }
        }

        if (Event.current.isMouse)
            switch (Event.current.type)
            {
                case EventType.MouseDown when !DesignatorShapes.Settings.LockPanelInPlace:
                    dragging = true;
                    Event.current.Use();
                    break;
                case EventType.MouseDrag when dragging:
                    WindowRect = new Rect(WindowRect.x + Event.current.delta.x, WindowRect.y + Event.current.delta.y, Width, Height);
                    Event.current.Use();
                    break;
                case EventType.MouseUp when dragging:
                    dragging = false;
                    DesignatorShapes.Settings.WindowX = (int)WindowRect.x;
                    DesignatorShapes.Settings.WindowY = (int)WindowRect.y;
                    DesignatorShapes.Settings.Write();
                    Event.current.Use();
                    break;
            }
    }
}
