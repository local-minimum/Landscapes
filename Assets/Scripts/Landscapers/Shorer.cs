using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Shorer : LandscaperBase
{
    [Range(1, 22.5f)]
    public float angleTolerance = 10f;

    [Range(0, 1)]
    public float maxLength = 0.35f;
    [Range(0, 1)]
    public float minLength = 0.05f;
    
    public float gizmoSize = 1f;

    struct ShoreEdge
    {
        public GeoNode Land;
        public GeoNode Water;

        public ShoreEdge(GeoNode land, GeoNode water)
        {
            Land = land;
            Water = water;
        }

        public bool Equals(ShoreEdge other)
        {
            return other.Land == Land && other.Water == Water;
        }

        public Vector3 InterpolatedShorePoint(float t)
        {
            var off = Land.PlanarOffset(Water) * Mathf.Clamp01(t);
            return new Vector3(Land.transform.position.x + off.x, 0, Land.transform.position.z + off.y);
        }

        public void Inject(GeoNode other)
        {
            Land.RemoveNeighbour(Water);
            other.AddNeighbour(Land);
            other.AddNeighbour(Water);
        }

        public GeoNode[] Triangle(ShoreEdge other)
        {
            if (Land == other.Land && Water.HasNeighbour(other.Water))
            {
                return new GeoNode[] { Water, Land, other.Water };
            } else if (Water == other.Water && Land.HasNeighbour(other.Land))
            {
                return new GeoNode[] { Land, Water, other.Land };
            }
            throw new System.ArgumentException(string.Format(
                "{0} and {1} don't form a triangle",
                this,
                other
            ));
        }

        public float PlanarDistance
        {
            get
            {
                return Water.PlanarDistance(Land);
            }
        }
    }

    protected override IEnumerator<float> Landscape(Geography geography)
    {
        var visitedShores = new List<GeoNode>();
        var shores = geography.GetNodes(n => {
            var topo = n.topology;
            return topo.HasFlag(GeoNode.Topology.Shore) && topo.HasFlag(GeoNode.Topology.Land);
        }).ToArray();
        var j = 0;
        for (int i=0; i<shores.Length; i++)
        {
            var seed = shores[i];
            if (visitedShores.Contains(seed)) continue;
            var shoreEdges = CollectShoreEdges(seed, visitedShores);
            MakeShore(geography, shoreEdges);            
            if (j % 5 == 0) yield return (float) i / shores.Length;
            j++;
        }
    }

    List<ShoreEdge> CollectShoreEdges(GeoNode seed, List<GeoNode> visitedShores)
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
            }
            first = false;
        }
        //Debug.Log(string.Format("Shore length {0}.", edges.Count));
        return edges;
    }

    void MakeShore(Geography geography, List<ShoreEdge> edges)
    {
        if (edges.Count < 2) return;
        var prevEdge = edges[0];
        var prevNode = GeoNode.Spawn(geography, prevEdge.InterpolatedShorePoint(Random.Range(minLength, maxLength)), gizmoSize);
        var firstNode = prevNode;
        geography.AddNodeUnsafe(prevNode);
        prevEdge.Inject(prevNode);

        for (int i=1, l=edges.Count; i<l; i++)
        {
            var edge = edges[i];
            var triangle = edge.Triangle(prevEdge);
            var node = GeoNode.Spawn(geography, edge.InterpolatedShorePoint(Random.Range(minLength, maxLength)), gizmoSize);
            geography.AddNodeUnsafe(node);
            edge.Inject(node);

            var internalNode = MakeTriangleInternalNode(geography, triangle, Mathf.Min(edge.PlanarDistance, prevEdge.PlanarDistance));
            internalNode.AddNeighbour(node);
            internalNode.AddNeighbour(prevNode);
            prevNode = node;
            prevEdge = edge;
        }

        var finalNode = MakeTriangleInternalNode(geography, prevEdge.Triangle(edges[0]), Mathf.Min(edges[0].PlanarDistance, prevEdge.PlanarDistance));
        finalNode.AddNeighbour(prevNode);
        finalNode.AddNeighbour(firstNode);
    }

    GeoNode MakeTriangleInternalNode(Geography geography, GeoNode[] triangle, float refEdgeLength)
    {
        var shared = triangle[1];
        var off1 = shared.PlanarOffset(triangle[0]);
        var off2 = shared.PlanarOffset(triangle[2]);
        var dist = (shared.Is(Geography.NodeFilter.Water) ? 1 - Random.Range(minLength, maxLength) : Random.Range(minLength, maxLength)) * refEdgeLength;
        var pos = (off1 + off2).normalized * dist;
        var internalNode = GeoNode.Spawn(geography, new Vector3(shared.transform.position.x + pos.x, 0, shared.transform.position.z + pos.y), gizmoSize);
        geography.AddNodeUnsafe(internalNode);
        for (int j = 0; j < 3; j++)
        {
            internalNode.AddNeighbour(triangle[j]);
        }
        return internalNode;
    }
}
