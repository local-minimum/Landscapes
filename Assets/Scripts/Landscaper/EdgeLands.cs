using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EdgeLands : LandscaperBase
{
    public float connectionTolerance = 2f;
    protected override void Landscape(Geography geography)
    {
        System.Func<GeoNode, bool> filter = node =>
            node.topology == GeoNode.Topology.Edge && node.transform.position.y < 0;

        var waterEdges = geography.GetNodes(filter).ToArray();
        Debug.Log(string.Format("{0} water edges", waterEdges.Length));
        // Figure out open cardinals
        // Figure out AddNode
        geography.AddNode(
            GeoNode.Spawn(geography, waterEdges[0].transform.position, waterEdges[0].gizmoSize),
            waterEdges[0],
            waterEdges[0].avgPlanarDistance * connectionTolerance
        );
    }

}
