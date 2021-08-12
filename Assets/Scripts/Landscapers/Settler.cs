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
        float totalWeight = 0;
        for (int i = 0, l = candidates.Count; i < l; i++)
        {
            totalWeight += nodeWeights[candidates[i]];
        }
        totalWeight *= Random.value;
        for (int i = 0, l = candidates.Count; i < l; i++)
        {
            var candidate = candidates[i];
            var weight = nodeWeights[candidate];
            if (totalWeight <= weight) return candidate;
            totalWeight -= weight;
        }
        return candidates.Last();
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

