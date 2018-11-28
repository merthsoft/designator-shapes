using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes.Defs {
    public class OverlayGroupDef : Def {
        public string uiIconPath;
        [Unsaved] public Texture2D UiIcon;

        public string closeUiIconPath;
        [Unsaved] public Texture2D CloseUiIcon;

        public string parentGroupName;
        [Unsaved] public OverlayGroupDef ParentGroup;

        [Unsaved] public List<DesignatorShapeDef> Shapes;
        [Unsaved] public List<OverlayGroupDef> ChildrenGroups = new List<OverlayGroupDef>();

        public int NumShapes => Shapes?.Count ?? 0;
        public DesignatorShapeDef FirstShape => Shapes?.FirstOrDefault() ?? null;
    }
}
