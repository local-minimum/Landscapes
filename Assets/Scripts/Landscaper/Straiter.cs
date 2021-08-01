using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Straiter : LandscaperBase
{
    protected override void Landscape(Geography geography)
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
        Debug.Log(string.Format("Found {0} seas. Sizes {1}", sea, string.Join(", ", seaSizes)));
    }
}
