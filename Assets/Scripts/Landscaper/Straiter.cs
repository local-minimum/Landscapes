using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Straiter : LandscaperBase
{
    public float straitProbability = 1f;

    protected override void Landscape(Geography geography)
    {
        var (seaLookup, seaSizes) = LabelSeas(geography);        
        if (seaSizes.Count <= 1) return;
        var centerOfSeas = CalculateCenterOfSeas(seaLookup, seaSizes);
        MakeStraits(seaLookup, seaSizes, centerOfSeas);
        Debug.Log(string.Format("Found {0} seas. Sizes {1}", seaSizes.Count, string.Join(", ", seaSizes)));
    }

    (Dictionary<GeoNode, int> lookup, List<int> seaSizes) LabelSeas(Geography geography)
    {
        Dictionary<GeoNode, int> seaLookup = new Dictionary<GeoNode, int>();
        var stack = new Queue<GeoNode>();
        System.Func<GeoNode, bool> seaFilter = node => node.transform.position.y < 0f;
        var seaNodes = geography.GetNodes(seaFilter).ToList();
        System.Func<GeoNode, GeoNode, bool> filter = (node, neighbour) =>
        {
            return neighbour.transform.position.y < 0 && !seaLookup.ContainsKey(neighbour) && !stack.Contains(neighbour);
        };
        int sea = 0;
        List<int> seaSizes = new List<int>();

        while (seaNodes.Count > 0)
        {
            sea++;
            var seaSize = 0;
            var node = seaNodes[0];
            stack.Enqueue(node);
            while (stack.Count > 0)
            {
                seaSize++;
                node = stack.Dequeue();
                seaNodes.Remove(node);
                seaLookup[node] = sea;
                foreach (var neighbour in node.GetNeighbours(filter))
                {
                    stack.Enqueue(neighbour);
                }
            }
            seaSizes.Add(seaSize);
        }
        return (seaLookup, seaSizes);
    }

    List<Vector2> CalculateCenterOfSeas(Dictionary<GeoNode, int> seaLookup, List<int> seaSizes)
    {
        var centerOfSeas = new List<Vector2>();
        for (int i = 1, l=seaSizes.Count; i <= l; i++)
        {
            var center = seaLookup
                .Where(kvp => kvp.Value == i)
                .Select(kvp => kvp.Key.PlanarPosition)
                .Aggregate(Vector2.zero, (a, b) => a + b) / seaSizes[i - 1];
            centerOfSeas.Add(center);
        }
        return centerOfSeas;
    }

    void MakeStraits(Dictionary<GeoNode, int> seaLookup, List<int> seaSizes, List<Vector2> centerOfSeas)
    {
        var sea = seaSizes.Count;
        // If there's only 1 sea no straits to be made
        while (sea > 1)
        {
            if (Random.value > straitProbability) continue;
            var attractor = GetStraitAttractor(centerOfSeas, sea);
            sea--;
        }
    }

    Vector2 GetStraitAttractor(List<Vector2> centerOfSeas, int sea)
    {
        int targetIdx = 0;
        float bestSqDist = Vector2.SqrMagnitude(centerOfSeas[sea - 1] - centerOfSeas[targetIdx]);
        // Sea is our sea we try to connect from so no need to calculate it (and indexes are one less
        for (int i = 1; i < sea - 1; i++)
        {
            float sqDist = Vector2.SqrMagnitude(centerOfSeas[sea - 1] - centerOfSeas[i]);
            if (sqDist < bestSqDist)
            {
                targetIdx = i;
                bestSqDist = sqDist;
            }
        }
        return centerOfSeas[targetIdx];
    }

    List<GeoNode> StraitSourceCandidates(Vector2 sourceSeaCenter, Vector2 targetSeaCenter, Dictionary<GeoNode, int> seaLookup, int sourceSea)
    {        
        var refSqDist = Vector2.SqrMagnitude(sourceSeaCenter - targetSeaCenter);
        return seaLookup
            .Where(kvp => kvp.Value == sourceSea)
            .Select(kvp => kvp.Key)
            .ToList();
    }
}
