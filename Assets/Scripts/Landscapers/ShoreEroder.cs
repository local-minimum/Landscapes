using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ShoreEroder : LandscaperBase
{
    [Range(0, 1)]
    public float erosionProbability = 0.1f;
    public AnimationCurve depths;
    public AnimationCurve erosionDistance;

    protected override IEnumerator<float> Landscape(Geography geography)
    {
        var nodes = geography
            .GetNodes(node => { var topo = node.topology; return topo.HasFlag(GeoNode.Topology.Shore) && topo.HasFlag(GeoNode.Topology.Land); })
            .Where(n => Random.value < erosionProbability)
            .ToArray();

        for (int i=0; i<nodes.Length; i++)
        {
            var node = nodes[i];
            node.Elevation = depths.Evaluate(Random.value);
            for (int j=0,n=Mathf.RoundToInt(erosionDistance.Evaluate(Random.value)); j<n; j++)
            {
                var neighbours = node
                    .GetNeighbours((curNode, neigh) => neigh.topology.HasFlag(GeoNode.Topology.Land))
                    .ToArray();
                if (neighbours.Length == 0) break;
                node = neighbours[Random.Range(0, neighbours.Length)];
                node.Elevation = depths.Evaluate(Random.value);
            }
            if (i % 100 == 0) yield return (float)i / nodes.Length;
        }
    }
}
