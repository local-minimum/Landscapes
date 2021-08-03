using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static Vector2 RandomPoint(this Rect r) {
        var x = Random.Range(r.xMin, r.xMax);
        var y = Random.Range(r.yMin, r.yMax);
        return new Vector2(x, y);
    }

    public static Vector2 RandomPoint(this Rect r, float padding)
    {
        var x = Random.Range(r.xMin + padding, r.xMax - padding);
        var y = Random.Range(r.yMin + padding, r.yMax - padding);
        return new Vector2(x, y);

    }

    public static readonly GeoNode.Direction[] Directions = new GeoNode.Direction[]
    {
        GeoNode.Direction.E,
        GeoNode.Direction.NE,
        GeoNode.Direction.N,
        GeoNode.Direction.NW,
        GeoNode.Direction.W,
        GeoNode.Direction.SW,
        GeoNode.Direction.S,
        GeoNode.Direction.SE
    };

    public static float AsAngle(this GeoNode.Direction dir)
    {
        switch (dir)
        {
            case GeoNode.Direction.E:
                return 0;
            case GeoNode.Direction.NE:
                return 45;
            case GeoNode.Direction.N:
                return 90;
            case GeoNode.Direction.NW:
                return 135;
            case GeoNode.Direction.W:
                return 180;
            case GeoNode.Direction.SW:
                return 225;
            case GeoNode.Direction.S:
                return 270;
            case GeoNode.Direction.SE:
                return 315;
        }
        throw new System.ArgumentException(string.Format("{0} has no angle.", dir));
        
    }

    public static int AsOrdinal(this GeoNode.Direction dir)
    {
        for (int i = 0; i<Directions.Length; i++)
        {
            if (dir == Directions[i]) return i;
        }
        return -1;
    }

    public static float Angle(this GeoNode.Direction dir, GeoNode.Direction other)
    {
        var a = dir.AsOrdinal();
        var b = other.AsOrdinal();
        var d1 = a - b;
        var d2 = b - a;
        if (d1 < 0) d1 += 8;
        if (d2 < 0) d2 += 8;
        return Mathf.Min(d1, d2) * 45f;
    }

    public static List<GeoNode.Direction> WithNeighbours(this GeoNode.Direction dir, int flanking)
    {
        var ordinal = dir.AsOrdinal();
        var directions = new List<GeoNode.Direction>();
        for (int i=-flanking; i<=flanking; i++)
        {
            var idx = i + ordinal;
            if (idx < 0) idx += 8;
            directions.Add(Directions[idx % 8]);
        }
        return directions;
    }

    public static GeoNode.Direction Inverted(this GeoNode.Direction dir)
    {
        var ordinal = dir.AsOrdinal();
        if (ordinal == -1) return GeoNode.Direction.NONE;
        return Directions[(ordinal + 4) % 8];
    }

    public static bool IsCardinal(this GeoNode.Direction dir)
    {
        return dir == GeoNode.Direction.E
            || dir == GeoNode.Direction.W
            || dir == GeoNode.Direction.N
            || dir == GeoNode.Direction.S;
    }

    public static GeoNode.Direction[] AllowedRotations(this GeoNode.Direction dir, GeoNode.Rotation rotation)
    {
        switch (rotation) {
            case GeoNode.Rotation.CW: 
                switch (dir)
                {
                    case GeoNode.Direction.E:
                        return new GeoNode.Direction[] { GeoNode.Direction.SE, GeoNode.Direction.S, GeoNode.Direction.SW, GeoNode.Direction.W};
                    case GeoNode.Direction.SE:
                        return new GeoNode.Direction[] { GeoNode.Direction.S, GeoNode.Direction.SW, GeoNode.Direction.W, GeoNode.Direction.NW};
                    case GeoNode.Direction.S:
                        return new GeoNode.Direction[] { GeoNode.Direction.SW, GeoNode.Direction.W, GeoNode.Direction.NW, GeoNode.Direction.N };
                    case GeoNode.Direction.SW:
                        return new GeoNode.Direction[] { GeoNode.Direction.W, GeoNode.Direction.NW, GeoNode.Direction.N, GeoNode.Direction.NE };
                    case GeoNode.Direction.W:
                        return new GeoNode.Direction[] { GeoNode.Direction.NW, GeoNode.Direction.N, GeoNode.Direction.NE, GeoNode.Direction.E };
                    case GeoNode.Direction.NW:
                        return new GeoNode.Direction[] { GeoNode.Direction.N, GeoNode.Direction.NE, GeoNode.Direction.E, GeoNode.Direction.SE };
                    case GeoNode.Direction.N:
                        return new GeoNode.Direction[] { GeoNode.Direction.NE, GeoNode.Direction.E, GeoNode.Direction.SE, GeoNode.Direction.S };
                    case GeoNode.Direction.NE:
                        return new GeoNode.Direction[] { GeoNode.Direction.E, GeoNode.Direction.SE, GeoNode.Direction.S, GeoNode.Direction.SW };
                }
                break;
            case GeoNode.Rotation.CCW:
                switch (dir)
                {
                    case GeoNode.Direction.E:
                        return new GeoNode.Direction[] { GeoNode.Direction.NE, GeoNode.Direction.N, GeoNode.Direction.NW, GeoNode.Direction.W };
                    case GeoNode.Direction.NE:
                        return new GeoNode.Direction[] { GeoNode.Direction.N, GeoNode.Direction.NW, GeoNode.Direction.W, GeoNode.Direction.SW };
                    case GeoNode.Direction.N:
                        return new GeoNode.Direction[] { GeoNode.Direction.NW, GeoNode.Direction.W, GeoNode.Direction.SW, GeoNode.Direction.S };
                    case GeoNode.Direction.NW:
                        return new GeoNode.Direction[] { GeoNode.Direction.W, GeoNode.Direction.SW, GeoNode.Direction.S, GeoNode.Direction.SE };
                    case GeoNode.Direction.W:
                        return new GeoNode.Direction[] { GeoNode.Direction.SW, GeoNode.Direction.S, GeoNode.Direction.SE, GeoNode.Direction.E };
                    case GeoNode.Direction.SW:
                        return new GeoNode.Direction[] { GeoNode.Direction.S, GeoNode.Direction.SE, GeoNode.Direction.E, GeoNode.Direction.NE };
                    case GeoNode.Direction.S:
                        return new GeoNode.Direction[] { GeoNode.Direction.SE, GeoNode.Direction.E, GeoNode.Direction.NE, GeoNode.Direction.N };
                    case GeoNode.Direction.SE:
                        return new GeoNode.Direction[] { GeoNode.Direction.E, GeoNode.Direction.NE, GeoNode.Direction.N, GeoNode.Direction.NW };
                }
                break;
        }
        return new GeoNode.Direction[0];
    }

    public static GeoNode.Direction AsDirection(this IEnumerable<GeoNode.Direction> dirs)
    {
        var dir = GeoNode.Direction.NONE;
        foreach (var d in dirs)
        {
            dir |= d;
        }
        return dir;
    }

}
