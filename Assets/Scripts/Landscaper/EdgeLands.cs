using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EdgeLands : LandscaperBase
{
    public float connectionTolerance = 2f;
    GeoNode.Direction directionsFilter = GeoNode.Direction.E
        | GeoNode.Direction.S
        | GeoNode.Direction.W
        | GeoNode.Direction.N;
    protected override void Landscape(Geography geography)
    {
        System.Func<GeoNode, bool> filter = node =>
            node.topology == GeoNode.Topology.Edge && node.transform.position.y < 0;

        var waterEdges = geography.GetNodes(filter).ToArray();
        for (int i =0, l=waterEdges.Length; i<l; i++)
        {
            var node = waterEdges[i];
            var distance = node.avgPlanarDistance;
            var j = 0;
            while (true)
            {
                j++;
                if (j > 3) {
                    Debug.LogError(string.Format("Tried to add 4 edges to {0}. This should never be needed", node.name));
                    break;
                };
                var empty = node
                    .GetNeighbours(directionsFilter)
                    .Where(item => item.node == null)
                    .Select(item => (int)item.dir * 45f)
                    .ToArray();
                if (empty.Length == 0)
                {
                    break;
                }
                var angle = empty[0];
                var z = distance * Mathf.Sin(angle * Mathf.Deg2Rad);
                var x = distance * Mathf.Cos(angle * Mathf.Deg2Rad);
                var pos = new Vector3(x, 0, z) + node.transform.position;
                pos.y = 0;
                var newNode = GeoNode.Spawn(geography, pos, node.gizmoSize);
                geography.AddNode(newNode, node, distance * connectionTolerance);
            }                
        }
    }
}
