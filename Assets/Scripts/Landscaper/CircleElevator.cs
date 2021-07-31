using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CircleElevator : LandscaperBase
{
    public int depthCircles = 20;
    public float[] depths = { -10, -50, -200, -500 };
    public AnimationCurve radiusDistribution;    

    protected override void Landscape(Geography geography)
    {
        var boundingRect = geography.BoundingRect;
        for (int i=0; i<depthCircles; i++)
        {
            var depth = depths[Random.Range(0, depths.Length)];
            var point = RandomPointInRect(boundingRect);
            var origin = geography.transform.position + new Vector3(point.x, 0, point.y);
            origin.y = 0;
            var sqRadius = Mathf.Pow(radiusDistribution.Evaluate(Random.value), 2);
            System.Func<GeoNode, bool> filter = node => {
                var pos = node.transform.position;
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

    Vector2 RandomPointInRect(Rect r)
    {
        var x = Random.Range(r.xMin, r.xMax);
        var z = Random.Range(r.yMin, r.yMax);
        return new Vector2(x, z);
    }
}
