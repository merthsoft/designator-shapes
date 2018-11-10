using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using UnityEngine;
using Verse;

namespace Merthsoft.DesignatorShapes.Defs {
    public class DesignatorShapeDef : BuildableDef {
        public string drawMethodName;
        public bool draggable;
        public int numRotations;
        public string overlayGroup;
        public string selectedUiIconPath;

        [Unsaved] public OverlayGroupDef Group;

        [Unsaved] public Texture2D selectedUiIcon;

        [Unsaved]
        private Func<IntVec3, IntVec3, int, IEnumerable<IntVec3>> drawMethodCached;
        public Func<IntVec3, IntVec3, int, IEnumerable<IntVec3>>  drawMethod {
            get {
                if (drawMethodCached == null) {
                    var splitName = drawMethodName.Split('.');
                    var methodName = splitName[splitName.Length - 1];
                    var typeName = string.Join(".", splitName.ToList().Take(splitName.Length - 1).ToArray());

                    var type = this.GetType().Assembly.GetType(typeName);
                    if (type == null) {
                        throw new Exception($"Could not load type {typeName}");
                    }

                    var method = type.GetMethod(methodName, new Type[] {typeof(IntVec3), typeof(IntVec3), typeof(int)});
                    if (method == null) {
                        throw new Exception($"Could not find {methodName} in {typeName}");
                    }

                    if (!(method.ReturnType == typeof(IEnumerable<IntVec3>))) {
                        throw new Exception($"Return type for {methodName} is not IEnumberable<IntVec3>");
                    }

                    drawMethodCached = (arg1, arg2, arg3) => method.Invoke(null, new object[] {arg1, arg2, arg3}) as IEnumerable<IntVec3>;
                }
                return drawMethodCached;
            }
        }

        public DesignatorShapeDef() {
            draggable = true;
            numRotations = 0;
        }

        public override void ResolveReferences() {
            base.ResolveReferences();

            LongEventHandler.ExecuteWhenFinished(() => {
                selectedUiIcon = DesignatorShapes.GetIcon(selectedUiIconPath);
            });
        }

        public void Select() {
            DesignatorShapes.SelectTool(this);
        }
    }
}