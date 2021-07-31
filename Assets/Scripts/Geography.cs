using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Geography : MonoBehaviour
{

    public float interStepPause = 1f;

    List<GeoNode> nodes = new List<GeoNode>();
    public List<LandscaperBase> creation = new List<LandscaperBase>();

    public void AddNode(GeoNode node)
    {
        node.gameObject.name = string.Format("GeoNode #{0}", nodes.Count);
        nodes.Add(node);
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

    public IEnumerable<GeoNode> GetNodes(System.Func<GeoNode, bool> filter)
    {
        return nodes.Where(filter);
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
            creation[i].Apply(this);
            yield return new WaitForSeconds(interStepPause);
        }
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
