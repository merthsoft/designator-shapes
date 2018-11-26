using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Merthsoft.DesignatorShapes.Defs;
using RimWorld;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes {
    public class ShapeControls {
        private List<OverlayGroupDef> groupDefs => DefDatabase<OverlayGroupDef>.AllDefsListForReading;
        private List<DesignatorShapeDef> shapeDefs => DefDatabase<DesignatorShapeDef>.AllDefsListForReading;

        public Rect WindowRect { get; set; }
        public Rect DraggingWindowRect => new Rect(WindowRect.x - 100, WindowRect.y - 100, WindowRect.width + 200, WindowRect.height + 200);

        public bool IsHorizontal { get; set; }

        public OverlayGroupDef SelectedGroup { get; set; }

        private const int ID = 12341234;

        public static int IconSize => DesignatorShapes.Settings.IconSize;
        public static int NumButtons = 8;
        public static int NumRows = 2;
        public static int LabelHeight = 20;

        private bool dragging;

        public static int Width => NumButtons / NumRows * IconSize;
        public static int Height => NumRows * IconSize + LabelHeight;

        public ShapeControls(int x, int y) {
            IsHorizontal = true;

            WindowRect = new Rect(x, y, Width, Height);
        }

        public void ShapeControlsOnGUI() {
            if (Find.MainTabsRoot.OpenTab == null) { return; }

            if (WindowRect.x == -1) {
                Rect infoRect = ArchitectCategoryTab.InfoRect;
                WindowRect = new Rect(infoRect.x, infoRect.y - 110, Width, Height);
            }

            Find.WindowStack.ImmediateWindow(ID, dragging ? DraggingWindowRect : WindowRect, WindowLayer.GameUI, DoWindow, false, dragging, 0);
        }
        
        private void drawIcon(Rect rect, Texture2D icon, Color? highlighColor, Action action) {
            if (Widgets.ButtonImage(rect, icon, Color.white, highlighColor ?? GenUI.MouseoverColor)) {
                action.Invoke();
            }
        }

        private class actionicon {
            public Texture2D icon;
            public Action action;
            public Color? highlightColor;
        }

        private void drawIcons(Rect rect, List<actionicon> icons) {
            var x = 0;
            var y = 0;
            foreach (var icon in icons) {
                drawIcon(new Rect(x * IconSize + rect.x, y * IconSize + rect.y, IconSize, IconSize), icon.icon, icon.highlightColor, icon.action);

                x++;
                if (x == NumButtons / NumRows) {
                    x = 0;
                    y++;
                }
            }
        }

        private void DoWindow() {
            var offset = dragging ? 100 : 0;

            if (DesignatorShapes.Settings.DrawBackground) {
                Widgets.DrawWindowBackground(new Rect(offset, offset, Width, Height));
            }

            Widgets.Label(new Rect(offset, offset, Width, LabelHeight), "Shapes");
            Widgets.DrawLineHorizontal(offset + 3, offset + LabelHeight, Width - 6);
            drawIcons(new Rect(offset, offset + LabelHeight + 3, Width, Height - LabelHeight), generateIcons());


            if (Event.current.isMouse) {
                switch (Event.current.type) {
                    case EventType.MouseDown:
                        dragging = true;
                        Event.current.Use();
                        break;
                    case EventType.MouseDrag:
                        WindowRect = new Rect(WindowRect.x + Event.current.delta.x, WindowRect.y + Event.current.delta.y, Width, Height);
                        Event.current.Use();
                        break;
                    case EventType.MouseUp:
                        dragging = false;
                        DesignatorShapes.Settings.WindowX = (int)WindowRect.x;
                        DesignatorShapes.Settings.WindowY = (int)WindowRect.y;
                        DesignatorShapes.Settings.Write();
                        Event.current.Use();
                        break;
                }
            }
        }

        private List<actionicon> generateIcons() {
            List<actionicon> icons;
            if (SelectedGroup == null) {
                icons = groupDefs.SelectList(g => new actionicon {
                    icon = DesignatorShapes.CurrentTool?.Group.defName == g.defName
                            ? DesignatorShapes.CurrentTool.selectedUiIcon
                            : g.uiIcon,
                    action = GetAction(g)

                });
                icons.Add(new actionicon {
                    icon = HistoryManager.CanUndo ? DesignatorShapes.Icon_UndoEnabled : DesignatorShapes.Icon_UndoDisabled,
                    action = HistoryManager.Undo,
                    highlightColor = HistoryManager.CanUndo ? GenUI.MouseoverColor : Color.grey
                });
                icons.Add(new actionicon {
                    icon = HistoryManager.CanRedo ? DesignatorShapes.Icon_RedoEnabled : DesignatorShapes.Icon_RedoDisabled,
                    action = HistoryManager.Redo,
                    highlightColor = HistoryManager.CanRedo ? GenUI.MouseoverColor : Color.grey
                });
            } else {
                icons = new List<actionicon> { new actionicon {
                    action = () => SelectedGroup = null,
                    icon = SelectedGroup.closeUiIcon
                } };
                icons.AddRange(SelectedGroup.Shapes.Select(s => new actionicon {
                    icon = DesignatorShapes.CurrentTool.defName == s.defName ? s.selectedUiIcon : s.uiIcon,
                    action = () => DesignatorShapes.SelectTool(s)
                }));
            }

            return icons;
        }

        Action GetAction(OverlayGroupDef g) {
            if (g.NumShapes == 1) {
                return () => DesignatorShapes.SelectTool(g.FirstShape);
            } else {
                if (DesignatorShapes.Settings.UseSubMenus) {
                    return () => SelectedGroup = g;
                } else {
                    return () => Find.WindowStack.Add(new FloatMenu(g.Shapes.SelectList(s =>
                        new FloatMenuOption(s.LabelCap, s.Select)
                    )));
                }
            }
        }
    }
}
