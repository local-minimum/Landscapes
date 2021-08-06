using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class GeoNode : NodeBase
{    
    public Geography geography { get; set; }
    public float gizmoSize { get; set; }
    public static GeoNode Spawn(Geography geography, Vector3 position, float gizmoSize)
    {
        var go = new GameObject();
        go.transform.SetParent(geography.nodesParent.transform);
        go.transform.position = position;
        var node = go.AddComponent<GeoNode>();
        node.geography = geography;
        node.gizmoSize = gizmoSize;
        return node;
    }

    public bool Is(Geography.NodeFilter filter)
    {
        switch (filter)
        {
            case Geography.NodeFilter.Any:
                return true;
            case Geography.NodeFilter.Land:
                return Elevation >= 0;
            case Geography.NodeFilter.Water:
                return Elevation < 0;
            case Geography.NodeFilter.ZeroOrWater:
                return Elevation <= 0;
        }
        throw new System.NotImplementedException(string.Format("{0} not implemented", filter));
    }

    #region Gizmos
    private void OnDrawGizmos()
    {
        if (!geography.enableGizmos) return;
        var topo = this.topology;
        if (geography.showGeoNodeConnectionsGizmos)
        {
            Gizmos.color = Color.white;
            for (int i = 0, l = neighbours.Count; i < l; i++)
            {
                var neighbour = neighbours[i];
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
        if (!geography.enableGizmos) return;
        if (!geography.showGeoNodeConnectionsGizmos)
        {
            Gizmos.color = Color.white;
            for (int i = 0, l = neighbours.Count; i < l; i++)
            {
                var neighbour = neighbours[i];
                Gizmos.DrawLine(transform.position, neighbour.transform.position);
            }
        }
    }
    #endregion

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
                if (Mathf.Abs(Mathf.DeltaAngle(neighbourAngles[j], a)) < angleTolerance)
                {
                    ret.Add(( dir, (GeoNode) neighbours[j]));
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
        return neighbours
            .Where(node => filter(this, (GeoNode) node))
            .Cast<GeoNode>();
    }


    public GeoNode GetNeighbour(Direction direction, float angleTolerance = 5)
    {        
        var neighbourAngles = neighbours
            .Select(node =>
            {                
                var offset = PlanarOffset(node);
                return Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
            })
            .ToArray();
        
        float a = direction.AsAngle();
        for (int i = 0; i < neighbourAngles.Length; i++)
        {
            if (Mathf.Abs(Mathf.DeltaAngle(neighbourAngles[i], a)) < angleTolerance)
            {
                return (GeoNode) neighbours[i];
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

    public GeoNode GetRotationNeighbour(Rotation rotation, GeoNode inNeighbour)
    {
        int step = 2 * ((int)rotation) - 1;
        int idx = neighbours.IndexOf(inNeighbour);
        if (idx < 0) return null;
        idx += step;
        var n = neighbours.Count;
        if (idx < 0)
        {
            idx += n;
        } else if (idx >= n)
        {
            idx -= n;
        }
        var outNeighbour = neighbours[idx];
        if (outNeighbour == inNeighbour) return null;
        return (GeoNode) outNeighbour;
    }
}
