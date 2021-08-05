using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LandscaperBase : MonoBehaviour
{

    public string description;
    public bool active = true;
    abstract protected IEnumerator<float> Landscape(Geography geography);

    public IEnumerator<float> Apply(Geography geography)
    {        
        if (active)
        {
            Debug.Log(string.Format("Applying {0}", description));
            var enumerator = Landscape(geography);
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }            
        } else
        {
            Debug.LogWarning(string.Format("Skipping {0}", description));
        }
    }
}
