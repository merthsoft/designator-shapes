using System;
using UnityEngine;

namespace Merthsoft.DesignatorShapes
{
    internal struct ActionIcon
    {
        public Texture2D icon;
        public Action action;
        public Color? highlightColor;
        public string label;
        public string description;
    }
}
