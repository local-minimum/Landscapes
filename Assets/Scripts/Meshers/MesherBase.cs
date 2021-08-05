using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class MesherBase : MonoBehaviour
{
    abstract public void Create(Geography geography);

    protected List<int> GenerateTris(GeoNode[] nodes)
    {
        var paths = new Dictionary<GeoNode, HashSet<GeoNode>>();
        var idxLookup = new Dictionary<GeoNode, int>();
        for (int i = 0; i < nodes.Length; i++)
        {
            idxLookup[nodes[i]] = i;
        }
        var tris = new List<int>();
        for (int i = 0; i < nodes.Length; i++)
        {
            var node = nodes[i];
            if (!paths.ContainsKey(node)) { paths[node] = new HashSet<GeoNode>(); }
            var neighbours = node
                .GetNeighbours((n, neighbour) => !paths[node].Contains(neighbour))
                .ToArray();
            for (int j = 0; j < neighbours.Length; j++)
            {
                var neighbour = neighbours[j];
                var thirdNode = neighbour.GetRotationNeighbour(GeoNode.Rotation.CCW, node);
                if (thirdNode != null)
                {
                    tris.Add(i);
                    tris.Add(idxLookup[neighbour]);
                    tris.Add(idxLookup[thirdNode]);
                    paths[node].Add(neighbour);
                }
            }
        }
        return tris;
    }
}
