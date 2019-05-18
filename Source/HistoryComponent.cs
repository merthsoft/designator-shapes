using Verse;

namespace Merthsoft.DesignatorShapes {
    class HistoryComponent : GameComponent {
        public HistoryComponent(Game g) {

        }

        public override void GameComponentTick() {
            base.GameComponentTick();
            HistoryManager.Clear();

            if (DesignatorShapes.Settings.ResetShapeOnResume) {
                DesignatorShapes.CachedTool = null;
            }
        }

        public override void ExposeData() {
            base.ExposeData();

            var cachedTool = DesignatorShapes.CachedTool;
            Scribe_Defs.Look(ref cachedTool, nameof(DesignatorShapes.CachedTool));
            DesignatorShapes.CachedTool = cachedTool;
        }
    }
}
