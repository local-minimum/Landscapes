using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaViewCamera : MonoBehaviour
{
    public Geography geography;
    public float angleSpeed = 5f;
    
    float angle;
    float planarDistance;
    float focusPointDistance;
    float focusPointAngleOffset;

    private void Start()
    {
        // Camera to center of geography
        var planarOffset = new Vector2(
            transform.position.z - geography.transform.position.z,
            transform.position.x - geography.transform.position.x
        );
        angle = Vector2.Angle(Vector2.right, planarOffset);
        planarDistance = planarOffset.magnitude;

        // Where on geography plane camera looks
        var plane = new Plane(geography.transform.up * -1, geography.transform.position);
        var ray = new Ray(transform.position, transform.forward);
        float distance;
        if (plane.Raycast(ray, out distance))
        {
            var focusPointOffset = ray.GetPoint(distance) - geography.transform.position;
            focusPointDistance = focusPointOffset.magnitude;
            focusPointAngleOffset = Mathf.Atan2(focusPointOffset.z, focusPointOffset.x) * Mathf.Rad2Deg - angle;
        } else
        {
            Debug.LogWarning("AreaViewCamera is not looking at geography, will assume focus on center.");
        }
    }

    private void Update()
    {
        angle += Time.deltaTime * angleSpeed;
        angle %= 360;
        var x = planarDistance * Mathf.Cos(angle * Mathf.Deg2Rad);
        var z = planarDistance * Mathf.Sin(angle * Mathf.Deg2Rad);
        transform.position = new Vector3(x, transform.position.y, z);
        x = focusPointDistance * Mathf.Cos((angle + focusPointAngleOffset) * Mathf.Deg2Rad);
        z = focusPointDistance * Mathf.Sin((angle + focusPointAngleOffset) * Mathf.Deg2Rad);
        transform.LookAt(new Vector3(x, geography.transform.position.y, z), Vector3.up);
    }
}
