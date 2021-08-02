using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class GeoNode : MonoBehaviour
{
    public enum Direction {
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
    
    public enum Rotation { CW, CCW };
    
    public static Direction DirectionFromNodes(GeoNode from, GeoNode to, float angleTolerance = 5)
    {
        var offset = from.PlanarOffset(to);
        var a = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
        for (int i=0; i<Extensions.Directions.Length; i++)        
        {
            var dir = Extensions.Directions[i];
            var dirA = dir.AsAngle();
            if (Mathf.Abs(dirA - a) % 360 < angleTolerance) return dir;
        }
        return Direction.NONE;
    }

    List<GeoNode> neighbours = new List<GeoNode>();
    public Geography geography { get; set; }
    public float gizmoSize { get; set; }
    
    public enum Topology {
        None = 0,
        WorldInternal = 1,
        WorldEdge = 2,
        Land = 4,
        Water = 8,
        Shore = 16,
        Main = 32
    };

    public Topology topology
    {
        get
        {
            var topology = Topology.WorldInternal;
            if (neighbours.Count < 5)
            {
                topology = Topology.WorldEdge;
            }
            bool water = transform.position.y < 0;
            topology |= water ? Topology.Water : Topology.Land;
            bool shore = false;
            for (int i = 0, l=neighbours.Count; i<l; i++)
            {
                if ((neighbours[i].transform.position.y < 0) != water)
                {
                    shore = true;
                    break;
                }
            }
            topology |= shore ? Topology.Shore : Topology.Main;
            return topology;
        }
    }
    public void SetNeighbours(List<GeoNode> nodes)
    {
        if (neighbours.Count == 0)
        {
            neighbours.AddRange(nodes);
        } else
        {
            throw new System.InvalidOperationException(
                "Neighbours may only be set once"
            );
        }
    }

    public void AddNeighbour(GeoNode other)
    {
        if (other == this)
        {
            throw new System.ArgumentException(string.Format("Can't be neighbour to oneself, {0}", name));
        }
        if (!neighbours.Contains(other))
        {
            neighbours.Add(other);
            other.AddNeighbour(this);
        }        
    }

    public Vector2 PlanarPosition
    {
        get
        {
            return new Vector2(transform.position.x, transform.position.z);
        }
    }

    /**Vector pointing to other
     */
    public Vector2 PlanarOffset(GeoNode other)
    {
        var offset = other.transform.position - transform.position;
        return new Vector2(offset.x, offset.z);
    }

    public float PlanarDistance(GeoNode other)
    {
        Vector3 offset = other.transform.position - transform.position;
        offset.y = 0;
        return offset.magnitude;
    }

    public float avgPlanarDistance
    {
        get
        {
            if (neighbours.Count == 0)
            {
                throw new System.ArithmeticException("GeoNode lacks neighbours.");
            }
            Vector2 myPos = PlanarPosition;
            return neighbours
                .Select(node => Vector2.Distance(myPos, node.PlanarPosition))
                .Sum() / neighbours.Count;
        }
    }

    private void OnDrawGizmos()
    {
        var topo = this.topology;
        if (geography.showGeoNodeConnectionsGizmos)
        {
            Gizmos.color = Color.white;
            for (int i = 0, l = neighbours.Count; i < l; i++)
            {
                GeoNode neighbour = neighbours[i];
                Gizmos.DrawLine(transform.position, neighbour.transform.position);
            }
        }
        if (geography.showGeoNodeGizmos)
        {
            Gizmos.color = topo.HasFlag(Topology.Water) ? Color.blue : Color.green;
            Gizmos.DrawSphere(transform.position, gizmoSize * geography.gizmoSize);
        }
        if (geography.showGeoNodeEdgeGizmos)
        {
            Gizmos.color = Color.cyan;
            if (topo.HasFlag(Topology.WorldEdge))
            {
                Gizmos.DrawWireCube(transform.position, Vector3.one * gizmoSize * geography.gizmoSize);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!geography.showGeoNodeConnectionsGizmos)
        {
            Gizmos.color = Color.white;
            for (int i = 0, l = neighbours.Count; i < l; i++)
            {
                GeoNode neighbour = neighbours[i];
                Gizmos.DrawLine(transform.position, neighbour.transform.position);
            }
        }
    }

    public static GeoNode Spawn(Geography geography, Vector3 position, float gizmoSize)
    {
        var go = new GameObject();
        go.transform.SetParent(geography.transform);
        go.transform.position = position;
        var node = go.AddComponent<GeoNode>();
        node.geography = geography;
        node.gizmoSize = gizmoSize;
        return node;
    }

    public List<(Direction dir, GeoNode node)> GetNeighbours(Direction directionsFilter, float angleTolerance = 5)
    {
        var ret = new List<(Direction dir, GeoNode node)>();
        var neighbourAngles = neighbours
            .Select(node =>
            {
                var offset = PlanarOffset(node);
                return Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
            })
            .ToArray();
        for (int i=0; i<Extensions.Directions.Length; i++)        
        {
            var dir = Extensions.Directions[i];
            if (!directionsFilter.HasFlag(dir)) continue;
            
            bool foundNeighbour = false;
            float a = dir.AsAngle();
            for (int j=0; j < neighbourAngles.Length; j++)
            {
                if (Mathf.Abs(neighbourAngles[j] - a) % 360 < angleTolerance)
                {
                    ret.Add(( dir, neighbours[j]));
                    foundNeighbour = true;
                }
            }
            if (!foundNeighbour)
            {
                ret.Add((dir, null));
            }                
            
        }
        return ret;
    }

    public IEnumerable<GeoNode> GetNeighbours(System.Func<GeoNode, GeoNode, bool> filter)
    {
        return neighbours.Where(node => filter(this, node));
    }


    public GeoNode GetNeighbour(Direction direction, float angleTolerance = 5)
    {        
        var neighbourAngles = neighbours
            .Select(node =>
            {
                var offset = node.transform.position - transform.position;
                return Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;
            })
            .ToArray();
        
        float a = direction.AsAngle();
        for (int i = 0; i < neighbourAngles.Length; i++)
        {
            if (Mathf.Abs(neighbourAngles[i] - a) % 360 < angleTolerance)
            {
                return neighbours[i];
            }
        }
        return null;
    }

    public GeoNode GetRotationNeighbour(Rotation rotation, Direction inDirection)
    {
        var neighbourDirections = inDirection.Inverted().AllowedRotations(rotation);
        for (int i = 0; i< neighbourDirections.Length; i++)
        {
            var neigbour = GetNeighbour(neighbourDirections[i]);
            if (neigbour != null)
            {
                return neigbour;
            }
        }
        return null;
    }
}
