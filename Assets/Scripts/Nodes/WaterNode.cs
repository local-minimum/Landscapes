using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterNode : NodeBase
{
    GeoNode seaFloor;
    float noWaterLevel = -0.05f;

    public float WaterDepth
    {
        get
        {
            return Mathf.Max(0, this.Elevation - seaFloor.Elevation);
        }

        set
        {
            if (value > 0)
            {
                this.Elevation = seaFloor.Elevation + value;
            } else
            {
                this.Elevation = seaFloor.Elevation + noWaterLevel;
            }
        }
    }
}
