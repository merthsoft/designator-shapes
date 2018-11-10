using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes.Defs {
    public class OverlayGroupDef : Def {
        public string uiIconPath;
        [Unsaved] public Texture2D uiIcon;

        public string closeUiIconPath;
        [Unsaved] public Texture2D closeUiIcon;

        public List<DesignatorShapeDef> Shapes { get; set; }
        public int NumShapes => Shapes?.Count ?? 0;
        public DesignatorShapeDef FirstShape => Shapes?.FirstOrDefault() ?? null;
    }
}
