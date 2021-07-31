using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class LandscaperBase : MonoBehaviour
{
    public abstract void Apply(Geography geography);
}
