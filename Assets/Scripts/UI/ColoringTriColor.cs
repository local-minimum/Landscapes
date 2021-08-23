using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ColoringTriColor : ColoringBase
{
    [SerializeField]
    Color minColor;
    [SerializeField]
    Color midColor;
    [SerializeField]
    Color maxColor;

    float min;
    float mid;
    float minMidRange;
    float midMaxRange;

    public void SetValueRanges(float min, float mid, float max)
    {
        this.min = min;
        this.mid = mid;
        minMidRange = mid - min;
        midMaxRange = max - mid;
    }

    public override void SetColor(Texture2D tex, int x, int y, float value)
    {
        if (value < mid)
        {
            tex.SetPixel(x, y, Color.Lerp(minColor, midColor, (value - min) / minMidRange));
            
        } else if (value > mid)
        {
            tex.SetPixel(x, y, Color.Lerp(midColor, maxColor, (value - mid) / midMaxRange));
        } else
        {
            tex.SetPixel(x, y, midColor);
        }
    }
}
