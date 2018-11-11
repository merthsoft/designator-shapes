using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
