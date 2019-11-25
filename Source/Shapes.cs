using RimWorld;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Merthsoft.DesignatorShapes {
    public static partial class Shapes {
        public static IEnumerable<IntVec3> Rectangle(IntVec3 s, IntVec3 t) =>
            Rectangle(s.x, s.y, s.z, t.x, t.y, t.z, false, DesignatorShapes.Rotation, DesignatorShapes.Thickness);

        public static IEnumerable<IntVec3> RectangleFilled(IntVec3 s, IntVec3 t) =>
            Rectangle(s.x, s.y, s.z, t.x, t.y, t.z, true, DesignatorShapes.Rotation, DesignatorShapes.Thickness);

        public static IEnumerable<IntVec3> Pentagon(IntVec3 s, IntVec3 t) =>
            Pentagon(s.x, s.y, s.z, t.x, t.y, t.z, false, DesignatorShapes.Rotation);

        public static IEnumerable<IntVec3> PentagonFilled(IntVec3 s, IntVec3 t) =>
            Pentagon(s.x, s.y, s.z, t.x, t.y, t.z, true, DesignatorShapes.Rotation);

        public static IEnumerable<IntVec3> Hexagon(IntVec3 s, IntVec3 t) =>
            Hexagon(s.x, s.y, s.z, t.x, t.y, t.z, false, DesignatorShapes.Rotation);

        public static IEnumerable<IntVec3> HexagonFilled(IntVec3 s, IntVec3 t) =>
            Hexagon(s.x, s.y, s.z, t.x, t.y, t.z, true, DesignatorShapes.Rotation);

        public static IEnumerable<IntVec3> Ellipse(IntVec3 s, IntVec3 t) =>
            Ellipse(s.x, s.y, s.z, t.x, t.y, t.z, false);

        public static IEnumerable<IntVec3> EllipseFilled(IntVec3 s, IntVec3 t) =>
            Ellipse(s.x, s.y, s.z, t.x, t.y, t.z, true);

        public static IEnumerable<IntVec3> Circle(IntVec3 s, IntVec3 t) => 
            Circle(s, t, false);

        public static IEnumerable<IntVec3> CircleFilled(IntVec3 s, IntVec3 t) => 
            Circle(s, t, true);

        public static IEnumerable<IntVec3> SunLamp(IntVec3 vert1, IntVec3 vert2) =>
            GenRadialCircle(vert1, DesignatorShapes.SunLampRadius, true);

        public static IEnumerable<IntVec3> SunLampOutline(IntVec3 vert1, IntVec3 vert2) =>
            GenRadialCircle(vert1, DesignatorShapes.SunLampRadius, false);

        public static IEnumerable<IntVec3> TradeBeacon(IntVec3 vert1, IntVec3 vert2) =>
            GenRadialCircle(vert1, DesignatorShapes.TradeBeaconRadius, true);

        public static IEnumerable<IntVec3> TradeBeaconOutline(IntVec3 vert1, IntVec3 vert2) =>
            GenRadialCircle(vert1, DesignatorShapes.TradeBeaconRadius, false);

        public static IEnumerable<IntVec3> FloodFill(IntVec3 s, IntVec3 t) {
            var ret = new HashSet<IntVec3>();
            var map = Find.CurrentMap;

            var wallAtMouse = getWallDefAt(map, t);
            var designationsAtMouse = getDesignaionsAt(map, t);
            var mineableAtMouse = getMineableAt(map, t);
            var floorAtMouse = getFloorAt(map, t);

            var cells = new Queue<IntVec3>();
            cells.Enqueue(t);

            while (cells.Count() > 0 && ret.Count() < DesignatorShapes.Settings.FloodFillCellLimit) {
                var cell = cells.Dequeue();
                if (!cell.InBounds(map)) { continue; }
                if (ret.Contains(cell)) { continue; }
                if (!Find.DesignatorManager.SelectedDesignator.CanDesignateCell(cell).Accepted) { continue; }

                var cellWall = getWallDefAt(map, cell);
                var cellDes = getDesignaionsAt(map, cell);
                var cellMineable = getMineableAt(map, cell);
                var cellFloor = getFloorAt(map, cell);
                var cellThings = map.thingGrid.ThingsListAtFast(cell);

                var addFlag = false;
                var neighborsFlag = false;

                if (wallAtMouse != null) {
                    if (cellWall == null) { continue; }
                    if (cellWall.defName == wallAtMouse.defName) {
                        addFlag = true;
                        neighborsFlag = true;
                    }
                } else if (mineableAtMouse != null) {
                    if (cellMineable?.def == mineableAtMouse.def) {
                        addFlag = true;
                        neighborsFlag = true;
                    }
                } else if (designationsAtMouse?.Count() > 0) {
                    if (cellDes.Count() > 0) {
                        addFlag = true;
                        neighborsFlag = true;
                    }
                } else {
                    if (cellWall == null && cellMineable == null) {
                        addFlag = true;
                        neighborsFlag = true;
                        foreach (var thing in cellThings) {
                            var def = thing.def.entityDefToBuild == null ? thing.def : thing.def.entityDefToBuild as ThingDef;
                            if (def.coversFloor || def.designationCategory == DesignationCategoryDefOf.Structure) {
                                addFlag = false;
                                neighborsFlag = false;
                                break;
                            }
                        }
                    }
                }

                if (addFlag) {
                    ret.Add(cell);
                }
                if (neighborsFlag) {
                    cells.Enqueue(cell + IntVec3.North);
                    cells.Enqueue(cell + IntVec3.East);
                    cells.Enqueue(cell + IntVec3.South);
                    cells.Enqueue(cell + IntVec3.West);
                }
            }

            return ret;
        }

        static TerrainDef getFloorAt(Map map, IntVec3 cell) {
            return map.terrainGrid.TerrainAt(cell);
        }

        static IEnumerable<Designation> getDesignaionsAt(Map map, IntVec3 cell) {
            return map.designationManager.AllDesignationsAt(cell);
        }

        static Thing getMineableAt(Map map, IntVec3 cell) {
            return map.thingGrid.ThingsListAtFast(cell).FirstOrDefault(t => t is Mineable);
        }

        static Def getWallDefAt(Map map, IntVec3 cell) {
            var things = cell.GetThingList(map);
            foreach (var thing in things) {
                switch (thing) {
                    case Blueprint b when b.def.entityDefToBuild.designationCategory == DesignationCategoryDefOf.Structure:
                        return b.def.entityDefToBuild as ThingDef;
                    case Frame f when f.def.entityDefToBuild.designationCategory == DesignationCategoryDefOf.Structure:
                        return f.def.entityDefToBuild as ThingDef;
                    case Thing t when (t.def as BuildableDef)?.designationCategory == DesignationCategoryDefOf.Structure:
                        return t.def;
                    default:
                        continue;
                }
            }

            return null;
        }

    }
}
