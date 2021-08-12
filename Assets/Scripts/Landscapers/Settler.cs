using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Settler : LandscaperBase
{
    [SerializeField]
    int settlements = 20;

    [SerializeField]
    float maxWeight = 100;
    [SerializeField]
    float minWeightSelected = 5;

    [SerializeField]
    float shoreWeight = 1f;

    [SerializeField]
    float openSeaWeight = 3f;

    [SerializeField]
    float landBaseWeight = 1.5f;

    [SerializeField]
    float mountainWeight = 5f;

    [SerializeField]
    float deltaHeightForMountain = 2f;

    [SerializeField]
    float bonusWeightThreshold = 50f;

    [SerializeField]
    int bonusSearchDepth = 3;

    [SerializeField]
    float bonusMultiplier = 3f;

    Dictionary<NodeBase, float> nodeWeights = new Dictionary<NodeBase, float>();

    protected override IEnumerator<float> Landscape(Geography geography)
    {
        var candidates = GetCandidates(geography);

        for (int i = 0; i<settlements; i++)
        {
            var settlement = GetSettlement(candidates);
            candidates.Remove(settlement);
            settlement.gameObject.AddComponent<Dwelling>();
            RecalculateWeights(settlement);
            if (i % 5 == 0) yield return (i + 1f) / settlements;
        }
    }

    List<GeoNode> GetCandidates(Geography geography)
    {
        var candidates = geography
            .GetNodes(n => { var topo = n.topology; return topo.HasFlag(NodeBase.Topology.Land) && topo.HasFlag(NodeBase.Topology.Shore); })
            .ToList();
        for (int i = 0, l=geography.NodeCount; i<l; i++)
        {
            nodeWeights[geography.GetNode(i)] = maxWeight;
        }
        return candidates;
    }

    GeoNode GetSettlement(List<GeoNode> candidates)
    {
        var weights = new float[candidates.Count];
        float totalWeight = 0;
        for (int i = 0, l = candidates.Count; i < l; i++)
        {            
            var candidate = candidates[i];
            var weight = nodeWeights[candidate];
            if (weight < minWeightSelected) continue;
            var bonus = CalculateBonus(candidate);
            weights[i] = bonus + weight;
            totalWeight += bonus + weight;
        }
        totalWeight *= Random.value;
        for (int i = 0, l = candidates.Count; i < l; i++)
        {
            var candidate = candidates[i];
            var weight = weights[i];
            if (totalWeight <= weight) return candidate;
            totalWeight -= weight;
        }
        return candidates.Last();
    }

    float CalculateBonus(GeoNode node)
    {
        
        var tiles = new List<GeoNode>() { node };
        var depths = new Dictionary<GeoNode, int>();
        depths[node] = 0;
        var j = 0;
        while (j < tiles.Count)
        {
            var curNode = tiles[j];
            if (depths[curNode] >= bonusSearchDepth)
            {
                j++;
                continue;
            }
            foreach (var neighbour in curNode.GetNeighbours((n, neigh) => !depths.ContainsKey(neigh))) {
                if (
                    nodeWeights[neighbour] <= bonusWeightThreshold
                    || Mathf.Abs(Mathf.Max(0, curNode.Elevation) - Mathf.Max(0, neighbour.Elevation)) >= deltaHeightForMountain
                ) continue;
                tiles.Add(neighbour);
                depths[neighbour] = depths[curNode] + 1;
            }
            j++;                        
        }
        return (tiles.Count - 1) * bonusMultiplier;
    }

    void RecalculateWeights(GeoNode settlement)
    {
        nodeWeights[settlement] = 0;
        var nodes = new List<GeoNode>() { settlement };
        var visited = new HashSet<GeoNode>();
        while (nodes.Count > 0)
        {
            var node = nodes[0];
            nodes.RemoveAt(0);
            visited.Add(node);
            foreach (var neighbour in node.GetNeighbours((n, neigh) => !visited.Contains(neigh)))
            {
                var weight = nodeWeights[node];
                var topo = neighbour.topology;
                if (topo.HasFlag(NodeBase.Topology.Water))
                {
                    if (topo.HasFlag(NodeBase.Topology.Shore))
                    {
                        weight += shoreWeight;
                    } else
                    {
                        weight += openSeaWeight;
                    }
                } else
                {
                    if (topo.HasFlag(NodeBase.Topology.Shore))
                    {
                        weight += shoreWeight;
                    } else
                    {
                        weight += landBaseWeight;
                    }
                    if (Mathf.Abs(Mathf.Max(0, node.Elevation) - neighbour.Elevation) > deltaHeightForMountain)
                    {
                        weight += mountainWeight;
                    }
                }
                if (nodeWeights[neighbour] > weight)
                {
                    nodeWeights[neighbour] = nodeWeights[neighbour];
                    nodes.Add(neighbour);
                    nodes.Sort((a, b) => nodeWeights[a] - nodeWeights[b] < 0 ? -1 : 1);
                }                                
            }
        }
    }
}

