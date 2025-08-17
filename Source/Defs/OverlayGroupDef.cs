using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes.Defs;

public class OverlayGroupDef : Def
{
    public string uiIconPath;
    [Unsaved] public Texture2D UiIcon;

    public string closeUiIconPath;
    [Unsaved] public Texture2D CloseUiIcon;

    public string parentGroupName;
    [Unsaved] public OverlayGroupDef ParentGroup;

    public bool autoSelect = true;

    [Unsaved] public List<DesignatorShapeDef> Shapes;
    [Unsaved] public List<OverlayGroupDef> ChildrenGroups = [];
    public AutoSelectGroupMode SelectMode
        => autoSelect
            ? (Shapes?.Count ?? 0) == 1 ? AutoSelectGroupMode.AutoSelect : AutoSelectGroupMode.DoNotAutoSelect
            : AutoSelectGroupMode.DoNotAutoSelect;

    public DesignatorShapeDef FirstShape => Shapes?.FirstOrDefault() ?? null;

    public override void PostLoad()
    {
        base.PostLoad();
        LongEventHandler.ExecuteWhenFinished(delegate
        {
            UiIcon = Icons.GetIcon(uiIconPath);

            if (closeUiIconPath != null)
                CloseUiIcon = Icons.GetIcon(closeUiIconPath);

            Shapes = DefDatabase<DesignatorShapeDef>.AllDefsListForReading.Where(s => s.overlayGroup == defName).ToList();
            Shapes.ForEach(s =>
            {
                s.Group = this;
                s.RootGroup = ParentGroup ?? this;
            });

            if (parentGroupName != null)
            {
                var parent = DefDatabase<OverlayGroupDef>.GetNamed(parentGroupName, false);
                parent.ChildrenGroups.Add(this);

                ParentGroup = parent;
            }
        });
    }
}
