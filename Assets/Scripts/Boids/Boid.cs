using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour {

    BoidSettings settings;

    // State
    [HideInInspector]
    public Vector3 position;
    [HideInInspector]
    public Vector3 forward;
    Vector3 velocity;

    // To update:
    Vector3 acceleration;
    [HideInInspector]
    public Vector3 alignment;
    [HideInInspector]
    public Vector3 separation;
    [HideInInspector]
    public Vector3 centreOfCohesion;
    [HideInInspector]
    public int neighbourCount;
    [HideInInspector]
    public Transform targetFromVR;

    // Cached
    Material material;
    Transform cachedTransform;
    Transform target;

    void Awake () {
        material = transform.GetComponentInChildren<MeshRenderer> ().material;
        cachedTransform = transform;
    }

    public void Initialize (BoidSettings settings, Transform target) {
        this.target = target;
        this.settings = settings;

        position = cachedTransform.position;
        forward = cachedTransform.forward;

        float startSpeed = settings.minSpeed;
        velocity = forward * startSpeed;
    }

    public void SetColour (Color col) {
        if (material != null) {
            material.color = col;
        }
    }

    public void UpdateBoid () {
        Vector3 acceleration = Vector3.zero;

        acceleration = FollowTarget();
        acceleration = AdaptToNeighbours(acceleration);
        acceleration = AvoidCollisions(acceleration);

        // Reconstruct velocity according to speed boundaries
        velocity += acceleration * Time.deltaTime;
        Vector3 dir = velocity / velocity.magnitude;
        float speed = Mathf.Clamp (velocity.magnitude, settings.minSpeed, settings.maxSpeed);
        velocity = dir * speed;

        // Update positions
        cachedTransform.position += velocity * Time.deltaTime;
        cachedTransform.forward = dir;
        position = cachedTransform.position;
        forward = dir;
    }



    Vector3 AvoidCollisions(Vector3 acc)
    {
        if (WillCollide ()) {
            Vector3 collisionAvoidDir = ObstacleRays ();
            Vector3 collisionAvoidForce = SteerTowards (collisionAvoidDir) * settings.avoidCollisionWeight;
            acc += collisionAvoidForce;
        }
        return acc;
    }

    bool WillCollide()
    {
        if (Physics.SphereCast(position, settings.boundsRadius, forward, out _, settings.collisionAvoidDst, settings.obstacleMask))
        {
            return true;
        }
        return false;
    }

    Vector3 ObstacleRays () {
        Vector3[] rays = BoidHelper.directions;

        for (int i = 0; i < rays.Length; i++) {
            Vector3 dir = cachedTransform.TransformDirection (rays[i]);
            Ray ray = new Ray (position, dir);

            if (!Physics.SphereCast (ray, settings.boundsRadius, settings.collisionAvoidDst, settings.obstacleMask)) {
                return dir;
            }
        }

        return forward;
    }

    Vector3 AdaptToNeighbours(Vector3 acc)
    {
        if (neighbourCount != 0)
        {
            centreOfCohesion /= neighbourCount;

            Vector3 offsetToCentre = (centreOfCohesion - position);
            Vector3 alignmentForce = SteerTowards(alignment) * settings.alignWeight;
            Vector3 cohesionForce = SteerTowards(offsetToCentre) * settings.cohesionWeight;
            Vector3 seperationForce = SteerTowards(separation) * settings.seperateWeight;

            acc += alignmentForce;
            acc += cohesionForce;
            acc += seperationForce;
        }

        return acc;
    }

    Vector3 FollowTarget()
    {
        if(isBaited())
        {
            Vector3 offset = targetFromVR.position - position;
            return SteerTowards(offset) * settings.targetWeight;
        }

        if (target != null)
        {
            Vector3 offset = target.position - position;
            return SteerTowards(offset) * settings.targetWeight;
        }

        return Vector3.zero;
    }

    Vector3 SteerTowards (Vector3 vector) {
        Vector3 v = vector.normalized * settings.maxSpeed - velocity;
        return Vector3.ClampMagnitude (v, settings.maxSteerForce);
    }

    bool isBaited()
    {
        if(targetFromVR != null)
        {
            return !(targetFromVR.position.x == 0 && targetFromVR.position.y == 0 && targetFromVR.position.z == 0);
        }

        return false;
    }

}