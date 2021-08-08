using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Boat : MonoBehaviour
{
    enum CollisionType { Nothing = 0, Water = 1, Ground = 2, Both = 3 };

    [SerializeField]
    float fullSpeedAheadForce = 100;

    [SerializeField]
    float fullTorqueForce = 20;

    [SerializeField]
    List<Collider> floaters = new List<Collider>();
    [SerializeField]
    List<Collider> grounders = new List<Collider>();
    Dictionary<Collider, CollisionType> colliding = new Dictionary<Collider, CollisionType>();

    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        var worldCam = FindObjectOfType<AreaViewCamera>();
        if (worldCam != null) worldCam.gameObject.SetActive(false);
    }

    public bool Floating
    {
        get
        {
            int j = 0;
            for (int i=0,l=floaters.Count; i<l; i++)
            {
                var floater = floaters[i];
                if (colliding.ContainsKey(floater) && colliding[floater].HasFlag(CollisionType.Water))
                {
                    j++;
                }
            }
            return j > floaters.Count / 2;
        }
    }

    public bool Grounded
    {
        get
        {
            var j = 0;
            for (int i=0,l=floaters.Count; i<l; i++)
            {
                var floater = floaters[i];
                if (colliding.ContainsKey(floater) && colliding[floater].HasFlag(CollisionType.Ground))
                {
                    j++;
                }
            }
            if (j > floaters.Count / 2) return true;
            j = 0;
            for (int i = 0, l = grounders.Count; i < l; i++)
            {
                var grounder = grounders[i];
                if (colliding.ContainsKey(grounder) && colliding[grounder].HasFlag(CollisionType.Ground))
                {
                    j++;
                }
            }
            return j > 1;
        }
    }

    private void Update()
    {
        if (!Floating)
        {            
            Debug.LogWarning("Not in water");
            return;
        }
        if (Grounded)
        {
            Debug.LogWarning("Grounded");
            return;
        }
        if (Input.GetAxis("Vertical") > 0)
        {
            rb.AddForce(transform.forward * Input.GetAxis("Vertical") * fullSpeedAheadForce);
        }
        rb.AddTorque(0f, Input.GetAxis("Horizontal") * fullTorqueForce, 0f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        var collisionType = collision.gameObject.CompareTag("Water") ? CollisionType.Water : CollisionType.Ground;

        foreach (var pt in collision.contacts)
        {
            if (colliding.ContainsKey(pt.thisCollider)) {
                colliding[pt.thisCollider] |= collisionType;
            } else
            {
                colliding[pt.thisCollider] = collisionType;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        var collisionType = collision.gameObject.CompareTag("Water") ? CollisionType.Water : CollisionType.Ground;
        var mask = ~(1 << (int)collisionType);
        foreach (var pt in collision.contacts)
        {
            colliding[pt.thisCollider] = (CollisionType) ((int) colliding[pt.thisCollider] & mask);
        }
    }
}
