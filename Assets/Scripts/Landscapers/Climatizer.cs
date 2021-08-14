using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Climatizer : LandscaperBase
{
    protected override IEnumerator<float> Landscape(Geography geography)
    {
        for (int i = 0, l=geography.NodeCount; i<l; i++)
        {
            var node = geography.GetNode(i);
            var climate = node.gameObject.AddComponent<Climatology>();
            //TODO: calc distance to shore and full neigbours;
            if (i % 1000 == 0) yield return (float)i / l;
        }
    }
}
