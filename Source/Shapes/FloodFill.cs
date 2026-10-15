using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Merthsoft.DesignatorShapes.Shapes;

public static class FloodFill
{
    public static IEnumerable<IntVec3> Fill(IntVec3 s, IntVec3 t)
    {
        var ret = new HashSet<IntVec3>();
        var map = Find.CurrentMap;
        if (map == null || !t.InBounds(map))
            return ret;

        var restrictToRevealed = DesignatorShapes.Settings.RestrictFloodFillToRevealedCells;
        bool CanVisit(IntVec3 cell) => cell.InBounds(map) && (!restrictToRevealed || IsRevealedCell(map, cell));
        if (!CanVisit(t))
            return ret;

        var wallAtMouse = map.getWallDefAt(t);
        // AllDesignationsAt reuses a temporary list; retain only the seed's state.
        var hasDesignationsAtMouse = map.getDesignationsAt(t).Any();
        var mineableAtMouse = map.getMineableAt(t);
        var barrierDefs = DesignatorShapes.Settings.EnableCustomFloodFillBarriers
            ? new HashSet<string>(DesignatorShapes.Settings.AdditionalFloodFillBarrierDefs ?? new List<string>()) : new HashSet<string>();
        var fillDef = wallAtMouse ?? mineableAtMouse?.def;
        bool IsAdditionalBarrier(IntVec3 cell) => barrierDefs.Count>0 && map.thingGrid.ThingsListAtFast(cell).Any(thing=>
        {
            var def=thing.def.entityDefToBuild as ThingDef ?? thing.def;
            return def!=fillDef && barrierDefs.Contains(def.defName);
        });
        if(IsAdditionalBarrier(t))return ret;
        var floorAtMouse = map.getTerrainAt(t);
        var fertAtMouse = map.getFertilityAt(t);

        var cells = new Queue<IntVec3>();
        cells.Enqueue(t);
        void EnqueueIfAllowed(IntVec3 cell)
        {
            if (CanVisit(cell) && !IsAdditionalBarrier(cell))
                cells.Enqueue(cell);
        }

        while (cells.Count() > 0 && ret.Count() < DesignatorShapes.Settings.FloodFillCellLimit)
        {
            var cell = cells.Dequeue();
            if (!CanVisit(cell))
                continue;
            if (IsAdditionalBarrier(cell))
                continue;
            if (ret.Contains(cell))
                continue;
            if (!Find.DesignatorManager.SelectedDesignator.CanDesignateCell(cell).Accepted)
                continue;
            var cellWall = map.getWallDefAt(cell);
            var cellDes = map.getDesignationsAt(cell);
            var cellMineable = map.getMineableAt(cell);
            var cellFloor = map.getTerrainAt(cell);
            var cellFert = map.getFertilityAt(cell);
            var cellThings = map.thingGrid.ThingsListAtFast(cell);

            var addFlag = false;

            if (wallAtMouse != null)
            {
                if (cellWall == null)
                    continue;
                if (cellWall.defName == wallAtMouse.defName)
                    addFlag = true;
            }
            else if (mineableAtMouse != null)
            {
                if (cellMineable?.def == mineableAtMouse.def)
                    addFlag = true;
            }
            else if (hasDesignationsAtMouse)
            {
                if (cellWall == null && cellMineable == null && cellDes.Count() > 0)
                    addFlag = true;
            }
            else if (cellWall == null && cellMineable == null)
            {
                if (Find.PlaySettings.showFertilityOverlay && fertAtMouse != cellFert && !floorAtMouse.IsFloor && !cellFloor.IsFloor)
                    addFlag = false;
                else
                {
                    addFlag = true;
                    foreach (var thing in cellThings)
                    {
                        var def = thing.def.entityDefToBuild == null ? thing.def : thing.def.entityDefToBuild as ThingDef;
                        if (def?.coversFloor == true || def.IsStructure())
                        {
                            addFlag = false;
                            break;
                        }
                    }
                }
            }

            if (addFlag)
            {
                ret.Add(cell);
                EnqueueIfAllowed(cell + IntVec3.North);
                EnqueueIfAllowed(cell + IntVec3.East);
                EnqueueIfAllowed(cell + IntVec3.South);
                EnqueueIfAllowed(cell + IntVec3.West);
            }
        }

        return ret;
    }

    private static bool IsRevealedCell(Map map, IntVec3 cell)
    {
        if (cell.Fogged(map))
            return false;

        var mineable = map.getMineableAt(cell);
        if (mineable?.def.building?.isNaturalRock != true && mineable?.def.building?.isResourceRock != true)
            return true;

        // Require a cardinal face blocked by neither a rock link nor a full edifice.
        // An unfogged interior tile still conceals its ore identity until a face is exposed.
        foreach (var direction in GenAdj.CardinalDirections)
        {
            var adjacent = cell + direction;
            if (adjacent.InBounds(map) && !adjacent.Fogged(map)
                && (map.linkGrid.LinkFlagsAt(adjacent) & LinkFlags.Rock) == 0
                && (adjacent.GetEdifice(map)?.def.fillPercent ?? 0f) < 1f)
                return true;
        }
        return false;
    }

    private static float getFertilityAt(this Map map, IntVec3 cell) 
        => map.fertilityGrid.FertilityAt(cell);

    private static TerrainDef getTerrainAt(this Map map, IntVec3 cell) 
        => map.terrainGrid.TerrainAt(cell);

    private static IEnumerable<Designation> getDesignationsAt(this Map map, IntVec3 cell) 
        => map.designationManager.AllDesignationsAt(cell);

    private static Thing getMineableAt(this Map map, IntVec3 cell) 
        => map.thingGrid.ThingsListAtFast(cell).FirstOrDefault(t => t is Mineable);

    private static bool IsStructure(this Def d)
        => (d as BuildableDef)?.designationCategory?.defName == "Structure";

    private static ThingDef GetStructureDef(this Thing thing)
        => thing.def.entityDefToBuild.IsStructure()
            ? thing.def.entityDefToBuild as ThingDef
            : thing.def.IsStructure()
                ? thing.def : null;

    private static Def getWallDefAt(this Map map, IntVec3 cell)
        =>  cell.GetThingList(map)
                .Select(t => t.GetStructureDef())
                .FirstOrDefault(t => t != null);
}