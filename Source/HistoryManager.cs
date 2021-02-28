using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Merthsoft.DesignatorShapes
{
    public class HistoryManager
    {
        public class Entry
        {
            public List<Designation> Designations = new();
            public List<Blueprint> Blueprints = new();
        }

        private static readonly Dictionary<Map, HistoryManager> Histories = new();

        private readonly DesignationManager DesignationManager;
        private bool Building = false;
        private static bool InRedo = false;

        private readonly Stack<Entry> UndoStack = new();
        private readonly Stack<Entry> RedoStack = new();

        public HistoryManager(DesignationManager designationManager)
            => DesignationManager = designationManager;

        public static void Clear() => GetManager()?.InternalClear();

        private void InternalClear()
        {
            UndoStack.Clear();
            RedoStack.Clear();
        }

        public static void AddEntry(Designation des) => AddEntry(des, null);

        public static void AddEntry(Blueprint bp) => AddEntry(null, bp);

        public static void AddEntry(Designation des, Blueprint bp)
        {
            if (InRedo)
                return;
            GetManager()?.InteralAddEntry(des, bp);
        }

        private void InteralAddEntry(Designation des, Blueprint bp)
        {
            if (!Find.TickManager.Paused)
                return;
            if (!Building)
                StartBuilding();
            if (UndoStack.Count == 0)
            {
                Log.Message("Somehow the undo stack is empty even though we've started building...");
                return;
            }

            if (des != null)
                UndoStack.Peek().Designations.Add(des);
            if (bp != null)
                UndoStack.Peek().Blueprints.Add(bp);
        }

        public static void StartBuilding() => GetManager().InteralStartBuilding();

        private void InteralStartBuilding()
        {
            if (!Find.TickManager.Paused)
                return;
            if (Building)
                return;
            RedoStack.Clear();
            UndoStack.Push(new Entry());
            Building = true;
        }

        public static void FinishBuilding() 
            => GetManager()?.InternalFinishBuilding();

        public void InternalFinishBuilding() 
            => Building = false;

        public static void Redo() 
            => GetManager().InteralRedo();

        public static void Undo() 
            => GetManager().InternalUndo();

        public static bool CanUndo => GetManager().UndoStack.Any();

        public static bool CanRedo => GetManager().RedoStack.Any();

        private static HistoryManager GetManager()
        {
            var map = Find.CurrentMap;
            if (map == null)
                return null;
            if (!Histories.ContainsKey(map))
                Histories[map] = new HistoryManager(map.designationManager);

            return Histories[map];
        }

        private void InternalUndo()
        {
            if (UndoStack.Count == 0)
                return;
            var entry = UndoStack.Pop();
            foreach (var designation in entry.Designations)
                designation.Delete();
            foreach (var blueprint in entry.Blueprints)
                if (blueprint.Spawned)
                    blueprint.DeSpawn();
            RedoStack.Push(entry);
        }

        private void InteralRedo()
        {
            if (RedoStack.Count == 0)
                return;
            var entry = RedoStack.Pop();
            InRedo = true;
            foreach (var des in entry.Designations)
                DesignationManager.AddDesignation(des);
            var map = Find.CurrentMap;
            var generatedBlueprints = new List<Blueprint>();
            foreach (var blueprint in entry.Blueprints)
                if (!blueprint.Spawned)
                    if (blueprint is Blueprint_Build build)
                    {
                        var blueprint_Build = (Blueprint_Build)ThingMaker.MakeThing(build.def);
                        blueprint_Build.SetFactionDirect(Faction.OfPlayer);
                        blueprint_Build.stuffToUse = build.stuffToUse;
                        GenSpawn.Spawn(blueprint_Build, build.Position, map, build.Rotation);
                        generatedBlueprints.Add(blueprint_Build);
                    }
            entry.Blueprints = generatedBlueprints;
            InRedo = false;
            UndoStack.Push(entry);
        }
    }
}
