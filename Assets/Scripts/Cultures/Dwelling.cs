using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class Dwelling : MonoBehaviour
{

    private void OnEnable()
    {
        Geography.OnWorldReady += Geography_OnWorldReady;
    }

    private void OnDisable()
    {
        Geography.OnWorldReady -= Geography_OnWorldReady;
    }
    private void Geography_OnWorldReady(Geography geography)
    {
        CreateMesh();
    }

    void CreateMesh()
    {
        var mf = GetComponent<MeshFilter>();
        var mesh = new Mesh();
        mesh.name = "Dwelling";
        mesh.vertices = new Vector3[]
        {
                new Vector3 (-1, 0, -1),
                new Vector3 (1, 0, -1),
                new Vector3 (1, 3, -1),
                new Vector3 (-1, 3, -1),
                new Vector3 (-1, 3, 1),
                new Vector3 (1, 3, 1),
                new Vector3 (1, 0, 1),
                new Vector3 (-1, 0, 1),
        };
        mesh.triangles = new int[]
        {
                0, 2, 1, //face front
                0, 3, 2,
                2, 3, 4, //face top
                2, 4, 5,
                1, 2, 5, //face right
                1, 5, 6,
                0, 7, 4, //face left
                0, 4, 3,
                5, 4, 7, //face back
                5, 7, 6,
                0, 6, 7, //face bottom
                0, 1, 6
        };
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();        
        mf.sharedMesh = mesh;
        var mr = GetComponent<MeshRenderer>();
        mr.material = Resources.Load<Material>("DwellingMatA");
        
    }
}
