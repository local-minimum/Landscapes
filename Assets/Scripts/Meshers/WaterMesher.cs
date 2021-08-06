using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class WaterMesher : MesherBase
{
    public float waterHeight = -0.05f;
    public Material material;

    public override void Create(Geography geography)
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        MeshRenderer mr = GetComponent<MeshRenderer>();
        MeshCollider mc = GetComponent<MeshCollider>();
        var mesh = new Mesh();
        mesh.name = "Water";
        var nodes = geography.GetNodes(n => true).ToArray();
        Vector3[] verts = nodes
            .Select(n => { var pos = n.transform.position; pos.y = waterHeight; return pos; })
            .ToArray();

        mesh.vertices = verts;
        mesh.triangles = GenerateTris(nodes).ToArray();
        mesh.RecalculateNormals();
        mf.mesh = mesh;
        if (material != null) mr.material = material;
        mc.sharedMesh = mesh;
    }
}
