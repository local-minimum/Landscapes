using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class LandscaperBase : MonoBehaviour
{
    [SerializeField]
    public bool active = true;
    abstract protected void Landscape(Geography geography);

    public void Apply(Geography geography)
    {
        if (active) Landscape(geography);
    }
}
