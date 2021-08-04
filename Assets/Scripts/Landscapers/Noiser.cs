using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Noiser : LandscaperBase
{
    public Vector3 magnitude = new Vector3(0.2f, 0.2f, 0.2f);

    protected override void Landscape(Geography geography)
    {
        for (int i=0, l=geography.NodeCount; i<l; i++)
        {
            var node = geography.GetNode(i);
            var noise = new Vector3(
                Random.Range(-magnitude.x/2, magnitude.x/2),
                Random.Range(-magnitude.y, magnitude.y),
                Random.Range(-magnitude.z/2, magnitude.z/2)
            );
            if (node.Elevation < 0)
            {
                noise.y = -Mathf.Abs(noise.y);
            } else
            {
                noise.y = Mathf.Abs(noise.y);
            }
            node.transform.position += noise;
        }
    }
}
