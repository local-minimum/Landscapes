using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class LandMesher : MesherBase
{
    public Material material;

    public override void Create(Geography geography)
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        MeshRenderer mr = GetComponent<MeshRenderer>();
        MeshCollider mc = GetComponent<MeshCollider>();
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
        mc.sharedMesh = mesh;
    }

}
