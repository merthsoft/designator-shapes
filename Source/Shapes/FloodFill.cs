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

        var wallAtMouse = map.getWallDefAt(t);
        var designationsAtMouse = map.getDesignationsAt(t);
        var mineableAtMouse = map.getMineableAt(t);
        var floorAtMouse = map.getTerrainAt(t);
        var fertAtMouse = map.getFertilityAt(t);

        var cells = new Queue<IntVec3>();
        cells.Enqueue(t);

        while (cells.Count() > 0 && ret.Count() < DesignatorShapes.Settings.FloodFillCellLimit)
        {
            var cell = cells.Dequeue();
            if (!cell.InBounds(map))
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
            else if (designationsAtMouse?.Count() > 0)
            {
                if (cellDes.Count() > 0)
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
                        if (def.coversFloor || def.IsStructure())
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
                cells.Enqueue(cell + IntVec3.North);
                cells.Enqueue(cell + IntVec3.East);
                cells.Enqueue(cell + IntVec3.South);
                cells.Enqueue(cell + IntVec3.West);
            }
        }

        return ret;
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