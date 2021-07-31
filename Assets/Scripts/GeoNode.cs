using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeoNode : MonoBehaviour
{
    public enum Topology { Internal, Edge };
    List<GeoNode> neighbours = new List<GeoNode>();
    public Geography geography { get; set; }
    public float gizmoSize { get; set; }

    public Topology topology
    {
        get
        {
            if (neighbours.Count < 6)
            {
                return Topology.Edge;
            }
            return Topology.Internal;
        }
    }
    public void SetNeighbours(List<GeoNode> nodes)
    {
        if (neighbours.Count == 0)
        {
            neighbours.AddRange(nodes);
        } else
        {
            throw new System.InvalidOperationException(
                "Neighbours may only be set once"
            );
        }
    }

    public float SqDistance(GeoNode neighbour)
    {
        for (int i = 0, l = neighbours.Count; i < l; i++)
        {
            if (neighbours[i] == neighbour)
            {
                return Vector3.SqrMagnitude(neighbour.transform.position - transform.position);
            }
        }
        throw new System.ArgumentException("Not a neighbour", nameof(neighbour));
    }

    private void OnDrawGizmos()
    {
        if (geography.showGeoNodeConnectionsGizmos)
        {
            Gizmos.color = Color.white;
            for (int i = 0, l = neighbours.Count; i < l; i++)
            {
                GeoNode neighbour = neighbours[i];
                if (neighbour.transform.position.x < transform.position.x)
                {
                    Gizmos.DrawLine(transform.position, neighbour.transform.position);
                }
                else if (neighbour.transform.position.x == transform.position.x && neighbour.transform.position.z < transform.position.z)
                {
                    Gizmos.DrawLine(transform.position, neighbour.transform.position);
                }
            }
        }
        if (geography.showGeoNodeGizmos)
        {
            Gizmos.color = transform.position.y < 0 ? Color.blue : Color.green;
            Gizmos.DrawSphere(transform.position, gizmoSize);
        }
        if (geography.showGeoNodeEdgeGizmos)
        {
            Gizmos.color = Color.cyan;
            if (topology == Topology.Edge)
            {
                Gizmos.DrawWireCube(transform.position, Vector3.one * gizmoSize);
            }
        }
    }

}
