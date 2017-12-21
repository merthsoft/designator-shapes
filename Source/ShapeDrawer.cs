using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Merthsoft.DesignatorShapes {
    public static class ShapeDrawer {
        /// <summary>
        /// Swaps two values.
        /// </summary>
        /// <typeparam name="T">The type of the items we're swapping.</typeparam>
        /// <param name="item1">The first item.</param>
        /// <param name="item2">The second item.</param>
        private static void swap<T>(ref T item1, ref T item2) {
            T temp = item1;
            item1 = item2;
            item2 = temp;
        }

        public static IEnumerable<IntVec3> DrawLine(IntVec3 vert1, IntVec3 vert2) {
            return DrawLine(vert1.x, vert1.y, vert1.z, vert2.x, vert2.y, vert2.z);
        }

        public static IEnumerable<IntVec3> DrawLine(int x1, int y1, int z1, int x2, int y2, int z2) {
            return DrawLine(x1, y1, z1, x2, y2, z2, true);
        }

        public static IEnumerable<IntVec3> DrawLineNoCorners(int x1, int y1, int z1, int x2, int y2, int z2) {
            return DrawLine(x1, y1, z1, x2, y2, z2, false);
        }

        /// <summary>
        /// Draws a line to the sprite.
        /// </summary>
        /// <param name="x1">The X coordinate of one end point.</param>
        /// <param name="z1">The Y coordinate of one end point.</param>
        /// <param name="x2">The X coordinate of the second end point.</param>
        /// <param name="z2">The Y coordinate of the second end point.</param>
        public static IEnumerable<IntVec3> DrawLine(int x1, int y1, int z1, int x2, int y2, int z2, bool fillCorners) {
            var ret = new HashSet<IntVec3>();

            int deltaX = Math.Abs(x1 - x2);
            int deltaZ = Math.Abs(z1 - z2);
            int stepX = x2 < x1 ? 1 : -1;
            int stepZ = z2 < z1 ? 1 : -1;

            int err = deltaX - deltaZ;

            while (true) {
                ret.Add(new IntVec3(x2, y1, z2));
                if (x2 == x1 && z2 == z1) { break; }

                int e2 = 2 * err;
                
                ret.Add(new IntVec3(x2, y1, z2));

                if (e2 > -deltaZ) {
                    err = err - deltaZ;
                    x2 = x2 + stepX;
                }

                if (x2 == x1 && z2 == z1) { break; }
                if (fillCorners) {
                    ret.Add(new IntVec3(x2, y1, z2));
                }

                if (e2 < deltaX) {
                    err = err + deltaX;
                    z2 = z2 + stepZ;
                }
            }

            return ret;
        }

        public static IEnumerable<IntVec3> DrawHorizontalLine(int x1, int x2, int y, int z) {
            if (x1 > x2) { swap(ref x1, ref x2); }
            return Enumerable.Range(x1, x2 - x1 + 1).Select(x => new IntVec3(x, y, z));
        }

        public static IEnumerable<IntVec3> DrawVerticalLine(int x, int y, int z1, int z2) {
            if (z1 > z2) { swap(ref z1, ref z2); }
            return Enumerable.Range(z1, z2 - z1 + 1).Select(z => new IntVec3(x, y, z));
        }

        public static IEnumerable<IntVec3> DrawRectangle(IntVec3 s, IntVec3 t) {
            return DrawRectangle(s.x, s.y, s.z, t.x, t.y, t.z, false);
        }

        public static IEnumerable<IntVec3> DrawRectangleFilled(IntVec3 s, IntVec3 t) {
            return DrawRectangle(s.x, s.y, s.z, t.x, t.y, t.z, true);
        }

        /// <summary>
        /// Draws a rectangle to the sprite.
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="z1"></param>
        /// <param name="x2"></param>
        /// <param name="z2"></param>
        /// <param name="color">The color to draw.</param>
        public static IEnumerable<IntVec3> DrawRectangle(int x1, int y1, int z1, int x2, int y2, int z2, bool fill) {
            var ret = new HashSet<IntVec3>();

            if (!fill) {
                ret.AddRange(DrawHorizontalLine(x1, x2, y1, z1));
                ret.AddRange(DrawHorizontalLine(x1, x2, y1, z2));
                ret.AddRange(DrawVerticalLine(x1, y1, z1, z2));
                ret.AddRange(DrawVerticalLine(x2, y1, z1, z2));
            } else {
                if (x1 > x2) {
                    swap(ref x1, ref x2);
                }
                for (int x = x1; x <= x2; x++) {
                    ret.AddRange(DrawVerticalLine(x, y1, z1, z2));
                }
            }

            return ret;
        }

        public static IEnumerable<IntVec3> DrawPolygon(int x1, int y1, int x2, int y2, int numSides, bool fill = false) {
            if (x2 < x1) { swap(ref x1, ref x2); }
            if (y2 < y1) { swap(ref y1, ref y2); }

            int hr = (x2 - x1) / 2;
            int kr = (y2 - y1) / 2;
            int h = x1 + hr;
            int k = y1 + kr;

            return DrawPolygonUsingRadius(h, 0, k, hr, kr, numSides, fill);
        }

        private static IntVec3 toIntVec(decimal x, decimal y, decimal z) {
            return new IntVec3((int)(x), (int)(y), (int)(z));
        }

        private static IntVec3 getPolygonVertex(int x, int z, int xRadius, int zRadius, int numSides, int i) {
            var theta = 2.0 * Math.PI * i / numSides;
            int xVert = x + (int)Math.Round(xRadius * Math.Cos(theta));
            int zVert = z + (int)Math.Round(zRadius * Math.Sin(theta));
            var vec = new IntVec3(xVert, 0, zVert);
            return vec;
        }

        public static IEnumerable<IntVec3> DrawHexagon(IntVec3 s, IntVec3 t) {
            return DrawHexagon(s.x, s.y, s.z, t.x, t.y, t.z, false);
        }

        public static IEnumerable<IntVec3> DrawHexagonFilled(IntVec3 s, IntVec3 t) {
            return DrawHexagon(s.x, s.y, s.z, t.x, t.y, t.z, true);
        }

        public static IEnumerable<IntVec3> DrawHexagon(int sx, int sy, int sz, int tx, int ty, int tz, bool fill = false) {
            if (tx < sx) { swap(ref sx, ref tx); }
            if (tz < sz) { swap(ref sz, ref tz); }

            var w = tx - sx;
            var h = tz - sz;
            var mx = Math.Floor(w / 2M) + sx;
            var mz = Math.Floor(h / 2M) + sz;
            var wt = Math.Floor(w / 4M);

            var A = toIntVec(sx, sy, mz);
            var B = toIntVec(sx + wt, sy, sz);
            var C = toIntVec(sx + wt, sy, tz);
            var D = toIntVec(tx - wt, sy, sz);
            var E = toIntVec(tx - wt, sy, tz);
            var F = toIntVec(tx, sy, mz);
            
            var ret = new HashSet<IntVec3>();
            ret.AddRange(DrawLine(A, B));
            ret.AddRange(DrawLine(B, D));
            ret.AddRange(DrawLine(F, D));
            ret.AddRange(DrawLine(A, C));
            ret.AddRange(DrawLine(C, E));
            ret.AddRange(DrawLine(F, E));

            if (fill) {
                return Fill(ret);
            } else {
                return ret;
            }
        }

        public static IEnumerable<IntVec3> DrawPolygonUsingRadius(int x, int y, int z, int width, int height, int numSides, bool fill = false) {
            return DrawPolygonUsingRadius(new IntVec3(x, y, z), width, height, numSides, fill);
        }

        // ChJees
        public static IEnumerable<IntVec3> DrawPolygonUsingRadius(IntVec3 center, int width, int height, int numSides, bool fill = false) {
            HashSet<IntVec3> ret = new HashSet<IntVec3>();
            
            //Make points.
            float radiusWidth = width / 2f;
            float radiusHeight = height / 2f;

            float errorWidth = radiusWidth % 1;

            if (errorWidth > 0.5f)
                errorWidth = 1f;
            else
                errorWidth = 0f;

            float errorHeight = radiusHeight % 1;

            if (errorHeight > 0.5f)
                errorHeight = 1f;
            else
                errorHeight = 0f;

            for (int i = 0; i < numSides; i++) {
                float angle = (((360f / (float)numSides) * i) % 360) * (float)(Math.PI / 180d);
                float x, z;
                //if (flipped) {
                //    z = center.z + (float)(radiusWidth * Math.Sin(angle) + errorWidth);
                //    x = center.x + (float)(radiusHeight * Math.Cos(angle) + errorHeight);
                //    //z = center.x + (float)(radiusWidth * Math.Sin(angle));
                //    //x = center.z + (float)(radiusHeight * Math.Cos(angle));
                //} else {
                x = center.x + (float)(radiusWidth * Math.Sin(angle) + errorWidth);
                z = center.z + (float)(radiusHeight * Math.Cos(angle) + errorHeight);
                //x = center.x + (float)(radiusWidth * Math.Sin(angle));
                //z = center.z + (float)(radiusHeight * Math.Cos(angle));
                //}

                //Correct errors
                //x = (float)Math.Round(x, MidpointRounding.ToEven);
                //z = (float)Math.Round(z, MidpointRounding.ToEven);

                ret.Add(new IntVec3((int)x, 0, (int)z));
            }

            if (fill) {
                return Fill(TraceShape(ret));
            } else {
                return ret; // TraceShape(ret);
            }
        }

        public static IEnumerable<IntVec3> TraceShape(this IEnumerable<IntVec3> vertices) {
            var ret = new HashSet<IntVec3>();
            var prev = vertices.First();
            foreach (var current in vertices.Skip(1)) {
                ret.AddRange(DrawLine(prev, current));
                prev = current;
            }
            ret.AddRange(DrawLine(vertices.First(), vertices.Last()));

            return ret;
        }

        //private static bool addVector(this HashSet<IntVec3> vectors, IntVec3 vec) {
        //    return vectors.Add(vec);
        //}

        private static void AddRange(this HashSet<IntVec3> vectors, IEnumerable<IntVec3> newVectors) {
            foreach (var vec in newVectors) {
                vectors.Add(vec);
            }
        }

        public static IEnumerable<IntVec3> DrawEllipse(IntVec3 s, IntVec3 t) {
            return DrawEllipse(s.x, s.y, s.z, t.x, t.y, t.z, false);
        }

        public static IEnumerable<IntVec3> DrawEllipseFilled(IntVec3 s, IntVec3 t) {
            return DrawEllipse(s.x, s.y, s.z, t.x, t.y, t.z, true);
        }

        /// <summary>
        /// Draws an ellipse to the sprite.
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="z1"></param>
        /// <param name="x2"></param>
        /// <param name="z2"></param>
        /// <param name="fill">True to fill the ellipse.</param>
        public static IEnumerable<IntVec3> DrawEllipse(int x1, int y1, int z1, int x2, int y2, int z2, bool fill = false) {
            if (x2 < x1) { swap(ref x1, ref x2); }
            if (z2 < z1) { swap(ref z1, ref z2); }

            int hr = (x2 - x1) / 2;
            int kr = (z2 - z1) / 2;
            int h = x1 + hr;
            int k = z1 + kr;

            return DrawEllipseUsingRadius(h, k, hr, kr, fill);
        }

        private static void incrementX(ref int x, ref int dxt, ref int d2xt, ref int t) { x++; dxt += d2xt; t += dxt; }
        private static void incrementY(ref int y, ref int dyt, ref int d2yt, ref int t) { y--; dyt += d2yt; t += dyt; }

        /// <summary>
        /// Draws a filled ellipse to the sprite.
        /// </summary>
        /// <remarks>Taken from http://enchantia.com/graphapp/doc/tech/ellipses.html. </remarks>
        /// <param name="x">The center point X coordinate.</param>
        /// <param name="y">The center point Y coordinate.</param>
        /// <param name="xRadius">The x radius.</param>
        /// <param name="yRadius">The y radius.</param>
        /// <param name="fill">True to fill the ellipse.</param>
        public static IEnumerable<IntVec3> DrawEllipseUsingRadius(int x, int y, int xRadius, int yRadius, bool fill = false) {
            var ret = new HashSet<IntVec3>();

            int plotX = 0;
            int plotY = yRadius;

            int xRadiusSquared = xRadius * xRadius;
            int yRadiusSquared = yRadius * yRadius;
            int crit1 = -(xRadiusSquared / 4 + xRadius % 2 + yRadiusSquared);
            int crit2 = -(yRadiusSquared / 4 + yRadius % 2 + xRadiusSquared);
            int crit3 = -(yRadiusSquared / 4 + yRadius % 2);

            int t = -xRadiusSquared * plotY;
            int dxt = 2 * yRadiusSquared * plotX, dyt = -2 * xRadiusSquared * plotY;
            int d2xt = 2 * yRadiusSquared, d2yt = 2 * xRadiusSquared;

            while (plotY >= 0 && plotX <= xRadius) {
                ret.Add(new IntVec3(x + plotX, 0, y + plotY));
                if (plotX != 0 || plotY != 0) {
                    ret.Add(new IntVec3(x - plotX, 0, y - plotY));
                }

                if (plotX != 0 && plotY != 0) {
                    ret.Add(new IntVec3(x + plotX, 0, y - plotY));
                    ret.Add(new IntVec3(x - plotX, 0, y + plotY));
                }

                if (t + yRadiusSquared * plotX <= crit1 || t + xRadiusSquared * plotY <= crit3) {
                    incrementX(ref plotX, ref dxt, ref d2xt, ref t);
                } else if (t - xRadiusSquared * plotY > crit2) {
                    incrementY(ref plotY, ref dyt, ref d2yt, ref t);
                } else {
                    incrementX(ref plotX, ref dxt, ref d2xt, ref t);
                    //incrementY(ref plotY, ref dyt, ref d2yt, ref t);
                }
            }

            if (fill) {
                return Fill(ret);
            } else {
                return ret;
            }
        }

        public static IEnumerable<IntVec3> DrawCircle(IntVec3 s, IntVec3 t) {
            var x1 = s.x;
            var y1 = s.y;
            var z1 = s.z;
            var x2 = t.x;
            var y2 = t.y;
            var z2 = t.z;

            if (x2 < x1) { swap(ref x1, ref x2); }
            if (z2 < z1) { swap(ref z1, ref z2); }

            var r = Math.Max(x2 - x1, z2 - z1);
            return DrawCircle(s.x, s.z, r);
        }

        public static IEnumerable<IntVec3> DrawCircleFilled(IntVec3 s, IntVec3 t) {
            var x1 = s.x;
            var y1 = s.y;
            var z1 = s.z;
            var x2 = t.x;
            var y2 = t.y;
            var z2 = t.z;

            if (x2 < x1) { swap(ref x1, ref x2); }
            if (z2 < z1) { swap(ref z1, ref z2); }

            var r = Math.Max(x2 - x1, z2 - z1);
            return DrawCircle(s.x, s.z, r, true);
        }

        /// <summary>
        /// Draws a circle to the sprite.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="r"></param>
        /// <param name="fill">True to fill the circle.</param>
        public static IEnumerable<IntVec3> DrawCircle(int x, int y, int r, bool fill = false) {
            return DrawEllipseUsingRadius(x, y, r, r, fill);
        }

        public static IEnumerable<IntVec3> FloodFill(IntVec3 s, IntVec3 t) {
            HashSet<IntVec3> ret = new HashSet<IntVec3>();
            var designator = Find.DesignatorManager.SelectedDesignator;
            Map map = Find.VisibleMap;

            var wallAtMouse = getWallAt(map, t);
            var blueprintAtMouse = getBlueprintAt(map, t);
            var designationsAtMouse = getDesignaionsAt(map, t);
            var mineableAtMouse = getMineableAt(map, t);

            var cells = new Queue<IntVec3>();
            cells.Enqueue(t);
            while (cells.Count() > 0 && cells.Count() < 900) {
                var cell = cells.Dequeue();
                if (!cell.InBounds(map)) { continue; }
                if (ret.Contains(cell)) { continue; }

                var cellWall = getWallAt(map, cell);
                var cellDes = getDesignaionsAt(map, cell);
                var cellBlueprint = getBlueprintAt(map, cell);
                var cellMineableAt = getMineableAt(map, cell);

                var addFlag = false;
                var neighborsFlag = false;

                if (wallAtMouse != null) {
                    if (cellWall?.def == wallAtMouse.def) {
                        addFlag = true;
                        neighborsFlag = true;
                    }
                } else if (mineableAtMouse != null) {
                    if (cellMineableAt?.def == mineableAtMouse.def) {
                        addFlag = true;
                        neighborsFlag = true;
                    }
                } else {
                    if (cellWall != null && designator is Designator_Build) {
                        addFlag = true;
                    } else if (cellWall == null && cellDes.Count() == 0 && cellBlueprint == null) {
                        addFlag = true;
                        neighborsFlag = true;
                    }
                }

                if (addFlag) { ret.Add(cell); }
                if (neighborsFlag) {
                    cells.Enqueue(cell + IntVec3.North);
                    cells.Enqueue(cell + IntVec3.East);
                    cells.Enqueue(cell + IntVec3.South);
                    cells.Enqueue(cell + IntVec3.West);
                }
            }

            return ret;
        }

        static IEnumerable<Designation> getDesignaionsAt(Map map, IntVec3 cell) {
            return map.designationManager.AllDesignationsAt(cell);
        }

        static Thing getMineableAt(Map map, IntVec3 cell) {
            return map.thingGrid.ThingsListAtFast(cell).FirstOrDefault(t => t is Mineable);
        }

        static Thing getWallAt(Map map, IntVec3 cell) {
            return map.thingGrid.ThingsListAtFast(cell).FirstOrDefault(t => (t.def as BuildableDef)?.designationCategory == DesignationCategoryDefOf.Structure);
        }

        static Thing getBlueprintAt(Map map, IntVec3 cell) {
            return map.thingGrid.ThingsListAtFast(cell).FirstOrDefault(t => ((t as Blueprint)?.def.entityDefToBuild as BuildableDef)?.designationCategory ==DesignationCategoryDefOf.Structure);
        }

        public static IEnumerable<IntVec3> Fill(IEnumerable<IntVec3> outLine) {
            var ret = new HashSet<IntVec3>();
            foreach (var lineGroup in outLine.OrderBy(vec => vec.x).GroupBy(vec => vec.z)) {
                if (lineGroup.Count() == 1) {
                    ret.Add(lineGroup.First());
                } else {
                    var sorted = lineGroup.OrderBy(v => v.x);
                    var point1 = lineGroup.First();
                    var point2 = lineGroup.Last();
                    ret.AddRange(DrawHorizontalLine(point1.x, point2.x, point1.y, lineGroup.Key));
                }
            }

            return ret;
        }

        public static IEnumerable<IntVec3> Rotate(this IEnumerable<IntVec3> shape, IntVec3 s, IntVec3 t, float angle) {
            return shape.Rotate(s.x, s.y, s.z, t.x, t.y, t.z, angle);
        }

        public static IEnumerable<IntVec3> Rotate(this IEnumerable<IntVec3> shape, int x1, int y1, int z1, int x2, int y2, int z2, float angle) {
            if (x2 < x1) { swap(ref x1, ref x2); }
            if (z2 < z1) { swap(ref z1, ref z2); }

            var deltaX = (int)(x1 + (x2 - x1) / 2f);
            var deltaZ = (int)(z1 + (z1 - z1) / 2f);

            var ret = new HashSet<IntVec3>();
            foreach (var vec in shape) {
                var x = vec.x - deltaX;
                var z = vec.z - deltaZ;

                var rotX = (int)Math.Round(x * Math.Cos(angle) - z * Math.Sin(angle));
                var rotZ = (int)Math.Round(x * Math.Sin(angle) + z * Math.Cos(angle));

                ret.Add(new IntVec3(rotX + deltaX, y1, rotZ + deltaZ));
            }

            return ret;
        }
    }
}
