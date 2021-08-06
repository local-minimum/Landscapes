using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EdgeLands : LandscaperBase
{
    public Gridder gridder;
    public float connectionTolerance = 2f;
    GeoNode.Direction spawnFilter = GeoNode.Direction.E
        | GeoNode.Direction.S
        | GeoNode.Direction.W
        | GeoNode.Direction.N;

    GeoNode.Direction connectionsFilter = GeoNode.Direction.E
        | GeoNode.Direction.S
        | GeoNode.Direction.W
        | GeoNode.Direction.N
        | GeoNode.Direction.E
        | GeoNode.Direction.SE
        | GeoNode.Direction.NW;

    protected override IEnumerator<float> Landscape(Geography geography)
    {
        System.Func<GeoNode, bool> filter = node =>
        {
            var topology = node.topology;
            return topology.HasFlag(GeoNode.Topology.WorldEdge) && topology.HasFlag(GeoNode.Topology.Water);
        };

        var waterEdges = geography.GetNodes(filter).ToArray();
        for (int i =0, l=waterEdges.Length; i<l; i++)
        {
            var node = waterEdges[i];
            var refDistance = gridder == null ? node.AveragePlanarDistance : gridder.spacing;
            var refDiagDistance = Mathf.Sqrt(2 * Mathf.Pow(refDistance, 2));

            var cardinalNeighbour = node
                .GetNeighbours(spawnFilter)
                .ToArray();

            for (int j = 0; j<cardinalNeighbour.Length; j++)
            {
                var neighbour = cardinalNeighbour[j];
                var newNode = neighbour.node == null ? AddNode(geography, node, neighbour.dir, refDistance) : neighbour.node;
                var rotatedEdges = neighbour.dir.WithNeighbours(2);
                for (int k = 0; k < 3; k++)
                {
                    var dir = rotatedEdges[k * 2];
                    if (newNode.GetNeighbour(dir) == null)
                    {
                        AddNode(geography, newNode, dir, refDistance);
                    }
                }
            }
            if (i % 20 == 0) yield return (float)i / l;
        }        
    }

    GeoNode AddNode(Geography geography, GeoNode node, GeoNode.Direction direction, float distance)
    {
        var edgeRad = direction.AsAngle() * Mathf.Deg2Rad;        
        var z = distance * Mathf.Sin(edgeRad);
        var x = distance * Mathf.Cos(edgeRad);
        var pos = new Vector3(x, 0, z) + node.transform.position;
        pos.y = 0;
        var newNode = GeoNode.Spawn(geography, pos, node.gizmoSize);
        geography.AddNode(newNode, node, distance * connectionTolerance, connectionsFilter);
        return newNode;
    }
}
