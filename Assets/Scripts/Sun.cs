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
        var time = StandardTime.instance.LocalTime(playerPosition.position) - HALF_PI;
        var z = latitudeAmplitude * -1 * Mathf.Cos(StandardTime.instance.YearProgres);
        var y = distance * Mathf.Sin(time);
        var x = distance * Mathf.Cos(time);
        transform.position = new Vector3(x, y, z);
        transform.LookAt(playerPosition);

        if (time < -halfTwilightDuration)
        {
            light.color = nightLight;
        } else if (time < 0)
        {
            light.color = Color.Lerp(nightLight, twilightLight, (time - halfTwilightDuration) / halfTwilightDuration);
        } else if (time < twilightDuration)
        {
            light.color = Color.Lerp(twilightLight, dayLight, time / halfTwilightDuration);
        } else if (time < Mathf.PI - halfTwilightDuration)
        {
            light.color = dayLight;
        } else if (time < Mathf.PI)
        {
            light.color = Color.Lerp(dayLight, twilightLight, (time - (Mathf.PI - halfTwilightDuration)) / halfTwilightDuration);
        } else if (time < Mathf.PI + halfTwilightDuration)
        {
            light.color = Color.Lerp(twilightLight, nightLight, (time - Mathf.PI) / halfTwilightDuration);
        } else
        {
            light.color = nightLight;
        }
    }
}
