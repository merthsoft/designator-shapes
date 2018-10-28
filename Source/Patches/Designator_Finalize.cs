using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace Merthsoft.DesignatorShapes.Patches {
    [HarmonyPatch(typeof(Designator), "Finalize", new[] { typeof(bool) })]
    class Designator_Finalize {
        public static void Postfix(bool somethingSucceeded) {
            HistoryManager.FinishBuilding();
        }
    }
}
