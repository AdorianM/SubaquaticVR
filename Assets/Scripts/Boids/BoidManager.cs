using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour {

    const int threadGroupSize = 1024;

    public BoidSettings settings;
    public ComputeShader compute;
    Boid[] boids;

    public struct BoidData
    {
        public Vector3 position;
        public Vector3 direction;

        public Vector3 alignment;
        public Vector3 centreOfCohesion;
        public Vector3 separation;
        public int boidCount;

        public static int Size
        {
            get
            {
                // Five Vector3 + one integer
                return sizeof(float) * 3 * 5 + sizeof(int);
            }
        }
    }

    void Start () {
    }

    void Update () {
        boids = FindObjectsOfType<Boid>();
        foreach (Boid boid in boids)
        {
            boid.Initialize(settings, null);
        }
        if (boids != null) {

            int boidCount = boids.Length;
            var boidData = new BoidData[boidCount];

            for (int i = 0; i < boidCount; i++) {
                boidData[i].direction = boids[i].forward;
                boidData[i].position = boids[i].position;
            }

            var boidBuffer = new ComputeBuffer (boidCount, BoidData.Size);
            boidBuffer.SetData (boidData);

            compute.SetBuffer (0, "boids", boidBuffer);
            compute.SetInt ("boidCount", boidCount);
            compute.SetFloat ("viewRadius", settings.perceptionRadius);
            compute.SetFloat ("avoidRadius", settings.avoidanceRadius);

            // Count of thread Groups (total boids / size of a group)
            int threadGroups = Mathf.CeilToInt (boidCount / (float) threadGroupSize);
            compute.Dispatch (0, threadGroups, 1, 1);

            boidBuffer.GetData (boidData);

            for (int i = 0; i < boidCount; i++) {
                boids[i].alignment = boidData[i].alignment;
                boids[i].centreOfCohesion = boidData[i].centreOfCohesion;
                boids[i].separation = boidData[i].separation;
                boids[i].neighbourCount = boidData[i].boidCount;

                boids[i].UpdateBoid ();
            }

            boidBuffer.Release ();
        }
    }
}