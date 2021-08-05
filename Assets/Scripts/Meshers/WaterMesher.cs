using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class WaterMesher : MesherBase
{
    public Material material;

    public override void Create(Geography geography)
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        MeshRenderer mr = GetComponent<MeshRenderer>();
        var mesh = new Mesh();
        mesh.name = "Water";
        var nodes = geography.GetNodes(n => true).ToArray();
        Vector3[] verts = nodes
            .Select(n => { var pos = n.transform.position; pos.y = 0; return pos; })
            .ToArray();

        mesh.vertices = verts;
        mesh.triangles = GenerateTris(nodes).ToArray();
        mesh.RecalculateNormals();
        mf.mesh = mesh;
        if (material != null) mr.material = material;
    }
}
