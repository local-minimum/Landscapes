using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LandscaperBase : MonoBehaviour
{

    public string description;
    public bool active = true;
    abstract protected void Landscape(Geography geography);

    public void Apply(Geography geography)
    {
        if (active) Landscape(geography);
    }
}
