using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class Extensions
{
    #region Rect
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

    public static Vector2 ScaledPosition(this Rect r, Vector2 pos)
    {
        return new Vector2((pos.x - r.xMin) / r.width, (pos.y - r.yMin) / r.height);
    }
    #endregion Rect

    #region Direction
    public static readonly NodeBase.Direction[] Directions = new NodeBase.Direction[]
    {
        NodeBase.Direction.E,
        NodeBase.Direction.NE,
        NodeBase.Direction.N,
        NodeBase.Direction.NW,
        NodeBase.Direction.W,
        NodeBase.Direction.SW,
        NodeBase.Direction.S,
        NodeBase.Direction.SE
    };

    public static float AsAngle(this NodeBase.Direction dir)
    {
        switch (dir)
        {
            case NodeBase.Direction.E:
                return 0;
            case NodeBase.Direction.NE:
                return 45;
            case NodeBase.Direction.N:
                return 90;
            case NodeBase.Direction.NW:
                return 135;
            case NodeBase.Direction.W:
                return 180;
            case NodeBase.Direction.SW:
                return 225;
            case NodeBase.Direction.S:
                return 270;
            case NodeBase.Direction.SE:
                return 315;
        }
        throw new System.ArgumentException(string.Format("{0} has no angle.", dir));
        
    }

    public static int AsOrdinal(this NodeBase.Direction dir)
    {
        for (int i = 0; i<Directions.Length; i++)
        {
            if (dir == Directions[i]) return i;
        }
        return -1;
    }

    public static float Angle(this NodeBase.Direction dir, NodeBase.Direction other)
    {
        var a = dir.AsOrdinal();
        var b = other.AsOrdinal();
        var d1 = a - b;
        var d2 = b - a;
        if (d1 < 0) d1 += 8;
        if (d2 < 0) d2 += 8;
        return Mathf.Min(d1, d2) * 45f;
    }

    public static List<NodeBase.Direction> WithNeighbours(this NodeBase.Direction dir, int flanking)
    {
        var ordinal = dir.AsOrdinal();
        var directions = new List<NodeBase.Direction>();
        for (int i=-flanking; i<=flanking; i++)
        {
            var idx = i + ordinal;
            if (idx < 0) idx += 8;
            directions.Add(Directions[idx % 8]);
        }
        return directions;
    }

    public static NodeBase.Direction Inverted(this NodeBase.Direction dir)
    {
        var ordinal = dir.AsOrdinal();
        if (ordinal == -1) return NodeBase.Direction.NONE;
        return Directions[(ordinal + 4) % 8];
    }

    public static List<NodeBase.Direction> CCWFrom(this NodeBase.Direction dir, bool includeSelf)
    {        
        var ordinal = dir.AsOrdinal();
        var after = Directions.Skip(ordinal + 1).Take(8 - ordinal).ToList();
        after.AddRange(Directions.Take(ordinal + (includeSelf ? 1 : 0)));
        return after;        
    }

    public static bool IsCardinal(this NodeBase.Direction dir)
    {
        return dir == NodeBase.Direction.E
            || dir == NodeBase.Direction.W
            || dir == NodeBase.Direction.N
            || dir == NodeBase.Direction.S;
    }

    public static NodeBase.Direction[] AllowedRotations(this NodeBase.Direction dir, NodeBase.Rotation rotation)
    {
        switch (rotation) {
            case NodeBase.Rotation.CW: 
                switch (dir)
                {
                    case NodeBase.Direction.E:
                        return new NodeBase.Direction[] { NodeBase.Direction.SE, NodeBase.Direction.S, NodeBase.Direction.SW, NodeBase.Direction.W};
                    case NodeBase.Direction.SE:
                        return new NodeBase.Direction[] { NodeBase.Direction.S, NodeBase.Direction.SW, NodeBase.Direction.W, NodeBase.Direction.NW};
                    case NodeBase.Direction.S:
                        return new NodeBase.Direction[] { NodeBase.Direction.SW, NodeBase.Direction.W, NodeBase.Direction.NW, NodeBase.Direction.N };
                    case NodeBase.Direction.SW:
                        return new NodeBase.Direction[] { NodeBase.Direction.W, NodeBase.Direction.NW, NodeBase.Direction.N, GeoNode.Direction.NE };
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

    public static NodeBase.Direction AsDirection(this IEnumerable<GeoNode.Direction> dirs)
    {
        var dir = NodeBase.Direction.NONE;
        foreach (var d in dirs)
        {
            dir |= d;
        }
        return dir;
    }
    #endregion Direction
}
