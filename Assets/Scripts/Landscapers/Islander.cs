using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Islander : LandscaperBase
{
    public int[] sizes = new int[] { 1, 1, 2, 3, 3, 3, 4, 5 };
    public int islands = 20;
    public AnimationCurve heightDistribution;    

    protected override IEnumerator<float> Landscape(Geography geography)
    {
        System.Func<GeoNode, bool> filter = node =>
        {
            var topology = node.topology;
            return topology.HasFlag(GeoNode.Topology.Water) && topology.HasFlag(GeoNode.Topology.Main);
        };
        var openSeaNodes = geography.GetNodes(filter).ToList();        
        System.Func<GeoNode, GeoNode, bool> openSeaNeighbours = (node, neighbour) =>
        {
            return openSeaNodes.Contains(neighbour);
        };

        for (int i=0; i<islands; i++)
        {
            var size = sizes[Random.Range(0, sizes.Length)];
            var origin = openSeaNodes[Random.Range(0, openSeaNodes.Count)];

            for (int j=0; j<size; j++)
            {
                origin.Elevation = heightDistribution.Evaluate(Random.value);
                openSeaNodes.Remove(origin);
                var neighbours = origin.GetNeighbours(openSeaNeighbours).ToArray();
                if (neighbours.Length == 0) break;
                origin = neighbours[Random.Range(0, neighbours.Length)];
            };

            if (i % 5 == 0) yield return (float) i / islands;
        }
    }
}
