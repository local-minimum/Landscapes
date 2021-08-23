using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Climatology : MonoBehaviour
{
    public bool isLand
    {
        get; private set;
    }

    public int DistanceToShore
    {
        get; private set;
    }    

    public float Temperature
    {
        get;
        private set;
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
        Temperature = GlobalClimate.instance.referenceTemperature;
    }

    private void Geography_OnWorldReady(Geography geography)
    {
        if (!running) {
            running = true;
        }
        var node = GetComponent<GeoNode>();
        isLand = node.Is(Geography.NodeFilter.Land);
        DistanceToShore = node.topology.HasFlag(NodeBase.Topology.Shore) ? 0 : 1;
        Latitude = node.Latitude;
        Longitude = node.Longitude;
    }

    private void Update()
    {
        if (!running) return;
        UpdateTemperature();
    }

    void UpdateTemperature()
    {
        var sunRadiationFactor = Mathf.Max(0, Mathf.Sin(SunAngle));
        Temperature += (sunRadiationFactor * Sun.instance.energyFlux) * Time.deltaTime;
        Temperature -= (Temperature - GlobalClimate.instance.referenceTemperature) * GlobalClimate.instance.surfaceCoolingFactor * Time.deltaTime;
    }
}
