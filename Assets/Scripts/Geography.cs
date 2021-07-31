using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Geography : MonoBehaviour
{
    public int width = 30;
    public int height = 20;
    public float spacing = 1;

    List<GeoNode> nodes = new List<GeoNode>();

    private void Start()
    {
        CreateGrid();
        ConnectGrid();
    }

    void CreateGrid()
    {
        float halfWidth = width / 2.0f;
        float halfHeight = height / 2.0f;
        for (int x = 0; x<width; x++)
        {
            for (int z=0; z<height;z++)
            {
                Vector3 pos = new Vector3(
                    transform.position.x - (x - halfWidth) * spacing,
                    transform.position.y,
                    transform.position.z - (z - halfHeight) * spacing
                );
                nodes.Add(SpawnGeoNode(pos));
            }
        }
    }

    GeoNode SpawnGeoNode(Vector3 position)
    {
        var go = new GameObject(string.Format("GeoNode #{0}", nodes.Count));
        go.transform.SetParent(transform);
        go.transform.position = position;
        var node = go.AddComponent<GeoNode>();
        node.geography = this;
        node.gizmoSize = spacing / 5f;
        return node;
    }

    void ConnectGrid()
    {
        float connectLengthSq = 2.001f * spacing * spacing;
        int nNodes = nodes.Count;
        for (int i=0; i<nNodes; i++)
        {
            GeoNode node = nodes[i];
            List<GeoNode> neighbours = new List<GeoNode>();
            for (int j=0; j<nNodes; j++)
            {
                if (i == j) continue;
                if (Vector3.SqrMagnitude(nodes[j].transform.position - node.transform.position) < connectLengthSq)
                {
                    GeoNode other = nodes[j];
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
