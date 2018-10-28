using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Merthsoft.DesignatorShapes {
    public class HistoryManager {
        public class Entry {
            public List<Designation> Designations = new List<Designation>();
            public List<Blueprint> Blueprints = new List<Blueprint>();
        }

        static Dictionary<Map, HistoryManager> histories = new Dictionary<Map, HistoryManager>();

        DesignationManager designationManager;
        private bool building = false;
        private static bool inRedo = false;

        Stack<Entry> undoStack = new Stack<Entry>();
        Stack<Entry> redoStack = new Stack<Entry>();

        public HistoryManager(DesignationManager designationManager) {
            this.designationManager = designationManager;
        }

        public static void Clear() {
            getManager()?.clear();
        }

        private void clear() {
            undoStack.Clear();
            redoStack.Clear();
        }

        public static void AddEntry(Designation des) {
            AddEntry(des, null);
        }

        public static void AddEntry(Blueprint bp) {
            AddEntry(null, bp);
        }

        public static void AddEntry(Designation des, Blueprint bp) {
            if (inRedo) { return; }
            getManager()?.addEntry(des, bp);
        }

        private void addEntry(Designation des, Blueprint bp) {
            if (!Find.TickManager.Paused) { return; }

            if (!building) {
                StartBuilding();
            }
            if (undoStack.Count == 0) {
                Log.Message("Somehow the undo stack is empty even though we've started building...");
                return;
            }

            if (des != null) {
                undoStack.Peek().Designations.Add(des);
            }
            if (bp != null) {
                undoStack.Peek().Blueprints.Add(bp);
            }
        }

        public static void StartBuilding() {
            getManager().startBuilding();
        }

        private void startBuilding() {
            if (!Find.TickManager.Paused) { return; }
            if (building) { return; }

            redoStack.Clear();
            undoStack.Push(new Entry());
            building = true;
        }

        public static void FinishBuilding() {
            getManager()?.finishBuilding();
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
            if (map == null) { return null; }
            if (!histories.ContainsKey(map)) {
                histories[map] = new HistoryManager(map.designationManager);
            }

            return histories[map];
        }

        private void undo() {
            if (undoStack.Count == 0) { return; }
            var entry = undoStack.Pop();
            foreach (var designation in entry.Designations) {
                designation.Delete();
            }
            foreach (var blueprint in entry.Blueprints) {
                if (blueprint.Spawned) {
                    blueprint.DeSpawn();
                }
            }
            redoStack.Push(entry);
        }

        private void redo() {
            if (redoStack.Count == 0) { return; }

            var entry = redoStack.Pop();
            inRedo = true;
            foreach (var des in entry.Designations) {
                designationManager.AddDesignation(des);
            }
            var map = Find.CurrentMap;
            var generatedBlueprints = new List<Blueprint>();
            foreach (var blueprint in entry.Blueprints) {
                if (!blueprint.Spawned) {
                    var build = blueprint as Blueprint_Build;
                    if (build != null) {
                        var blueprint_Build = (Blueprint_Build)ThingMaker.MakeThing(build.def);
                        blueprint_Build.SetFactionDirect(Faction.OfPlayer);
                        blueprint_Build.stuffToUse = build.stuffToUse;
                        GenSpawn.Spawn(blueprint_Build, build.Position, map, build.Rotation);
                        generatedBlueprints.Add(blueprint_Build);
                    }
                }
            }
            entry.Blueprints = generatedBlueprints;
            inRedo = false;
            undoStack.Push(entry);
        }
    }
}
