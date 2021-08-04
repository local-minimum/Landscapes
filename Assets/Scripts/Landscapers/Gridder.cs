using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gridder : LandscaperBase
{
    public int width = 30;
    public int height = 20;
    public float spacing = 1;

    protected override void Landscape(Geography geography)
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
                geography.AddNodeUnsafe(GeoNode.Spawn(geography, pos, spacing / 5f));
            }
        }
    }

    void ConnectGrid(Geography geography)
    {
        for (int x=0; x< width; x++)
        {
            for (int y=0; y < height; y++)
            {
                int pos = x * height + y;
                var node = geography.GetNode(pos);
                if (x > 0)
                {
                    node.AddNeighbour(geography.GetNode((x - 1) * height + y));
                    if (y < height - 1)
                    {
                        node.AddNeighbour(geography.GetNode((x - 1) * height + y + 1));
                    }
                }
                if (y > 0)
                {
                    node.AddNeighbour(geography.GetNode(x * height + y - 1));
                }                
            }
        }
    }
}
