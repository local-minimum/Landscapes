using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoatCollider : MonoBehaviour
{
    public delegate void BoatColliderEvent(BoatCollider boatCollider);
    public static event BoatColliderEvent OnBoatCollider;
    public bool Colliding { private set; get; }

    private void OnCollisionEnter(Collision collision)
    {
        Colliding = true;
        OnBoatCollider?.Invoke(this);
    }

    private void OnCollisionExit(Collision collision)
    {
        Colliding = false;
        OnBoatCollider?.Invoke(this);
    }

}
