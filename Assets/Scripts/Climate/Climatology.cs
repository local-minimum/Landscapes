using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Climatology : MonoBehaviour
{
    public bool isLand
    {
        get; private set;
    }

    public float WaterDepth
    {
        get; private set;
    }

    public bool isMountainous
    {
        get; private set;
    }

    public int DistanceToShore
    {
        get; private set;
    }    
    
    public float Latitude
    {
        get;
        private set;
    }

    public float Longitude
    {
        get;
        private set;
    }

    public float LocalTime
    {
        get
        {
            return StandardTime.instance.LocalTime(transform.position);
        }
    }

    public string LocalTimeHuman
    {
        get
        {
            var hours = LocalTime / (Mathf.PI * 2) * 24;
            var localHours = Mathf.FloorToInt(hours);
            if (localHours < 0) localHours += 24;
            if (localHours > 23) localHours -= 24;
            if (hours < 0) hours += 24;
            var localMinuts = Mathf.FloorToInt((hours * 60) % 60);
            return string.Format("{0:D2}:{1:D2}", localHours, localMinuts);
        }
    }

    public float SunAngle
    {
        get
        {            
            return StandardTime.instance.LocalSunAngle(transform.position);
        }
    }

    public float Temperature
    {
        get
        {
            return EnergyAir / (GlobalClimate.instance.air.WattPerCelsius(0)) + GlobalClimate.instance.absoluteZero;
        }

        private set
        {
            var kelvin = value - GlobalClimate.instance.absoluteZero;
            EnergyAir = kelvin * GlobalClimate.instance.air.WattPerCelsius(0);
            var eProp = isLand ? (isMountainous ? GlobalClimate.instance.mountain : GlobalClimate.instance.soil) : GlobalClimate.instance.water;
            EnergyGround = kelvin * eProp.WattPerCelsius(isLand ? GlobalClimate.instance.groundVolumeEnergyConsideration : WaterDepth);
        }
    }

    public float EnergyGround
    {
        get;
        private set;
    }

    public float EnergyAir
    {
        get;
        private set;
    }

    static bool running;

    private void OnEnable()
    {
        Geography.OnWorldReady += Geography_OnWorldReady;
    }

    private void OnDisable()
    {
        Geography.OnWorldReady -= Geography_OnWorldReady;
    }

    private void Start()
    {
        Temperature = GlobalClimate.instance.annualAverageTemperatur + Random.value * GlobalClimate.instance.refTempSeedingNoise;
    }

    private void Geography_OnWorldReady(Geography geography)
    {
        if (!running) {
            running = true;
        }
        var node = GetComponent<GeoNode>();
        isLand = node.Is(Geography.NodeFilter.Land);
        isMountainous = isLand ? (node.Elevation > GlobalClimate.instance.mountainousElevationThreshold) : false;
        WaterDepth = isLand ? 0 : -node.Elevation;
        DistanceToShore = node.topology.HasFlag(NodeBase.Topology.Shore) ? 0 : 1;
        Latitude = node.Latitude;
        Longitude = node.Longitude;
    }

    private void Update()
    {
        if (!running) return;        
        CalculateSunEnergy();
        RadiateEnergy();
        DispereseAirEnergy();
        if (!isLand)
        {
            DispereseWaterEnergy();
        }
    }

    void CalculateSunEnergy()
    {
        var sunAngleFactor = Mathf.Max(0, Mathf.Sin(SunAngle));
        var sunEnergy = sunAngleFactor * Sun.instance.energyFlux * Time.deltaTime;
        var sunAngleAtmoFactor = (1 + GlobalClimate.instance.atmosphereReflection * sunAngleFactor);
        sunEnergy *= (1 - GlobalClimate.instance.atmosphereReflection) * sunAngleAtmoFactor;
        var atmoAbs = sunEnergy * (1 - GlobalClimate.instance.atmosphereSunAngleCoeff) * sunAngleAtmoFactor;
        /*
         * if (isCloudy) {
         *      sunEnergy *= (1 - GlobalClimate.instance.cloudeReflection);
         *      atmoAbs *= cloudAbsorptionFactor;
         * }
         */
        sunEnergy -= atmoAbs;
        var groundAbsorption = sunEnergy * GlobalClimate.instance.groundAbsorpiton;
        EnergyGround += groundAbsorption;
        EnergyAir += atmoAbs;
    }

    void RadiateEnergy()
    {
        var groundRadiation = EnergyGround * GlobalClimate.instance.groundRadiation * Time.deltaTime;
        EnergyGround -= groundRadiation;
        groundRadiation *= (1 - GlobalClimate.instance.groundSpaceRadiation);
        EnergyAir += groundRadiation;
        EnergyAir *= (1 - GlobalClimate.instance.atmosphereRadiation) * Time.deltaTime;
    }

    void DispereseAirEnergy()
    {
        // TODO: Winds and diffusion
    }

    void DispereseWaterEnergy()
    {
        // TODO: Currents and diffusion
    }
}
