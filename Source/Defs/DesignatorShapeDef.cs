using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes.Defs
{
    public class DesignatorShapeDef : Def
    {
        public string drawMethodName;
        public bool draggable = true;
        public int numRotations = 0;
        public string overlayGroup;
        public string selectedUiIconPath;
        public bool useSizeInputs = false;
        public bool pauseOnSelection = false;
        public string uiIconPath;

        [Unsaved] public Graphic graphic;
        [Unsaved] public Texture2D uiIcon;

        [Unsaved] public OverlayGroupDef Group;
        [Unsaved] public OverlayGroupDef RootGroup;
        [Unsaved] public Texture2D selectedUiIcon;

        public bool AllowDragging => draggable && !useSizeInputs;

        [Unsaved]
        private Func<IntVec3, IntVec3, IEnumerable<IntVec3>> drawMethodCached;

        public Func<IntVec3, IntVec3, IEnumerable<IntVec3>> DrawMethod
            => drawMethodCached ??= GenerateDrawMethod();

        private Func<IntVec3, IntVec3, IEnumerable<IntVec3>> GenerateDrawMethod()
        {
            var splitName = drawMethodName.Split('.');
            var methodName = splitName[splitName.Length - 1];
            var typeName = string.Join(".", splitName.ToList().Take(splitName.Length - 1).ToArray());

            var type = GetType().Assembly.GetType(typeName);
            if (type == null)
                throw new Exception($"Could not load type {typeName}");

            var method = type.GetMethod(methodName, new Type[] { typeof(IntVec3), typeof(IntVec3) });
            if (method == null)
                throw new Exception($"Could not find {methodName}(IntVec3, IntVec3) in {typeName}");

            if (!(method.ReturnType == typeof(IEnumerable<IntVec3>)))
                throw new Exception($"Return type for {methodName} is not IEnumerable<IntVec3>");

            return(arg1, arg2) => method.Invoke(null, new object[] { arg1, arg2 }) as IEnumerable<IntVec3>;
        }

        public void Select() => DesignatorShapes.SelectTool(this);

        public override void PostLoad()
        {
            base.PostLoad();
            LongEventHandler.ExecuteWhenFinished(delegate
            {
                uiIcon = Icons.GetIcon(uiIconPath);
                selectedUiIcon = Icons.GetIcon(selectedUiIconPath);
            });
        }
    }
}