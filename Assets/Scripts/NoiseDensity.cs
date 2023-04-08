using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseDensity : MonoBehaviour {

    [Header("Noise")]
    public int seed;
    public int numOctaves = 4;
    public float lacunarity = 2; // Measures frequency amplification
    public float persistence = .5f; // Measures amplitude amplification

    public float noiseScale = 0.032f;
    public float noiseWeight = 1;
    public bool closeEdges;
    public float floorOffset = 1;
    public float weightMultiplier = 1;

    public Vector4 shaderParams;

    const int threadGroupSize = 8;
    public ComputeShader densityShader;

    protected List<ComputeBuffer> buffersToRelease;

    private void OnValidate()
    {
        if (FindObjectOfType<MeshGenerator>())
        {
            FindObjectOfType<MeshGenerator>().RequestMeshUpdate();
        }
    }

    public ComputeBuffer Generate (ComputeBuffer pointsBuffer, int numPointsPerAxis, float boundsSize, Vector3 worldBounds, Vector3 centre, Vector3 offset, float spacing) {
        buffersToRelease = new List<ComputeBuffer> ();

        // Noise parameters
        var rand = new System.Random (seed);
        var offsets = new Vector3[numOctaves];
        float offsetRange = 1000;
        for (int i = 0; i < numOctaves; i++) {
            offsets[i] = new Vector3 ((float)rand.NextDouble () * 2 - 1, (float)rand.NextDouble () * 2 - 1, (float)rand.NextDouble () * 2 - 1) * offsetRange;
        }

        var offsetsBuffer = new ComputeBuffer (offsets.Length, sizeof (float) * 3);
        offsetsBuffer.SetData (offsets);
        buffersToRelease.Add (offsetsBuffer);

        densityShader.SetVector ("centre", new Vector4 (centre.x, centre.y, centre.z));
        densityShader.SetInt ("octaves", Mathf.Max (1, numOctaves));
        densityShader.SetFloat ("lacunarity", lacunarity);
        densityShader.SetFloat ("persistence", persistence);
        densityShader.SetFloat ("noiseScale", noiseScale);
        densityShader.SetFloat ("noiseWeight", noiseWeight);
        densityShader.SetBool ("closeEdges", closeEdges);
        densityShader.SetBuffer (0, "offsets", offsetsBuffer);
        densityShader.SetFloat ("floorOffset", floorOffset);
        densityShader.SetFloat ("weightMultiplier", weightMultiplier);

        densityShader.SetVector ("params", shaderParams);

        int numThreadsPerAxis = Mathf.CeilToInt(numPointsPerAxis / (float)threadGroupSize);
        densityShader.SetBuffer(0, "points", pointsBuffer);
        densityShader.SetInt("numPointsPerAxis", numPointsPerAxis);
        densityShader.SetFloat("boundsSize", boundsSize);
        densityShader.SetVector("centre", new Vector4(centre.x, centre.y, centre.z));
        densityShader.SetVector("offset", new Vector4(offset.x, offset.y, offset.z));
        densityShader.SetFloat("spacing", spacing);
        densityShader.SetVector("worldSize", worldBounds);

        densityShader.Dispatch(0, numThreadsPerAxis, numThreadsPerAxis, numThreadsPerAxis);

        if (buffersToRelease != null)
        {
            foreach (var b in buffersToRelease)
            {
                b.Release();
            }
        }

        return pointsBuffer;
    }
}