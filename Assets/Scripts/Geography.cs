using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Geography : MonoBehaviour
{
    public int width = 30;
    public int height = 20;
    public float spacing = 1;

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

    private void Start()
    {
        for (int i = 0, l=creation.Count; i<l;i++)
        {
            creation[i].Apply(this);
        }
    }
}
