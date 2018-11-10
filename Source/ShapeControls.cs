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

        public bool IsHorizontal { get; set; }

        public OverlayGroupDef SelectedGroup { get; set; }

        private const int icon_size = 40;
        public static int NumButtons = 8;
        public static int NumRows = 2;

        public ShapeControls() {
            IsHorizontal = true;

            WindowRect = new Rect(0, 0, NumButtons / NumRows * icon_size, NumRows * icon_size);
        }

        public void ShapeControlsOnGUI() {
            if (Find.MainTabsRoot.OpenTab == null) { return; }

            Rect infoRect = ArchitectCategoryTab.InfoRect;
            WindowRect = new Rect(infoRect.x, infoRect.y - 90, WindowRect.width, WindowRect.height);
            Find.WindowStack.ImmediateWindow(12341234, WindowRect, WindowLayer.GameUI, DoWindow, true);
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

        private void drawIcons(List<actionicon> icons) {
            var x = 0;
            var y = 0;
            foreach (var icon in icons) {
                drawIcon(new Rect(x * 40, y * 40, icon_size, icon_size), icon.icon, icon.highlightColor, icon.action);

                x++;
                if (x == NumButtons / NumRows) {
                    x = 0;
                    y++;
                }
            }
        }

        private void DoWindow() {
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

            drawIcons(icons);
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
