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
            node.transform.position += new Vector3(
                Random.Range(-magnitude.x, magnitude.x),
                Random.Range(-magnitude.y, magnitude.y),
                Random.Range(-magnitude.z, magnitude.z)
            );
        }
    }
}
