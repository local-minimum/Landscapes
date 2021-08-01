using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Islander : LandscaperBase
{
    public int[] sizes = new int[] { 1, 1, 2, 3, 3, 3, 4, 5 };
    public int islands = 20;
    public AnimationCurve heightDistribution;    

    protected override void Landscape(Geography geography)
    {
        System.Func<GeoNode, GeoNode, bool> neighbourLandFilter = (node, neighbour) =>
        {
            return neighbour.transform.position.y > 0f;
        };
        System.Func<GeoNode, bool> filter = node =>
        {
            return node.transform.position.y < 0 && node.GetNeighbours(neighbourLandFilter).Count() == 0;
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
                origin.transform.position = GetLandPosition(origin);
                openSeaNodes.Remove(origin);
                var neighbours = origin.GetNeighbours(openSeaNeighbours).ToArray();
                if (neighbours.Length == 0) break;
                origin = neighbours[Random.Range(0, neighbours.Length)];
            };
        }
    }

    Vector3 GetLandPosition(GeoNode node)
    {
        Vector3 landPosition = node.transform.position;
        landPosition.y = heightDistribution.Evaluate(Random.value);
        return landPosition;

    }
}
