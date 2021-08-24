using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EnergyProperties
{
    [Tooltip("kg")]
    public float estimateMass;

    [Tooltip("W / kg C")]
    public float specificHeatCapacity;

    [Tooltip("g / cm3")]
    public float density;

    [Tooltip("Use estimate mass")]
    public bool useEstmatedMass;

    /// <summary>
    /// The mass
    /// </summary>
    /// <param name="volume">m3, disregarded if estimate mass is in use</param>
    /// <returns>kg</returns>
    public float Mass(float volume)
    {
        
        return useEstmatedMass ? estimateMass : volume * density * 1000f;    
    }

    /// <summary>
    /// Returns Watt / Celcius
    /// </summary>
    /// <param name="volume">m3, disregarded if estimate mass is in use</param>
    public float WattPerCelsius(float volume)
    {
        return specificHeatCapacity * Mass(volume);
        
    }

    public EnergyProperties(float specificHeatCapacity, float density, float estimateMass)
    {
        this.estimateMass = estimateMass;
        this.specificHeatCapacity = specificHeatCapacity;
        this.density = density;
        useEstmatedMass = true;
    }

    public EnergyProperties(float specificHeatCapacity, float density)
    {
        estimateMass = 0;
        this.specificHeatCapacity = specificHeatCapacity;
        this.density = density;
        useEstmatedMass = false;
    }
}
