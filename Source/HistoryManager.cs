using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Merthsoft.DesignatorShapes {
    public class HistoryManager {
        public class Entry {
            public List<Designation> Designations = new List<Designation>();
            public Entry() {}
        }

        static Dictionary<Map, HistoryManager> histories = new Dictionary<Map, HistoryManager>();

        DesignationManager designationManager;
        int historyIndex = -1;
        List<Entry> history = new List<Entry>();
        private bool building = false;
        private static bool inRedo = false;

        public HistoryManager(DesignationManager designationManager) {
            this.designationManager = designationManager;
        }

        public static void AddEntry(Designation des) {
            if (inRedo) { return; }
            getManager().addEntry(des);
        }

        private void addEntry(Designation des) {
            if (!building) {
                StartBuilding();
            }
            history[historyIndex].Designations.Add(des);
        }

        public static void StartBuilding() {
            getManager().startBuilding();
        }

        private void startBuilding() {
            //
            // Start:
            // []
            // Count = 0, index = -1

            // [A]
            //  ^
            // Count = 1, index = 0

            // [A, B]
            //     ^
            // Count = 2, index = 1

            // ...

            // [A, B, C, D]
            //           ^
            // Count = 4, index = 3

            // [A, B, C, D]
            //        ^
            // Count = 4, index = 2

            // [A, B, C, D]
            //     ^
            // Count = 4, index = 1

            // [A, B, E]
            //        ^
            // Count = 3, index = 2
            if (history.Count > historyIndex + 1) {
                history.RemoveRange(historyIndex + 1, histories.Count - historyIndex - 1);
            }
            historyIndex++;
            history.Add(new Entry());
            building = true;
        }

        public static void FinishBuilding() {
            getManager().finishBuilding();
        }

        public void finishBuilding() {
            building = false;
        }

        public static void Redo() {
            getManager().redo();
        }

        public static void Undo() {
            getManager().undo();
        }

        private static HistoryManager getManager() {
            var map = Find.CurrentMap;
            if (!histories.ContainsKey(map)) {
                histories[map] = new HistoryManager(map.designationManager);
            }

            return histories[map];
        }

        private void undo() {
            if (historyIndex == -1) { return; }
            var entry = history[historyIndex];
            historyIndex--;
            foreach (var des in entry.Designations) {
                des.Delete();
            }
        }

        private void redo() {
            if (historyIndex == -1) { return; }
            if (history.Count == historyIndex + 1) { return; }

            historyIndex++;
            var entry = history[historyIndex];
            inRedo = true;
            foreach (var des in entry.Designations) {
                designationManager.AddDesignation(des);
            }
            inRedo = false;
        }
    }
}
