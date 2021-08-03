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
        var direction = GeoNode.Direction.N;
        while (!circled)
        {
            // TODO list of directions based on in direction
            for (int i = 0; i < Extensions.Directions.Length; i++)
            {
                var neighbour = node.GetNeighbour(Extensions.Directions[i], angleTolerance);
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
                    }
                } else
                {
                    node = neighbour;
                    break;
                }
            }
            visitedShores.Add(node);
        }
    }
}
