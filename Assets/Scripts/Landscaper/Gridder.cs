using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gridder : LandscaperBase
{
    public int width = 30;
    public int height = 20;
    public float spacing = 1;

    public override void Apply(Geography geography)
    {
        CreateGrid(geography);
        ConnectGrid(geography);
    }


    void CreateGrid(Geography geography)
    {
        float halfWidth = width / 2.0f;
        float halfHeight = height / 2.0f;
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 pos = new Vector3(
                    geography.transform.position.x - (x - halfWidth) * spacing,
                    geography.transform.position.y,
                    geography.transform.position.z - (z - halfHeight) * spacing
                );
                geography.AddNode(SpawnGeoNode(geography, pos));
            }
        }
    }

    GeoNode SpawnGeoNode(Geography geography, Vector3 position)
    {
        var go = new GameObject();
        go.transform.SetParent(geography.transform);
        go.transform.position = position;
        var node = go.AddComponent<GeoNode>();
        node.geography = geography;
        node.gizmoSize = spacing / 5f;
        return node;
    }

    void ConnectGrid(Geography geography)
    {
        float connectLengthSq = 2.001f * spacing * spacing;
        int nNodes = geography.NodeCount;
        for (int i = 0; i < nNodes; i++)
        {
            GeoNode node = geography.GetNode(i);
            List<GeoNode> neighbours = new List<GeoNode>();
            for (int j = 0; j < nNodes; j++)
            {
                if (i == j) continue;
                GeoNode other = geography.GetNode(j);
                if (Vector3.SqrMagnitude(other.transform.position - node.transform.position) < connectLengthSq)
                {
                    if (other.transform.position.z < node.transform.position.z && other.transform.position.x < node.transform.position.x)
                    {
                        continue;
                    }
                    neighbours.Add(other);
                }
            }
            node.SetNeighbours(neighbours);
        }
    }

}
