using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ColoringBiColor : ColoringBase
{
    [SerializeField]
    Color minColor;
    [SerializeField]
    Color maxColor;

    float min;
    float range;

    public void SetValueRange(float min, float max)
    {
        this.min = min;
        range = max - min;
    }

    public override void SetColor(Texture2D tex, int x, int y, float value)
    {
        tex.SetPixel(x, y, Color.Lerp(minColor, maxColor, (value - min) / range));
    }
}
