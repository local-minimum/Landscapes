using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boat : MonoBehaviour
{
    [SerializeField]
    Transform floater;

    [SerializeField]
    Transform grounder;

    List<Collider> floaters = new List<Collider>();
    List<Collider> grounders = new List<Collider>();

    // Start is called before the first frame update
    void Start()
    {
        var worldCam = FindObjectOfType<AreaViewCamera>();
        if (worldCam != null) worldCam.gameObject.SetActive(false);
        floaters.AddRange(floater.GetComponentsInChildren<Collider>());
        grounders.AddRange(grounder.GetComponentsInChildren<Collider>());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
