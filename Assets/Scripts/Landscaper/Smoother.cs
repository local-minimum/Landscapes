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

    protected override void Landscape(Geography geography)
    {
        System.Func<GeoNode, bool> filter = node =>
        {
            return Random.value < smoothActionProbability;
        };
        System.Func<GeoNode, GeoNode, bool> neighbourFilter = (node, neighbour) =>
        {
            return (
                allowCrossSeaSurfaceInfluence 
                || Mathf.Sign(node.transform.position.y) == Mathf.Sign(neighbour.transform.position.y)
            ) && Mathf.Abs(node.transform.position.y - neighbour.transform.position.y) > actionDeltaThreshold;
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
            var position = node.transform.position;
            var nodeYSign = Mathf.Sign(position.y);
            var avg = neighbours
                .Select(n => Mathf.Sign(n.transform.position.y) == nodeYSign ? n.transform.position.y : 0)
                .Sum() / neighbours.Length;            
            var newY = Mathf.Lerp(position.y, avg, averageAttraction.Evaluate(Random.value));
            // Debug.Log(string.Format("{0}: y {1}, n-avg {2} -> {3}", node.name, position.y, avg, newY));
            position.y = newY;
            node.transform.position = position;
        }
    }

}
