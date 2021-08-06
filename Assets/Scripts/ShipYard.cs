using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ShipYard : MonoBehaviour
{
    public Boat prefab;

    private void OnEnable()
    {
        Geography.OnWorldReady += Geography_OnWorldReady;
    }

    private void OnDisable()
    {
        Geography.OnWorldReady -= Geography_OnWorldReady;
    }

    private void Geography_OnWorldReady(Geography geography)
    {
        var seas = geography.GetNodes(Geography.NodeFilter.Water).ToArray();
        var sea = seas[Random.Range(0, seas.Length)];
        var pos = sea.transform.position;
        pos.y = 0;
        Instantiate(prefab, pos, Quaternion.identity);
    }
}
