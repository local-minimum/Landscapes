using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class ColoringBase 
{
    [SerializeField]
    Color bgColor;

    public void BGFill(Texture2D tex)
    {
        for (int y = 0; y < tex.height; y++)
        {
            for (int x = 0; x < tex.width; x++)
            {
                
                tex.SetPixel(x, y, bgColor);
            }
        }
    }

    abstract public void SetColor(Texture2D tex, int x, int y, float value);
}
