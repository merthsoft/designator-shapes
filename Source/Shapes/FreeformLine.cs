using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace Merthsoft.DesignatorShapes.Shapes;

public static class FreeformLine
{
    private static bool[] _mask;
    private static readonly List<IntVec3> _designation = [];
    private static readonly List<IntVec2> _path = [];
    private static IntVec2 _min, _max, _maskSize;
    private static IntVec3 _oldStart, _oldStop;
    private static int _oldThickness, _lastPointDrawn;
    private static bool[,] _drawMask;
    private static bool _startNew = true;

    internal static void FreeMemory()
    {
        _startNew = true;
        _mask = null;
        _designation.Clear();
        _path.Clear();
    }

    public static IEnumerable<IntVec3> Line(IntVec3 s, IntVec3 t)
    {
        if (t != _oldStop) _path.Add(new IntVec2(t.x, t.z));

        int radius = (_oldThickness + 1) / 2;
        bool runAnyway = _oldThickness != Math.Max(DesignatorShapes.Thickness, 1);
        if (runAnyway)
        {
            _oldThickness = Math.Max(DesignatorShapes.Thickness, 1);

            // Create a circle out of booleans
            _drawMask = new bool[_oldThickness, _oldThickness];

            int center = _oldThickness - 1;
            int radiusSqr = _oldThickness * _oldThickness;
            for (int x = 0; x < _oldThickness; x++)
            {
                for (int y = 0; y < _oldThickness; y++)
                {
                    int x1 = x * 2 - center;
                    int y1 = y * 2 - center;
                    _drawMask[x, y] = x1 * x1 + y1 * y1 <= radiusSqr;
                }
            }

            // Entire path needs to be redone
            IntVec2 radiusV2 = new IntVec2(radius, radius);
            for (int i = 0; i < _path.Count; i++)
            {
                IntVec2 point = _path[i];
                _min = Min(_min, point - radiusV2);
                _max = Max(_max, point + radiusV2);
            }

            _maskSize = _max - _min + IntVec2.One;
            _mask = new bool[_maskSize.x * _maskSize.z];
            _lastPointDrawn = 0;
        }

        if (_startNew || s != _oldStart)
        {
            // Starting point changed so start new line
            _startNew = false;
            _oldStart = s;
            _designation.Clear();
            _path.Clear();
            _path.Add(new IntVec2(s.x, s.z));
            runAnyway = true;
            _min = new IntVec2(s.x - radius, s.x - radius);
            _max = new IntVec2(s.z + radius, s.z + radius);
            _maskSize = _max - _min + IntVec2.One;
            _mask = new bool[_maskSize.x * _maskSize.z];
            _lastPointDrawn = 0;
        }

        if (!runAnyway && t == _oldStop)
        {
            // Haven't moved
            return _designation;
        }

        _oldStop = t;

        IntVec2 newMin = Min(_min, new IntVec2(t.x - radius, t.z - radius));
        IntVec2 newMax = Max(_max, new IntVec2(t.x + radius, t.z + radius));
        if (newMin != _min || newMax != _max)
        {
            // Grow mask
            IntVec2 newSize = newMax - newMin + IntVec2.One;
            IntVec2 curOffset = Max(newMin - _min, IntVec2.Zero);
            IntVec2 newOffset = Max(_min - newMin, IntVec2.Zero);
            IntVec2 overlapMin = Max(newMin, _min);
            IntVec2 overlapMax = Min(newMax, _max);
            IntVec2 overlapSize = overlapMax - overlapMin + IntVec2.One;

            // Copy old mask to new mask
            bool[] newMask = new bool[newSize.x * newSize.z];
            for (int z = 0; z < overlapSize.z; z++)
            {
                Array.Copy(
                    _mask,
                    curOffset.x + (z + curOffset.z) * _maskSize.x,
                    newMask,
                    newOffset.x + (z + newOffset.z) * newSize.x,
                    overlapSize.x
                );
            }

            _min = newMin;
            _max = newMax;
            _maskSize = newSize;
            _mask = newMask;
        }

        MaskPath();

        // Compute path
        _designation.Clear();
        for (int z = 0; z < _maskSize.z; z++)
        {
            int zw = z * _maskSize.x;
            for (int x = 0; x < _maskSize.x; x++)
            {
                if (_mask[x + zw]) _designation.Add(new IntVec3(x + _min.x, s.y, z + _min.z));
            }
        }

        return _designation;
    }

    private static void MaskPath()
    {
        IntVec2 strokeRadius = new IntVec2(_oldThickness, _oldThickness) / 2;
        if (_path.Count == 1)
        {
            // Draw single point
            IntVec2 point = _path[0];
            DrawBrushStrokeAt(point - strokeRadius);
            return;
        }

        if (_lastPointDrawn + 1 >= _path.Count)
        {
            // No points to draw
            return;
        }

        // Draw stroke
        for (; _lastPointDrawn + 1 < _path.Count; _lastPointDrawn++)
        {
            IntVec2 last = _path[_lastPointDrawn];
            IntVec2 next = _path[_lastPointDrawn + 1];

            if (last.x == next.x)
            {
                // Vertical line
                for (int z = Math.Min(last.z, next.z); z <= Math.Max(last.z, next.z); z++)
                {
                    DrawBrushStrokeAt(new IntVec2(last.x, z) - strokeRadius);
                }
                continue;
            }

            if (last.z == next.z)
            {
                // Horizontal line
                for (int x = Math.Min(last.x, next.x); x <= Math.Max(last.x, next.x); x++)
                {
                    DrawBrushStrokeAt(new IntVec2(x, last.z) - strokeRadius);
                }
                continue;
            }

            // Diagonal line
            double dx = (last.x - next.x) / (double)(last.z - next.z);
            double dz = (last.z - next.z) / (double)(last.x - next.x);
            double dradius = _oldThickness * 0.5;
            if (Math.Abs(dx) > Math.Abs(dz))
            {
                // Go along x
                if (last.x > next.x) (last, next) = (next, last);

                for (int x = last.x; x <= next.x; x++)
                {
                    DrawBrushStrokeAt(new IntVec2(x - strokeRadius.x, (int)Math.Round(dz * (x - last.x) + last.z - dradius, MidpointRounding.AwayFromZero)));
                }
            }
            else
            {
                // Go along y
                if (last.z > next.z) (last, next) = (next, last);

                for (int z = last.z; z <= next.z; z++)
                {
                    DrawBrushStrokeAt(new IntVec2((int)Math.Round(dx * (z - last.z) + last.x - dradius, MidpointRounding.AwayFromZero), z - strokeRadius.z));
                }
            }
        }
    }

    private static IntVec2 Min(IntVec2 a, IntVec2 b) => new IntVec2(Math.Min(a.x, b.x), Math.Min(a.z, b.z));

    private static IntVec2 Max(IntVec2 a, IntVec2 b) => new IntVec2(Math.Max(a.x, b.x), Math.Max(a.z, b.z));

    private static void DrawBrushStrokeAt(IntVec2 pos)
    {
        pos -= _min;

        IntVec2 min = Max(pos - IntVec2.One, IntVec2.Zero);
        IntVec2 max = Min(min + new IntVec2(_oldThickness, _oldThickness), _maskSize - IntVec2.One);

        for (int z = min.z; z <= max.z; z++)
        {
            int zw = z * _maskSize.x;
            for (int x = min.x; x <= max.x; x++)
            {
                _mask[x + zw] |= GetPixel(x - pos.x, z - pos.z);
            }
        }
    }

    private static bool GetPixel(int x, int z) => x >= 0 && z >= 0 && x < _oldThickness && z < _oldThickness && _drawMask[x, z];
}
