using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;

public class Geography : MonoBehaviour
{
    public delegate void WorldReadyEvent(Geography geography);
    public static event WorldReadyEvent OnWorldReady;

    public Text loadingText;
    public bool enableGizmos = true;
    public bool showGeoNodeGizmos = true;
    public bool showGeoNodeEdgeGizmos = true;
    public bool showGeoNodeConnectionsGizmos = true;

    public Transform nodesParent;

    [Range(0.5f, 4)]
    public float gizmoSize = 1f;
    public float interStepPause = 1f;

    List<GeoNode> nodes = new List<GeoNode>();
    public List<LandscaperBase> creation = new List<LandscaperBase>();
    public List<MesherBase> meshers = new List<MesherBase>();

    public enum NodeFilter { Any, Water, Land, ZeroOrWater };

    public void AddNodeUnsafe(GeoNode node)
    {
        node.gameObject.name = string.Format("GeoNode #{0}", nodes.Count);
        nodes.Add(node);
    }

    public void AddNode(GeoNode node, GeoNode knownNeighbour, float breakDistance, GeoNode.Direction filterEdges)
    {
        AddNodeUnsafe(node);        
        var trail = new List<GeoNode>();
        var rotations = new GeoNode.Rotation[] { GeoNode.Rotation.CW, GeoNode.Rotation.CCW };
        int j = 0;
        for (int i=0; i<rotations.Length; i++)
        {
            var neighbour = knownNeighbour;
            var dir = GeoNode.DirectionFromNodes(node, knownNeighbour);
            while (!trail.Contains(neighbour))
            {
                j++;
                // Debug.Log(string.Format("{0}: Adding {1} {2}", node.name, neighbour.name, dir));
                                
                trail.Add(neighbour);
                var nextNeighbour = neighbour.GetRotationNeighbour(rotations[i], dir);
                if (nextNeighbour == null || nextNeighbour.PlanarDistance(node) > breakDistance)
                {
                    /*
                    if (nextNeighbour == null)
                    {
                        Debug.Log(string.Format("{0}: Neighbour {1} has no more that rotates {2}", node.name, neighbour.name, rotations[i]));
                    } else
                    {
                        Debug.Log(string.Format("{0}: Too far from {1}'s neighbour {2} to be neighbours {3}", node.name, neighbour.name, nextNeighbour.name, nextNeighbour.PlanarDistance(node)));
                    } 
                    */
                    if (filterEdges.HasFlag(NodeBase.DirectionFromNodes(node, neighbour))) node.AddNeighbour(neighbour);
                    trail.Add(nextNeighbour);
                    break;
                }

                if (filterEdges.HasFlag(NodeBase.DirectionFromNodes(node, neighbour))) node.AddNeighbour(neighbour);
                dir = NodeBase.DirectionFromNodes(neighbour, nextNeighbour);
                neighbour = nextNeighbour;
                if (j > 20)
                {
                    Debug.Log("Stuck circling for some reason");
                    return;
                }
            }
            trail.Remove(knownNeighbour);
        }
    }

    public int NodeCount {
        get {
            return nodes.Count;
        }
    }

    public GeoNode GetNode(int idx)
    {
        return nodes[idx];
    }

    public GeoNode GetClosestNode(Vector2 planarPosition)
    {
        return nodes
            .OrderBy(n => Vector2.SqrMagnitude(n.PlanarPosition - planarPosition))
            .FirstOrDefault();
    }

    public IEnumerable<GeoNode> GetNodes(System.Func<GeoNode, bool> filter)
    {
        return nodes.Where(filter);
    }

    public IEnumerable<GeoNode> GetNodes(NodeFilter filter)
    {
        switch (filter)
        {
            case NodeFilter.Any:
                return nodes.Where(n => true);
            case NodeFilter.ZeroOrWater:
                return nodes.Where(n => n.Elevation <= 0);
            case NodeFilter.Land:
                return nodes.Where(n => n.Elevation >= 0);
            case NodeFilter.Water:
                return nodes.Where(n => n.Elevation < 0);
        }
        throw new System.NotImplementedException(string.Format("Unsuported filter {0}", filter));
    }

    public Rect BoundingRect
    {
        get
        {
            float minX = float.MaxValue;
            float maxX = float.MinValue;
            float minZ = float.MaxValue;
            float maxZ = float.MinValue;

            nodes.ForEach(node => {
                var x = node.transform.position.x;
                var z = node.transform.position.z;
                minX = Mathf.Min(x, minX);
                maxX = Mathf.Max(x, maxX);
                minZ = Mathf.Min(z, minZ);
                maxZ = Mathf.Max(z, maxZ);
            });
            return new Rect(minX, minZ, maxX - minX, maxZ - minZ);
        }
    }

    private void Start()
    {
        StartCoroutine(BuildWorld());
    }

    IEnumerator<WaitForSeconds> BuildWorld()
    {
        for (int i = 0, l = creation.Count; i < l; i++)
        {
            var enumerator = creation[i].Apply(this);
            while (enumerator.MoveNext())
            {
                var msg = string.Format("{0}: {1:n0}%", creation[i].description, enumerator.Current * 100f);
                loadingText.text = msg;
                yield return new WaitForSeconds(0f);
            }
            yield return new WaitForSeconds(interStepPause);
        }
        enableGizmos = false;
        for (int i = 0, l = meshers.Count; i < l; i++)
        {
            meshers[i].Create(this);
            yield return new WaitForSeconds(interStepPause);
        }
        loadingText.enabled = false;
        OnWorldReady?.Invoke(this);
    }

    private void OnDrawGizmosSelected()
    {
        var rect = BoundingRect;
        var center = rect.center;
        var size = rect.size;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            new Vector3(center.x, transform.position.y, center.y),
            new Vector3(size.x, 0, size.y)
        );
    }
}
