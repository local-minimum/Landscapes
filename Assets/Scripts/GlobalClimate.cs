using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalClimate : MonoBehaviour
{
    [Tooltip("As cool as it can get")]
    public float referenceTemperature = -42f;

    [Tooltip("Min proportion of sun energy gotten at surfance at 150 deg offset")]
    public float maxLatitudeOffsetFactor = 0.1f;

    public float surfaceCoolingFactor = 0.01f;

    public static GlobalClimate instance { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        } else if (instance != this)
        {
            Destroy(this);
        }
    }
}
