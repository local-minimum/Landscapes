using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Metorologist : MonoBehaviour
{
    public enum PaintParameter { Shore, Temperature};

    struct ImgPos
    {
        public int X;
        public int Y;

        public ImgPos(Vector2 scaledPosition, Texture2D tex)
        {
            X = Mathf.Clamp(Mathf.RoundToInt(scaledPosition.x * tex.width), 0, tex.width - 1);
            Y = Mathf.Clamp(Mathf.RoundToInt(scaledPosition.y * tex.height), 0, tex.height - 1);
        }
    }

    Geography geography;    
    Dictionary<Climatology, Vector2> scaledPositions = new Dictionary<Climatology, Vector2>();

    [SerializeField, Range(0, 100)]
    int updateEachFrame = 5;
    int frame = 0;

    [SerializeField, Range(128, 1024)]
    int texWidth = 400;

    [SerializeField, Range(64, 512)]
    int texHeight = 300;

    Texture2D tex;

    [SerializeField]
    ColoringTriColor TempColorer;

    [SerializeField]
    ColoringBiColor ShoreColorer;

    [SerializeField]
    PaintParameter paintParameter;

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
        this.geography = geography;
        var worldRect = geography.BoundingRect;
        for (int i = 0, l = geography.NodeCount; i<l; i++)
        {
            var node = geography.GetNode(i);
            var climate = node.GetComponent<Climatology>();
            scaledPositions[climate] = worldRect.ScaledPosition(node.PlanarPosition);
        }
    }

    private void Start()
    {
        TempColorer.SetValueRanges(-20, 0, 50);
        ShoreColorer.SetValueRange(-1, 1);
    }

    private void Update()
    {
        frame++;
        if (frame < updateEachFrame) return;
        UpdateTex();
        switch (paintParameter)
        {
            case PaintParameter.Shore:
                Paint(climate => climate.isLand && climate.DistanceToShore == 0 ? 1 : -1, ShoreColorer);
                break;
            case PaintParameter.Temperature:
                Paint(climate => climate.Temperature, TempColorer);
                break;
        }       
    }

    void UpdateTex()
    {
        if (tex == null)
        {
            tex = new Texture2D(texWidth, texHeight);
            var sprite = Sprite.Create(tex, new Rect(0, 0, texWidth, texHeight), new Vector2(0.5f, 0.5f));
            var img = GetComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.enabled = true;

        } else if (tex.width != texWidth || tex.height != texHeight)
        {
            tex.Resize(texWidth, texHeight);
        }
    }

    void Paint(System.Func<Climatology, float> selector, ColoringBase colorer)
    {
        colorer.BGFill(tex);
        foreach(var climate in scaledPositions.Keys)
        {
            var value = selector(climate);
            var imgPos = new ImgPos(scaledPositions[climate], tex);            
            colorer.SetColor(tex, imgPos.X, imgPos.Y, value);
        }
        tex.Apply();
    }
}
