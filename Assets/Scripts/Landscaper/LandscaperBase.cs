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
        
        if (active)
        {
            Debug.Log(string.Format("Applying {0}", description));
            Landscape(geography);
        } else
        {
            Debug.LogWarning(string.Format("Skipping {0}", description));
        }
    }
}
