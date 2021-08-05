using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CircleElevator : LandscaperBase
{    
    public int circles = 40;
    public float[] elevations = { -1, -2, -4, -6 };
    public AnimationCurve radiusDistribution;
    public Geography.NodeFilter pointFilter;
    public float minSeaFraction = 0f;
    public float maxSeaFraction = 1f;
    public bool internalCircles = false;
    public Gridder gridder;

    protected override IEnumerator<float> Landscape(Geography geography)
    {
        var boundingRect = geography.BoundingRect;
        var nNodes = geography.NodeCount;
        for (int i=0; i<circles; i++)
        {
            var depth = elevations[Random.Range(0, elevations.Length)];
            var radius = radiusDistribution.Evaluate(Random.value) * (gridder == null ? 1f: gridder.spacing);
            var point = internalCircles ? boundingRect.RandomPoint(radius) : boundingRect.RandomPoint();
            var sqRadius = Mathf.Pow(radius, 2);
            var changed = geography
                .GetNodes(pointFilter)
                .Where(node => Vector2.SqrMagnitude(node.PlanarPosition - point) < sqRadius)
                .Select(node =>
                {
                    node.Elevation = depth;
                    return node;
                })
                .Count();
            float seaCount = geography
                    .GetNodes(node => { return node.Elevation < 0; })
                    .Count();
            float fraction = seaCount / nNodes;
            if (i + 1 == circles && fraction < minSeaFraction) {
                circles++;
            } else if (fraction > maxSeaFraction)
            {
                circles = i;
            }
            if (i % 4 == 0) yield return (float)i / circles;
        }        
    }
}
