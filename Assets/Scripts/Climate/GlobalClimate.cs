using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalClimate : MonoBehaviour
{
    [Tooltip("Regions average annual temperature")]
    public float annualAverageTemperatur = 15f;
    public float refTempSeedingNoise = 0.5f;

    [Tooltip("Min proportion of sun energy gotten at surfance at 150 deg offset")]
    public float maxLatitudeOffsetFactor = 0.1f;

    // Deprecated
    public float surfaceCoolingFactor = 0.01f;
    
    // atmosphereSunAngleCoeff just a guess
    public float atmosphereSunAngleCoeff = 0.5f;
    public float atmosphereReflection = 0.06f;
    public float atmosphereAbsorpitonClear = 0.16f;
    public float atmosphereRadiation = 0.64f;

    public float cloudReflection = 0.2f;
    // cloudAbsorptionFactor just a guess
    public float cloudAbsorptionFactor = 2f;

    public float groundAbsorpiton = 0.7f;
    public float groundRadiation = 0.4f;
    public float groundSpaceRadiation = 0.06f;

    public EnergyProperties air = new EnergyProperties(1.006f, 0.00125f, 5e18f / 5.1e14f);
    public EnergyProperties water = new EnergyProperties(4.182f, 1.0f);
    public EnergyProperties ice = new EnergyProperties(2.0f, 0.92f);
    public EnergyProperties soil = new EnergyProperties(1.9f, 1.3f);
    public EnergyProperties mountain = new EnergyProperties(0.75f, 2.66f);
    // we only calculate a m2 so lets consider 40m depth
    public float groundVolumeEnergyConsideration = 40;
    public float absoluteZero = -273.15f;

    public float mountainousElevationThreshold = 3.5f;
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
