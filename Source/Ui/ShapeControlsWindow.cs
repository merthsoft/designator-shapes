using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes.Ui;

public class ShapeControlsWindow
{
    private readonly ShapeMenuDriver controller = new();

    public Rect WindowRect { get; set; }

    public Rect VisibleRect => new(WindowRect.x, WindowRect.y, Width,
        DesignatorShapes.ShowControls ? PanelHeight : LabelHeight);

    public bool IsHorizontal { get; set; }

    private const int ID = 53187649;

    public static int IconSize { get; set; }

    public static int NumButtons => 8;

    public static int NumRows => 2;

    public static float LabelHeight => Text.LineHeight;

    public static float LineHeight => 3;

    public static int CollapseButtonSize => 20;

    private bool dragging;
    private Vector2 dragOriginMouse, dragOriginWindow;

    // Called before the window stack consumes input, even when the pointer leaves the panel.
    internal void HandleDragInput(bool visible)
    {
        var e = Event.current;
        if (!visible || DesignatorShapes.Settings.LockPanelInPlace
            || Find.WindowStack.Windows.Any(w => w.absorbInputAroundWindow && w is not MainTabWindow))
        {
            StopDragging();
            return;
        }
        if (e == null) return;
        if (dragging)
        {
            // Window callbacks can consume MouseUp or use window-local event coordinates.
            // Use the absolute UI cursor and button state, including on Repaint, instead.
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                StopDragging();
                return;
            }
            var position = dragOriginWindow + UI.MousePositionOnUIInverted - dragOriginMouse;
            WindowRect = new Rect(position.x, position.y, Width, PanelHeight);
            ClampToScreen();
            SyncInputRect();
            if (!Input.GetMouseButton(0) || (e.rawType == EventType.MouseUp && e.button == 0))
                StopDragging();
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                StopDragging();
                e.Use();
            }
            else if (e.isMouse) e.Use();
            return;
        }
    }

    private void BeginDragging(Vector2 mouse)
    {
        dragging = true;
        dragOriginMouse = mouse;
        dragOriginWindow = WindowRect.position;
        GUI.FocusControl(null);
        GUIUtility.keyboardControl = 0;
    }

    private void StopDragging()
    {
        if (!dragging) return;
        dragging = false;
        DesignatorShapes.Settings.WindowX = (int)WindowRect.x;
        DesignatorShapes.Settings.WindowY = (int)WindowRect.y;
        DesignatorShapes.Settings.Write();
    }

    private void SyncInputRect()
    {
        // ImmediateWindow normally updates its rectangle only on Repaint.
        foreach (var window in Find.WindowStack.Windows.Where(w => w.ID == -ID))
        {
            window.windowRect = VisibleRect;
            window.absorbInputAroundWindow = false;
        }
    }
    private int visibleRows = NumRows;
    private bool ShowSizeInputs => DesignatorShapes.CurrentTool?.Group == controller.SelectedGroup
        && DesignatorShapes.CurrentTool?.ShowSizeInputs == true;
    private float SizeInputHeight => Width < 144 ? 56 : 28;
    private float PanelHeight => Math.Max(ShowSizeInputs ? 1 : NumRows, visibleRows) * IconSize
        + LabelHeight + LineHeight * 2 + (ShowSizeInputs ? SizeInputHeight + 6 : 0);

    public static float Width => NumButtons / NumRows * IconSize;
    public static float Height => NumRows * IconSize + LabelHeight + LineHeight * 2;

    // Share the existing input widget, keeping checker tiles independent of grid/fixed sizes.
    private bool EditingCheckerboard => (DesignatorShapes.ShowControls
        ? DesignatorShapes.CurrentTool : PaletteShapeWorker.SelectedShape)?.overlayGroup == "Checkerboards";
    private int gridWidth = 6, gridHeight = 6;
    private int checkerWidth = 1, checkerHeight = 1;
    private ref int inputWidth => ref (EditingCheckerboard ? ref checkerWidth : ref gridWidth);
    private ref int inputHeight => ref (EditingCheckerboard ? ref checkerHeight : ref gridHeight);
    public static IntVec3 CheckerTileSize => new(DesignatorShapes.ShapeControls.checkerWidth, 0,
        DesignatorShapes.ShapeControls.checkerHeight);
    private bool editingInput;
    private string widthBuffer = "6", heightBuffer = "6";
    private int lastWidth = 6, lastHeight = 6;

    internal void DrawNativeSizeInputs(Rect rect) => DrawSizeInputs(rect, "DSNativeSize");
    private void DrawSizeInputs(Rect rect, string prefix)
    {
        if (inputWidth != lastWidth) widthBuffer = inputWidth.ToString();
        if (inputHeight != lastHeight) heightBuffer = inputHeight.ToString();
        if (pendingSizeFocus != null)
        {
            GUI.FocusControl(pendingSizeFocus);
            pendingSizeFocus = null;
        }
        bool stacked = rect.width < 112;
        float fieldWidth = stacked ? rect.width : (rect.width - 6) / 2;
        var left = new Rect(rect.x, rect.y, fieldWidth, 24);
        var right = new Rect(stacked ? rect.x : rect.x + fieldWidth + 6, stacked ? rect.y + 28 : rect.y, fieldWidth, 24);
        DrawSizeField(left, "Merthsoft_DesignatorShapes_SizeInputWidthLabel".Translate(), prefix + "Width", ref inputWidth, ref widthBuffer);
        DrawSizeField(right, "Merthsoft_DesignatorShapes_SizeInputHeightLabel".Translate(), prefix + "Height", ref inputHeight, ref heightBuffer);
        lastWidth = inputWidth; lastHeight = inputHeight;
        var focused = GUI.GetNameOfFocusedControl();
        if (focused == prefix + "Width" || focused == prefix + "Height")
        { sizeFocus = focused; sizeKeyboardControl = GUIUtility.keyboardControl; }
    }

    private static void DrawSizeField(Rect rect, string label, string name, ref int value, ref string buffer)
    {
        Widgets.Label(new Rect(rect.x, rect.y, 20, rect.height), label + ":");
        GUI.SetNextControlName(name);
        var next = Widgets.TextField(new Rect(rect.x + 20, rect.y, rect.width - 20, rect.height), buffer);
        if (next.Length <= 3 && next.All(char.IsDigit))
        {
            buffer = next;
            if (int.TryParse(next, out var parsed) && parsed >= 1) value = Math.Min(parsed, 512);
        }
    }
    private string sizeFocus, pendingSizeFocus;
    private int sizeKeyboardControl;
    internal void HandleKeyboardInput()
    {
        var e = Event.current;
        if (e?.type != EventType.KeyDown || e.keyCode != KeyCode.Tab
            || sizeKeyboardControl == 0 || GUIUtility.keyboardControl != sizeKeyboardControl
            || string.IsNullOrEmpty(sizeFocus)) return;
        if (sizeFocus.StartsWith("DSCustom") && (!DesignatorShapes.ShowControls || !ShowSizeInputs || !editingInput)) return;
        pendingSizeFocus = sizeFocus.EndsWith("Width")
            ? sizeFocus[..^5] + "Height" : sizeFocus[..^6] + "Width";
        e.Use();
    }

    private void ClampToScreen()
    {
        float height = DesignatorShapes.ShowControls ? PanelHeight : LabelHeight;
        WindowRect = new Rect(Mathf.Clamp(WindowRect.x, 0, Math.Max(0, UI.screenWidth - Width)),
            Mathf.Clamp(WindowRect.y, 0, Math.Max(0, UI.screenHeight - height)), Width, PanelHeight);
    }
    public static IntVec3 InputVec => new(DesignatorShapes.ShapeControls.inputWidth, 0, DesignatorShapes.ShapeControls.inputHeight);

    public ShapeControlsWindow(int x, int y, int iconSize, bool dictionaryMode = false)
    {
        IsHorizontal = true;
        IconSize = iconSize;
        WindowRect = new(x, y, Width, PanelHeight);
    }

    public void ShapeControlsOnGUI()
    {
        visibleRows = Math.Max(1, (controller.GenerateIcons().Count + 3) / 4);
        WindowRect = new Rect(WindowRect.x, WindowRect.y, Width, PanelHeight);
        if (Find.MainTabsRoot.OpenTab == null && DesignatorShapes.Settings.HideWhenNoOpenTab)
            return;

        if (WindowRect.x == -1)
        {
            var infoRect = ArchitectCategoryTab.InfoRect;
            WindowRect = new Rect(infoRect.x, infoRect.y - 110, Width, PanelHeight);
        }

        ClampToScreen();
        SyncInputRect();
        Find.WindowStack.ImmediateWindow(ID, VisibleRect, WindowLayer.SubSuper, DoWindow, false, false, 0);
    }

    private void DrawIcon(Rect rect, ActionIcon icon, string tooltip)
    {
        if (Widgets.ButtonImage(rect, icon.icon, Color.white, icon.highlightColor ?? GenUI.MouseoverColor, tooltip: tooltip))
            icon.action?.Invoke();
    }

    private void DrawIcons(Rect rect, List<ActionIcon> icons)
    {
        var x = 0;
        var y = 0;
        foreach (var icon in icons)
        {
            DrawIcon(new Rect(x * IconSize + rect.x, y * IconSize + rect.y, IconSize, IconSize), icon, icon.label);

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
        // Waylith dispatches clicks to the registered GUI window before the root pass.
        // Start capture here; continue/release it globally when the pointer leaves.
        var e = Event.current;
        if (!DesignatorShapes.Settings.LockPanelInPlace && e.type == EventType.MouseDown && e.button == 0
            && new Rect(0, 0, Width - CollapseButtonSize - 4, LabelHeight).Contains(e.mousePosition))
        {
            BeginDragging(UI.MousePositionOnUIInverted);
            e.Use();
        }
        if (e.type == EventType.KeyDown && Find.CurrentMap != null)
            ShapeShortcuts.Handle(Find.DesignatorManager);
        var rect = new Rect(0, 0, Width, VisibleRect.height);
        if (DesignatorShapes.Settings.DrawBackground)
            Widgets.DrawWindowBackground(rect.Clone(height: DesignatorShapes.ShowControls ? PanelHeight : LabelHeight));

        Widgets.Label(rect.Clone(addX: 3), " " + "Merthsoft_DesignatorShapes_Shapes".Translate());

        if (DesignatorShapes.Settings.ToggleableInterface)
        {
            var buttonRect = rect.Clone(addX: Width - CollapseButtonSize - 2, addY: 2, width: CollapseButtonSize, height: CollapseButtonSize);
            var buttonLabel = DesignatorShapes.ShowControls ? "-" : "+";
            if (Widgets.ButtonText(buttonRect, buttonLabel, drawBackground: false))
            {
                GUI.FocusControl(null); GUIUtility.keyboardControl = 0;
                DesignatorShapes.ShowControls = !DesignatorShapes.ShowControls;
                SyncInputRect();
            }
        }

        if (DesignatorShapes.ShowControls)
        {
            Widgets.DrawLineHorizontal(rect.x + 3, rect.y + LabelHeight, rect.width - 6);
            rect.y += LabelHeight + LineHeight;
            rect.height -= LabelHeight;
            DrawIcons(rect, controller.GenerateIcons());

            if (ShowSizeInputs)
            {
                var sizeRect = new Rect(rect.x + 5, rect.y + visibleRows * IconSize + 6, Width - 10, SizeInputHeight);
                if (Widgets.ButtonText(new Rect(sizeRect.x, sizeRect.y, 20, 24), editingInput ? "-" : "+"))
                {
                    editingInput = !editingInput;
                    if (!editingInput) { GUI.FocusControl(null); GUIUtility.keyboardControl = 0; }
                }
                var fields = new Rect(sizeRect.x + 24, sizeRect.y, sizeRect.width - 24, sizeRect.height);
                if (editingInput) DrawSizeInputs(fields, "DSCustomSize");
                else
                {
                    bool stacked = fields.width < 112;
                    float width = stacked ? fields.width : (fields.width - 6) / 2;
                    Widgets.Label(new Rect(fields.x, fields.y, width, 24), "Merthsoft_DesignatorShapes_SizeInputWidthLabel".Translate() + $": {inputWidth}");
                    Widgets.Label(new Rect(stacked ? fields.x : fields.x + width + 6, stacked ? fields.y + 28 : fields.y, width, 24),
                        "Merthsoft_DesignatorShapes_SizeInputHeightLabel".Translate() + $": {inputHeight}");
                }
            }
        }

    }
}
