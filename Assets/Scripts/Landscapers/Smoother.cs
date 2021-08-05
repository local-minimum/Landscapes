using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Smoother : LandscaperBase
{
    public float smoothActionProbability = 0.2f;
    public float actionDeltaThreshold = 0.5f;
    public AnimationCurve averageAttraction;
    public bool allowCrossSeaSurfaceInfluence = true;

    protected override IEnumerator<float> Landscape(Geography geography)
    {
        System.Func<GeoNode, bool> filter = node =>
        {
            return Random.value < smoothActionProbability;
        };
        System.Func<GeoNode, GeoNode, bool> neighbourFilter = (node, neighbour) =>
        {
            return (
                allowCrossSeaSurfaceInfluence 
                || Mathf.Sign(node.Elevation) == Mathf.Sign(neighbour.Elevation)
            ) && Mathf.Abs(node.Elevation - neighbour.Elevation) > actionDeltaThreshold;
        };

        var candidates = geography
            .GetNodes(filter)
            .ToArray();
        // Debug.Log(string.Format("{0} candidates for smoothing height", candidates.Length));
        for (int i=0; i<candidates.Length; i++)
        {
            var node = candidates[i];
            var neighbours = node.GetNeighbours(neighbourFilter).ToArray();            
            if (neighbours.Length == 0) continue;            
            var nodeDepthSign = Mathf.Sign(node.Elevation);
            var avg = neighbours
                .Select(n => Mathf.Sign(n.Elevation) == nodeDepthSign ? n.Elevation : 0)
                .Sum() / neighbours.Length;            
            var newDepth = Mathf.Lerp(node.Elevation, avg, averageAttraction.Evaluate(Random.value));
            // Debug.Log(string.Format("{0}: y {1}, n-avg {2} -> {3}", node.name, position.y, avg, newY));            
            node.Elevation = newDepth;
            if (i % 500 == 0) yield return (float)i / candidates.Length;
        }
    }

}
