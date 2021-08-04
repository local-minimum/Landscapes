using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Shorer : LandscaperBase
{
    [Range(1, 22.5f)]
    public float angleTolerance = 10f;

    struct ShoreEdge
    {
        public GeoNode Land;
        public GeoNode Water;

        public ShoreEdge(GeoNode land, GeoNode water)
        {
            Land = land;
            Water = water;
        }

        public static IEnumerable<ShoreEdge> CCWShores(GeoNode land, float angleTolerance)
        {
            return land
                .GetNeighbours(Extensions.Directions.AsDirection(), angleTolerance)
                .Where(n => n.node != null && n.node.topology.HasFlag(GeoNode.Topology.Water))
                .Select(n => new ShoreEdge(land, n.node));
        }

        public bool Equals(ShoreEdge other)
        {
            return other.Land == Land && other.Water == Water;
        }
    }

    protected override void Landscape(Geography geography)
    {
        var visitedShores = new List<GeoNode>();
        var shores = geography.GetNodes(n => {
            var topo = n.topology;
            return topo.HasFlag(GeoNode.Topology.Shore) && topo.HasFlag(GeoNode.Topology.Land);
        }).ToArray();

        for (int i=0; i<shores.Length; i++)
        {
            var seed = shores[i];
            if (visitedShores.Contains(seed)) continue;
            MakeShore(seed, visitedShores);
        }
    }

    void MakeShore(GeoNode seed, List<GeoNode> visitedShores)
    {
        var node = seed;
        var edges = new List<ShoreEdge>();
        bool circled = false;
        var landNeighbours = node
            .GetNeighbours(Extensions.Directions.AsDirection())
            .Where(n => n.node != null)
            .Where(n => { var topo = n.node.topology; return topo.HasFlag(GeoNode.Topology.Shore) && topo.HasFlag(GeoNode.Topology.Land); })
            .ToArray();
        var direction = GeoNode.Direction.N;
        if (landNeighbours.Length > 0)
        {
            direction = landNeighbours[0].dir.Inverted();
        }
        
        //Debug.Log(string.Format("{0} -> {1} -> ?", node.name, direction));
        bool first = true;
        while (!circled)
        {
            bool foundLand = false;
            bool foundWater = false;
            var directions = direction.Inverted().CCWFrom(true);            
            for (int i = 0, l = directions.Count; i<l ; i++)
            {
                var curDirection = directions[i];
                var neighbour = node.GetNeighbour(curDirection, angleTolerance);
                //Debug.Log(string.Format("{0} -> {1} -> {2}", node.name, curDirection, neighbour));
                if (neighbour == null) continue;
                if (neighbour.Is(Geography.NodeFilter.Water))
                {
                    var edge = new ShoreEdge(node, neighbour);
                    if (edges.Contains(edge))
                    {
                        circled = true;
                        break;
                    } else
                    {
                        edges.Add(edge);
                        foundWater = true;
                    }
                } else if (foundWater)
                {
                    var topo = neighbour.topology;
                    if (topo.HasFlag(GeoNode.Topology.Shore))
                    {
                        node = neighbour;
                        direction = curDirection;
                        foundLand = true;
                        break;
                    }
                }
            }
            if (first && !foundLand)
            {
                // This is a one point island
                circled = true;
            }
            visitedShores.Add(node);
            if (!foundLand && !circled)
            {
                Debug.LogError(string.Format("Shore disappeared at {0}", node.name));
                throw new System.Exception();
            }
            first = false;
        }
        Debug.Log(string.Format("Shore length {0}.", edges.Count));
    }
}
