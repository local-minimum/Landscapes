using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Straiter : LandscaperBase
{

    public float straitProbability = 1f;
    [Range(0, 2)]
    public float sourceLocationWeight = 0.5f;
    [Range(0, 3)]
    public int flexibility = 2;
    public AnimationCurve angleCost;
    public AnimationCurve heightCost;
    public float costNoise = 0.1f;

    struct StraitCandidate
    {
        public float cost;
        public List<GeoNode> path;

        public bool Empty
        {
            get { return path == null || path.Count == 0; }
        }

        public GeoNode Source
        {
            get { return path[0]; }
        }

        public GeoNode Location
        {
            get { return path[path.Count - 1]; }
        }

        public GeoNode.Direction TargetDirection(GeoNode target)
        {
            return GeoNode.DirectionFromNodes(Location, target, 22.5f);
        }

        public (GeoNode.Direction dir, List<(GeoNode.Direction dir, GeoNode node)> nodes) EdgesForward(GeoNode target, int flexibility)
        {
            var dir = TargetDirection(target);
            var dirs = dir.WithNeighbours(flexibility)
                .Aggregate(GeoNode.Direction.NONE, (d1, d2) => d1 | d2);
            return (dir, Location.GetNeighbours(dirs));
        }

        public StraitCandidate(GeoNode node, float cost)
        {
            this.cost = cost;
            path = new List<GeoNode>() { node };
        }

        public StraitCandidate(IEnumerable<GeoNode> path, float cost)
        {
            this.cost = cost;
            this.path = new List<GeoNode>();
            this.path.AddRange(path);
        }        

        public StraitCandidate Evolve(GeoNode node, float edgeCost)
        {
            var c = new StraitCandidate(path, cost + edgeCost);
            c.path.Add(node);
            return c;
        }
    }

    protected override void Landscape(Geography geography)
    {
        var (seaLookup, seaSizes) = LabelSeas(geography);        
        if (seaSizes.Count <= 1) return;
        var centerOfSeas = CalculateCenterOfSeas(seaLookup, seaSizes);
        MakeStraits(geography, seaLookup, seaSizes, centerOfSeas);
        Debug.Log(string.Format("Found {0} seas. Sizes {1}", seaSizes.Count, string.Join(", ", seaSizes)));
    }

    (Dictionary<GeoNode, int> lookup, List<int> seaSizes) LabelSeas(Geography geography)
    {
        Dictionary<GeoNode, int> seaLookup = new Dictionary<GeoNode, int>();
        var stack = new Queue<GeoNode>();
        System.Func<GeoNode, bool> seaFilter = node => node.Elevation < 0f;
        var seaNodes = geography.GetNodes(seaFilter).ToList();
        System.Func<GeoNode, GeoNode, bool> filter = (node, neighbour) =>
        {
            return neighbour.Elevation < 0 && !seaLookup.ContainsKey(neighbour) && !stack.Contains(neighbour);
        };
        int sea = 0;
        List<int> seaSizes = new List<int>();

        while (seaNodes.Count > 0)
        {
            sea++;
            var seaSize = 0;
            var node = seaNodes[0];
            stack.Enqueue(node);
            while (stack.Count > 0)
            {
                seaSize++;
                node = stack.Dequeue();
                seaNodes.Remove(node);
                seaLookup[node] = sea;
                foreach (var neighbour in node.GetNeighbours(filter))
                {
                    stack.Enqueue(neighbour);
                }
            }
            seaSizes.Add(seaSize);
        }
        return (seaLookup, seaSizes);
    }

    List<Vector2> CalculateCenterOfSeas(Dictionary<GeoNode, int> seaLookup, List<int> seaSizes)
    {
        var centerOfSeas = new List<Vector2>();
        for (int i = 1, l=seaSizes.Count; i <= l; i++)
        {
            var center = seaLookup
                .Where(kvp => kvp.Value == i)
                .Select(kvp => kvp.Key.PlanarPosition)
                .Aggregate(Vector2.zero, (a, b) => a + b) / seaSizes[i - 1];
            centerOfSeas.Add(center);
        }
        return centerOfSeas;
    }

    void MakeStraits(Geography geography, Dictionary<GeoNode, int> seaLookup, List<int> seaSizes, List<Vector2> centerOfSeas)
    {
        var sea = seaSizes.Count;
        // If there's only 1 sea no straits to be made
        while (sea > 1)
        {
            var seaNodes = seaLookup
                .Where(kvp => kvp.Value == sea)
                .Select(kvp => kvp.Key)
                .ToArray();
            if (Random.value > straitProbability) continue;
            var targetSea = GetStraitAttractor(centerOfSeas, sea);
            var attractor = centerOfSeas[targetSea - 1];
            var target = geography.GetClosestNode(attractor);
            var candidates = StraitSourceCandidates(centerOfSeas[sea - 1], attractor, seaNodes, sea);
            var visited = new List<GeoNode>();
            visited.AddRange(seaNodes);
            bool foundSea = false;
            while (candidates.Count > 0)
            {
                candidates.Sort((x, y) => x.cost - y.cost < 0 ? -1 : 1);
                // Most inexpensive path
                var candidate = candidates[0];
                candidates.RemoveAt(0);
                //Debug.Log(string.Format("{0}: {1}, {2}", sea, candidates.Count, candidate.Location.name));
                visited.Add(candidate.Location);

                var targetDirection = candidate.TargetDirection(target);
                var edges = candidate.EdgesForward(target, flexibility);
                var possible = edges.nodes.Where(e => e.node != null && !visited.Contains(e.node)).ToArray();

                // Reached another sea
                var destinations = possible.Where(e => e.node.topology.HasFlag(GeoNode.Topology.Water)).ToArray();
                if (destinations.Length > 0)
                {
                    foundSea = true;
                    var destination = destinations[Random.Range(0, destinations.Length)].node;
                    //Debug.Log(string.Format("Found strait {0} => {1}", string.Join(" => ", candidate.path.Select(n => n.name)), destination.name));
                    var depth = Mathf.Max(destination.Elevation, candidate.Source.Elevation);
                    var reachedSea = seaLookup[destination];
                    Debug.Log(string.Format("Sea {0} connected to sea {1}", sea, reachedSea));
                    // Make strait watery and of reached sea
                    for (int i = 1, l = candidate.path.Count; i < l; i++)
                    {
                        var straitNode = candidate.path[i];
                        seaLookup[straitNode] = reachedSea;
                        straitNode.Elevation = depth;
                    }

                    // Make source sea target sea
                    for (int i = 0; i < seaNodes.Length; i++)
                    {
                        seaLookup[seaNodes[i]] = reachedSea;
                    }                    
                    break;
                }

                // Visible nodes and info
                (GeoNode.Direction dir, GeoNode node, (StraitCandidate candidate, int idx) strait)[] locations = possible.Select(p =>
                {
                    return (
                        p.dir,
                        p.node,
                        candidates
                            .Select<StraitCandidate, (StraitCandidate candidate, int idx)>((c, cIdx) => (c, cIdx))
                            .Where(c => c.candidate.Location == p.node)
                            .FirstOrDefault()
                    );

                }).ToArray();
                for (int i = 0; i < locations.Length; i++)
                {
                    var location = locations[i];
                    var angleCost = this.angleCost.Evaluate(location.dir.Angle(targetDirection));
                    var heightCost = this.heightCost.Evaluate(location.node.Elevation);
                    var edgeCost = angleCost + heightCost + Random.value * costNoise;
                    var isNew = location.strait.candidate.Empty;
                    if (!isNew && candidate.cost + edgeCost > location.strait.candidate.cost) continue;
                    var newCandidate = candidate.Evolve(location.node, edgeCost);
                    if (isNew)
                    {
                        candidates.Add(newCandidate);
                    }
                    else
                    {
                        candidates[location.Item3.idx] = newCandidate;
                    }
                }
            }
            if (!foundSea) Debug.LogWarning(string.Format("Could not find a way to connect sea {0}", sea));            
            sea--;
        }
    }

    int GetStraitAttractor(List<Vector2> centerOfSeas, int sea)
    {
        int targetIdx = 0;
        float bestSqDist = Vector2.SqrMagnitude(centerOfSeas[sea - 1] - centerOfSeas[targetIdx]);
        // Sea is our sea we try to connect from so no need to calculate it (and indexes are one less
        for (int i = 1; i < sea - 1; i++)
        {
            float sqDist = Vector2.SqrMagnitude(centerOfSeas[sea - 1] - centerOfSeas[i]);
            if (sqDist < bestSqDist)
            {
                targetIdx = i;
                bestSqDist = sqDist;
            }
        }
        return targetIdx + 1;
    }

    List<StraitCandidate> StraitSourceCandidates(Vector2 sourceSeaCenter, Vector2 targetSeaCenter, GeoNode[] sourceNodes, int sourceSea)
    {        
        var refSqDist = Vector2.SqrMagnitude(sourceSeaCenter - targetSeaCenter);
        return sourceNodes
            .Where(node => node.topology.HasFlag(GeoNode.Topology.Shore) && Vector2.SqrMagnitude(targetSeaCenter - node.PlanarPosition) < refSqDist)
            .Select(node => new StraitCandidate(node, (targetSeaCenter - node.PlanarPosition).magnitude * sourceLocationWeight + Random.value * costNoise))
            .ToList();
    }
}
