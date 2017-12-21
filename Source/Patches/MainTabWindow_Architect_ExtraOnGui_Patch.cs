using System;
namespace Merthsoft.DesignatorShapes.Patches {
    public static class MainTabWindow_Architect_ExtraOnGui_Patch {
        public static void Prefix() {
            DesignatorShapes.LoadDefs();
        }
    }
}
