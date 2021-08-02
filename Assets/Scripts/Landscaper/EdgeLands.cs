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

    protected override void Landscape(Geography geography)
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
            var refDistance = gridder == null ? node.avgPlanarDistance : gridder.spacing;
            var refDiagDistance = Mathf.Sqrt(2 * Mathf.Pow(refDistance, 2));

            var empty = node
                .GetNeighbours(spawnFilter)
                .Where(item => item.node == null)
                .ToArray();
            for (int j = 0; j<empty.Length; j++)
            {
                var edge = empty[j];
                var edgeRad = edge.dir.AsAngle() * Mathf.Deg2Rad;                
                var distance = edge.dir.IsCardinal() ? refDistance : refDiagDistance;
                var z = distance * Mathf.Sin(edgeRad);
                var x = distance * Mathf.Cos(edgeRad);
                var pos = new Vector3(x, 0, z) + node.transform.position;
                pos.y = 0;                
                var newNode = GeoNode.Spawn(geography, pos, node.gizmoSize);
                geography.AddNode(newNode, node, refDistance * connectionTolerance, connectionsFilter);                                
            }
        }
    }
}
