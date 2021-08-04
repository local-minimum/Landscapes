using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Ranger : LandscaperBase
{
    [Range(0, 1)]
    public float continuationProbability = 0.99f;
    [Range(0, 1)]
    public float continuationProbDecay = 0.05f;
    public float magnitude = 1f;
    public int ranges = 10;

    public Geography.NodeFilter filter;

    protected override void Landscape(Geography geography)
    {
        var nodes = geography.GetNodes(filter).ToArray();        
        for (int i = 0; i<ranges; i++)
        {
            var node = nodes[Random.Range(0, nodes.Length)];
            MakeRange(node);
        }
    }


    void MakeRange(GeoNode seed)
    {
        var direction = Extensions.Directions[Random.Range(0, Extensions.Directions.Length)];
        var length = 1;
        var node = seed;
        float prevElevation = 0;
        do
        {
            float nextElevation = Mathf.Lerp(prevElevation, Random.value * magnitude, Random.value);
            node.Elevation += nextElevation;
            prevElevation = nextElevation;
            var neighbours = node
                .GetNeighbours(direction.WithNeighbours(1).AsDirection())
                .Where(n => n.node != null && n.node.Is(filter))                
                .ToArray();
            if (neighbours.Length == 0) return;
            (direction, node) = neighbours[Random.Range(0, neighbours.Length)];
        } while (Random.value < continuationProbability - length * continuationProbDecay);

    }
}
