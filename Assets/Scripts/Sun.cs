using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun : MonoBehaviour
{
    const float HALF_PI = Mathf.PI / 2f;

    [SerializeField]
    float equator = 0f;

    [SerializeField]
    float distance = 10000f;

    [SerializeField]
    float latitudeAmplitude = 50f;

    public float energyFlux = 1;

    public static Sun instance { get; private set; }

    Light light;

    [SerializeField]
    Color dayLight;
    [SerializeField]
    Color twilightLight;
    [SerializeField]
    Color nightLight;

    [SerializeField]
    float twilightDuration = 0.1f;

    [SerializeField]
    Transform playerPosition;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        } else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        light = GetComponent<Light>();
    }

    private void Update()
    {
        float halfTwilightDuration = twilightDuration * 0.5f;
        var sunRadians = StandardTime.instance.LocalSunInclination(playerPosition.position);
        var z = latitudeAmplitude * -1 * Mathf.Cos(StandardTime.instance.YearProgres);
        var y = distance * Mathf.Sin(sunRadians);
        var x = distance * Mathf.Cos(sunRadians);
        transform.position = new Vector3(x, y, z);
        transform.LookAt(playerPosition);

        if (sunRadians < -halfTwilightDuration)
        {
            light.color = nightLight;
        } else if (sunRadians < 0)
        {
            light.color = Color.Lerp(nightLight, twilightLight, (sunRadians - halfTwilightDuration) / halfTwilightDuration);
        } else if (sunRadians < twilightDuration)
        {
            light.color = Color.Lerp(twilightLight, dayLight, sunRadians / halfTwilightDuration);
        } else if (sunRadians < Mathf.PI - halfTwilightDuration)
        {
            light.color = dayLight;
        } else if (sunRadians < Mathf.PI)
        {
            light.color = Color.Lerp(dayLight, twilightLight, (sunRadians - (Mathf.PI - halfTwilightDuration)) / halfTwilightDuration);
        } else if (sunRadians < Mathf.PI + halfTwilightDuration)
        {
            light.color = Color.Lerp(twilightLight, nightLight, (sunRadians - Mathf.PI) / halfTwilightDuration);
        } else
        {
            light.color = nightLight;
        }
    }
}
