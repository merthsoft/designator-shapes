using Merthsoft.DesignatorShapes.Defs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes;
internal class ShapeMenuDriver
{
    private List<OverlayGroupDef> GroupDefs => DefDatabase<OverlayGroupDef>.AllDefsListForReading;
    public OverlayGroupDef SelectedGroup { get; set; }

    private readonly Stack<OverlayGroupDef> previousGroups = new();

    public bool DictionaryMode { get; set; } = false;

    public ShapeMenuDriver() { }

    public List<ActionIcon> GenerateIcons()
    {
        List<ActionIcon> icons;
        if (SelectedGroup == null)
        {
            icons = GroupDefs.Where(g => g.ParentGroup == null).SelectList(CreateActionIcon);
            icons.Add(new ActionIcon
            {
                label = DesignatorShapeDefOf.Undo.LabelCap,
                description = DesignatorShapeDefOf.Undo.description,
                icon = DictionaryMode || HistoryManager.CanUndo ? Icons.UndoEnabled : Icons.UndoDisabled,
                action = DictionaryMode ? null : HistoryManager.Undo,
                highlightColor = DictionaryMode || HistoryManager.CanUndo ? GenUI.MouseoverColor : Color.grey
            });
            icons.Add(new ActionIcon
            {
                label = DesignatorShapeDefOf.Redo.LabelCap,
                description = DesignatorShapeDefOf.Redo.description,
                icon = DictionaryMode || HistoryManager.CanRedo ? Icons.RedoEnabled : Icons.RedoDisabled,
                action = DictionaryMode ? null : HistoryManager.Redo,
                highlightColor = DictionaryMode || HistoryManager.CanRedo ? GenUI.MouseoverColor : Color.grey
            });
        }
        else
        {
            icons = new List<ActionIcon> { new ActionIcon {
                    label = "Back".Translate(),
                    action = () => SelectedGroup = previousGroups.Pop(),
                    icon = SelectedGroup.CloseUiIcon,
                } };
            icons.AddRange(SelectedGroup.Shapes.Select(s => new ActionIcon
            {
                label = s.LabelCap,
                description = s.description,
                icon = DictionaryMode || DesignatorShapes.CurrentTool?.defName != s.defName
                        ? s.uiIcon
                        : s.selectedUiIcon,
                action = DictionaryMode ? null : () => DesignatorShapes.SelectTool(s),
            }));
            icons.AddRange(SelectedGroup.ChildrenGroups.SelectList(CreateActionIcon));
        }

        return icons;
    }

    private ActionIcon CreateActionIcon(OverlayGroupDef g) => new()
    {
        label = g.LabelCap,
        description = g.description,
        icon = DictionaryMode || DesignatorShapes.CurrentTool?.Group?.defName != g.defName
                ? g.UiIcon
                : DesignatorShapes.CurrentTool?.selectedUiIcon,
        action = GetAction(g)
    };

    private Action GetAction(OverlayGroupDef g)
        => g.NumShapes switch
        {
            1 => DictionaryMode ? null : () => DesignatorShapes.SelectTool(g.FirstShape),
            _ => () =>
            {
                previousGroups.Push(SelectedGroup);
                SelectedGroup = g;
            }
        };
}
