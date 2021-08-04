using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class LandMesher : MesherBase
{
    public Material material;

    public override void Create(Geography geography)
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        MeshRenderer mr = GetComponent<MeshRenderer>();
        var mesh = new Mesh();
        mesh.name = "Landmass";
        var nodes = geography.GetNodes(n => true).ToArray();
        Vector3[] verts = nodes
            .Select(n => n.transform.position)
            .ToArray();

        mesh.vertices = verts;
        mesh.triangles = GenerateTris(nodes).ToArray();
        mesh.RecalculateNormals();
        mf.mesh = mesh;        
        if (material != null) mr.material = material;
    }

    List<int> GenerateTris(GeoNode[] nodes)
    {
        var paths = new Dictionary<GeoNode, HashSet<GeoNode>>();
        var idxLookup = new Dictionary<GeoNode, int>();
        for (int i=0;i<nodes.Length; i++)
        {
            idxLookup[nodes[i]] = i;
        }        
        var tris = new List<int>();
        for (int i=0; i<nodes.Length;i++)
        {
            var node = nodes[i];            
            if (!paths.ContainsKey(node)) { paths[node] = new HashSet<GeoNode>(); }
            var neighbours = node
                .GetNeighbours((n, neighbour) => !paths[node].Contains(neighbour))
                .ToArray();
            for (int j=0; j<neighbours.Length; j++)
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
