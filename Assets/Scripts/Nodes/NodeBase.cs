using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class NodeBase : MonoBehaviour
{
    public enum Topology
    {
        None = 0,
        WorldInternal = 1,
        WorldEdge = 2,
        Land = 4,
        Water = 8,
        Shore = 16,
        Main = 32
    };

    public enum Rotation { CW, CCW };

    public enum Direction
    {
        E = 1,
        NE = 2,
        N = 4,
        NW = 8,
        W = 16,
        SW = 32,
        S = 64,
        SE = 128,
        NONE = 0
    };

    public static Direction DirectionFromNodes(NodeBase from, NodeBase to, float angleTolerance = 5)
    {
        var offset = from.PlanarOffset(to);
        var a = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
        for (int i = 0; i < Extensions.Directions.Length; i++)
        {
            var dir = Extensions.Directions[i];
            var dirA = dir.AsAngle();
            if (Mathf.Abs(Mathf.DeltaAngle(dirA, a)) < angleTolerance) return dir;
        }
        return Direction.NONE;
    }

    #region Neighbourhood
    protected List<NodeBase> neighbours = new List<NodeBase>();

    void _SetNeighbours(List<NodeBase> nodes)
    {
        neighbours = nodes
            .Select(n =>
            {
                var a = Mathf.Atan2(n.transform.position.z - transform.position.z, n.transform.position.x - transform.position.x);
                return (a, n);
            })
            .OrderBy(i => i.a)
            .Select(i => i.n)
            .ToList();
    }
    public void SetNeighbours(List<NodeBase> nodes)
    {
        if (neighbours.Count == 0)
        {
            _SetNeighbours(nodes);
        }
        else
        {
            throw new System.InvalidOperationException(
                "Neighbours may only be set once"
            );
        }
    }

    public void AddNeighbour(NodeBase other)
    {
        if (other == this)
        {
            throw new System.ArgumentException(string.Format("Can't be neighbour to oneself, {0}", name));
        }
        if (!neighbours.Contains(other))
        {
            neighbours.Add(other);
            _SetNeighbours(neighbours);
            other.AddNeighbour(this);
        }
    }

    public void RemoveNeighbour(NodeBase other)
    {
        if (other == this)
        {
            throw new System.ArgumentException(string.Format("Can't un-neighbour myself, {0}", name));
        }
        if (neighbours.Contains(other))
        {
            neighbours.Remove(other);
            other.RemoveNeighbour(this);
        }
    }

    public bool HasNeighbour(NodeBase other)
    {
        for (int i = 0, l = neighbours.Count; i < l; i++)
        {
            if (neighbours[i] == other) return true;
        }
        return false;
    }


    #endregion

    #region Topology
    public Topology topology
    {
        get
        {
            var topology = Topology.WorldInternal;
            if (neighbours.Count < 5)
            {
                topology = Topology.WorldEdge;
            }
            bool water = Elevation < 0;
            topology |= water ? Topology.Water : Topology.Land;
            bool shore = false;
            for (int i = 0, l = neighbours.Count; i < l; i++)
            {
                if ((neighbours[i].Elevation < 0) != water)
                {
                    shore = true;
                    break;
                }
            }
            topology |= shore ? Topology.Shore : Topology.Main;
            return topology;
        }
    }

    public float Elevation
    {
        get
        {
            return transform.position.y;
        }

        set
        {
            Vector3 pos = transform.position;
            pos.y = value;
            transform.position = pos;
        }
    }

    #endregion

    #region Planar Coordinates
    public Vector2 PlanarPosition
    {
        get
        {
            return new Vector2(transform.position.x, transform.position.z);
        }
    }

    /**Vector pointing to other
     */
    public Vector2 PlanarOffset(NodeBase other)
    {
        var offset = other.transform.position - transform.position;
        return new Vector2(offset.x, offset.z);
    }

    public float PlanarDistance(NodeBase other)
    {
        Vector3 offset = other.transform.position - transform.position;
        offset.y = 0;
        return offset.magnitude;
    }

    public float AveragePlanarDistance
    {
        get
        {
            if (neighbours.Count == 0)
            {
                throw new System.ArithmeticException("Node lacks neighbours.");
            }
            Vector2 myPos = PlanarPosition;
            return neighbours
                .Select(node => Vector2.Distance(myPos, node.PlanarPosition))
                .Sum() / neighbours.Count;
        }
    }
    #endregion
}
