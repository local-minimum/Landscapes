using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public struct DateTime
{
    const float PI_TIME_TO_SECONDS = 3600f * 24f / (2 * Mathf.PI);
    const float TWO_PI = 2 * Mathf.PI;
    public int Year;
    public int DayOfYear;
    public int Hours;
    public int Minutes;
    public float Seconds;

    public DateTime(int year, int doy, int daysPerYear, float time)
    {

        if (time > TWO_PI)
        {
            time -= TWO_PI;
            doy++;
            if (doy >= daysPerYear)
            {
                doy %= daysPerYear;
                year++;
            }
        } else if (time < 0)
        {
            time += TWO_PI;
            doy--;
            if (doy < 0)
            {
                year--;
                doy += daysPerYear;
            }
        }
        Year = year;
        DayOfYear = doy;
        time *= PI_TIME_TO_SECONDS;
        Hours = Mathf.FloorToInt(time / 3600);
        time -= Hours * 3600;
        Minutes = Mathf.FloorToInt(time / 60);
        time -= Minutes * 60;
        Seconds = time;
    }
}

public class StandardTime : MonoBehaviour
{
    const float TWO_PI = Mathf.PI * 2;
    const float HALF_PI = Mathf.PI * 0.5f;

    [SerializeField]
    float longitudesPerUnit = 1f;

    [SerializeField]
    float latitudesPerUnit = 1f;

    [SerializeField]
    float secondsPerDay = 30f;

    [SerializeField, Range(5, 300)]
    int daysPerYear = 20;

    public static StandardTime instance { get; private set; }

    public int Year { get; private set; }
    public int DayOfYear { get; private set; }
    public float Time { get; private set; }
    public float YearProgres {        
        get {
            return (DayOfYear + Time / TWO_PI) / daysPerYear;
        }
    }

    Rect worldBounds;
    bool running;
    public float Latitudes(float units)
    {
        return units * latitudesPerUnit;
    }

    public float Longitudes(float units)
    {
        return units * longitudesPerUnit;
    }

    public float LocalTime(Vector3 position)
    {        
        return Time + Longitudes(position.x) * Mathf.Deg2Rad;
    }

    public float LocalSunInclination(Vector3 positon)
    {
        return LocalTime(positon) - HALF_PI;
    }

    public DateTime LocalDateTime(Vector3 position)
    {        
         return new DateTime(Year, DayOfYear, daysPerYear, LocalTime(position));
        
    }

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

    private void Start()
    {
        DayOfYear = Random.Range(0, daysPerYear);
    }

    private void OnEnable()
    {
        Geography.OnWorldReady += Geography_OnWorldReady;
    }
    
    private void OnDisable()
    {
        Geography.OnWorldReady -= Geography_OnWorldReady;
    }

    private void Geography_OnWorldReady(Geography geography)
    {
        running = true;
        worldBounds = geography.BoundingRect;
        if (worldBounds.width * longitudesPerUnit > 360)
        {
            Debug.LogWarning(string.Format("Adjusted longitude per unit because {0:F1} degree planet", worldBounds.width * longitudesPerUnit));
            longitudesPerUnit = 360 / worldBounds.width;
            
        } else
        {
            Debug.Log(string.Format("Map covers {0:F1} longitudes", worldBounds.width  * longitudesPerUnit));
        }
        if (worldBounds.height * latitudesPerUnit > 180)
        {
            Debug.LogWarning(string.Format("Adjusted latitude per unit because {0:F1} degree planet", worldBounds.height * latitudesPerUnit));
            latitudesPerUnit = 360 / worldBounds.height;
        }
        else
        {
            Debug.Log(string.Format("Map covers {0:F1} latitudes", worldBounds.height * latitudesPerUnit));
        }
    }


    /** A day is two pi.
     */
    private float deltaTime        
    {
        get
        {
            return UnityEngine.Time.deltaTime / secondsPerDay * TWO_PI;
        }
    }

    private void Update()
    {
        if (!running) return;
        Time += deltaTime;
        if (Time >= TWO_PI)
        {
            DayOfYear ++;
            Time %= TWO_PI;
            if (DayOfYear >= daysPerYear)
            {
                daysPerYear++;
                DayOfYear %= daysPerYear;
            }
        }
    }
}
