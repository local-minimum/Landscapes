using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Climatology : MonoBehaviour
{
    public const float MAX_LAT_EFFECT_DISTANCE = 150;

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
    }

    private void Update()
    {
        if (!running) return;
        UpdateTemperature();
    }

    void UpdateTemperature()
    {
        float latOffset = StandardTime.instance.Latitudes(Mathf.Abs(transform.position.z - Sun.instance.transform.position.z));
        float latEffect = Mathf.Lerp(1, GlobalClimate.instance.maxLatitudeOffsetFactor, latOffset / MAX_LAT_EFFECT_DISTANCE);
        float sunRadiationFactor = Mathf.Max(0, Mathf.Sin(StandardTime.instance.LocalSunInclination(transform.position))) * latEffect;
        Temperature += (sunRadiationFactor * Sun.instance.energyFlux) * Time.deltaTime;
        Temperature -= (Temperature - GlobalClimate.instance.referenceTemperature) * GlobalClimate.instance.surfaceCoolingFactor * Time.deltaTime;
    }
}
