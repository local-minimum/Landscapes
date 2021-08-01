using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class GeoNode : MonoBehaviour
{
    public enum Direction { E, NE, N, NW, W, SW, S, SE, NONE };
    public enum Rotation { CW, CCW };

    public static Direction DirectionFromNodes(GeoNode from, GeoNode to, float angleTolerance = 5)
    {
        var offset = to.transform.position - from.transform.position;
        var a = Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;
        foreach (Direction d in System.Enum.GetValues(typeof(Direction)))
        {
            if (d == Direction.NONE) continue;
            var dirA = (int)d * 45f;
            if (Mathf.Abs(dirA - a) % 360 < angleTolerance) return d;
        }
        return Direction.NONE;
    }

    public static Direction FlipDirection(Direction direction)
    {
        if (direction == Direction.NONE) return Direction.NONE;
        var i = (int)direction - 4;
        if (i < 0) i += 8;
        return (Direction)i;
    }

    public enum Topology { Internal, Edge };
    List<GeoNode> neighbours = new List<GeoNode>();
    public Geography geography { get; set; }
    public float gizmoSize { get; set; }

    public Topology topology
    {
        get
        {
            if (neighbours.Count < 5)
            {
                return Topology.Edge;
            }
            return Topology.Internal;
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
            Vector2 myPos = new Vector2(transform.position.x, transform.position.z);
            return neighbours
                .Select(node => {
                    return Vector2.Distance(myPos, new Vector2(node.transform.position.x, node.transform.position.z));
                })
                .Sum() / neighbours.Count;
        }
    }

    private void OnDrawGizmos()
    {
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
            Gizmos.color = transform.position.y < 0 ? Color.blue : Color.green;
            Gizmos.DrawSphere(transform.position, gizmoSize);
        }
        if (geography.showGeoNodeEdgeGizmos)
        {
            Gizmos.color = Color.cyan;
            if (topology == Topology.Edge)
            {
                Gizmos.DrawWireCube(transform.position, Vector3.one * gizmoSize);
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
                var offset = node.transform.position - transform.position;
                return Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;
            })
            .ToArray();
        foreach (Direction d in System.Enum.GetValues(typeof(Direction)))
        {
            if (directionsFilter.HasFlag(d))
            {
                bool foundNeighbour = false;
                float a = (int) d * 45;
                for (int i=0; i< neighbourAngles.Length; i++)
                {
                    if (Mathf.Abs(neighbourAngles[i] - a) % 360 < angleTolerance)
                    {
                        ret.Add(( d, neighbours[i]));
                        foundNeighbour = true;
                    }
                }
                if (!foundNeighbour)
                {
                    ret.Add((d, null));
                }                
            }
        }
        return ret;
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
        
        float a = (int)direction * 45;
        for (int i = 0; i < neighbourAngles.Length; i++)
        {
            if (Mathf.Abs(neighbourAngles[i] - a) % 360 < angleTolerance)
            {
                Debug.Log(string.Format("{4} ({5}): {0} - {1} | {2} < {3}", neighbourAngles[i], a, Mathf.Abs(neighbourAngles[i] - a) % 360, angleTolerance, name, direction));
                return neighbours[i];
            }
        }
        Debug.Log(string.Format("{0}: No neighbour {1}", name, direction));
        return null;
    }

    public GeoNode GetRotationNeighbour(Rotation rotation, Direction inDirection)
    {
        var neighbourDirections = FlipDirection(inDirection).AllowedRotations(rotation);
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
