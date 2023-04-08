using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bait : MonoBehaviour
{
    private bool enableTargeting;
    private float radius = 10f;

    public GameObject shape;
    private Boid[] boids;

    void Start()
    {
        boids = FindObjectsOfType<Boid>();
        shape = Instantiate(shape, transform.position, Quaternion.identity, transform);
        enableTargeting = false;
    }

    void Update()
    {
        if (enableTargeting && boids != null)
        {
            shape.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + radius);
            Vector3 pos = RotateAroundPivot(shape.transform.position, transform.position, transform.rotation.eulerAngles);
            shape.transform.position = pos;

            foreach (Boid b in boids)
            {
                b.targetFromVR = shape.transform;
                b.UpdateBoid();
            }
        } else
        {
            shape.transform.position = new Vector3(0, 0, 0);
        }
    }

    public void AttachBait(bool value)
    {
        enableTargeting = value;
    }

    Vector3 RotateAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        Vector3 dir = point - pivot;
        dir = Quaternion.Euler(angles) * dir;
        point = dir + pivot;
        return point;
    }
}
