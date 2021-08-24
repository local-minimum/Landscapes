using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColoringPalette : MonoBehaviour
{
    public static ColoringPalette instance
    {
        get; private set;
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

    [SerializeField]
    public ColoringTriColor Temperature;

    [SerializeField]
    public ColoringBiColor Shore;

    [SerializeField]
    public ColoringBiColor SunAngle;

    [SerializeField]
    public ColoringBiColor SunLatOffset;

    private void Start()
    {
        Temperature.SetValueRanges(-20, 0, 50);
        Shore.SetValueRange(-1, 1);
        SunAngle.SetValueRange(-Mathf.PI, Mathf.PI);
        SunLatOffset.SetValueRange(0, 90f);
    }
}
