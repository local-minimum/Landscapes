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
}
