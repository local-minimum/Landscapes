﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CircleElevator : LandscaperBase
{
    public enum PointFilter { Any, Water, Land };
    public int circles = 40;
    public float[] elevations = { -1, -2, -4, -6 };
    public AnimationCurve radiusDistribution;
    public PointFilter pointFilter;

    protected override void Landscape(Geography geography)
    {
        var boundingRect = geography.BoundingRect;
        for (int i=0; i<circles; i++)
        {
            var depth = elevations[Random.Range(0, elevations.Length)];
            var point = boundingRect.RandomPoint();
            var origin = geography.transform.position + new Vector3(point.x, 0, point.y);
            origin.y = 0;
            var sqRadius = Mathf.Pow(radiusDistribution.Evaluate(Random.value), 2);
            System.Func<GeoNode, bool> filter = node => {
                var pos = node.transform.position;
                switch (pointFilter)
                {
                    case PointFilter.Any:
                        break;
                    case PointFilter.Land:
                        if (pos.y < 0) return false;
                        break;
                    case PointFilter.Water:
                        if (pos.y >= 0) return false;
                        break;
                }
                pos.y = 0;
                return Vector3.SqrMagnitude(pos - origin) < sqRadius;
            };
            var changed = geography
                .GetNodes(filter)
                .Select(node =>
                {
                    Vector3 newPos = node.transform.position;
                    newPos.y = depth;
                    node.transform.position = newPos;
                    return node;
                })
                .Count();
            Debug.Log(string.Format("{0} nodes got {1} depth.", changed, depth));
        }
    }
}