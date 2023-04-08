using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    public enum GizmoType { Never, Always }

    public Boid prefab;
    public float spawnRadius = 10;
    public int spawnCount = 10;

    [Range(0.6f, 2f)]
    public float size = 1f;

    [Range(0f, 0.5f)]
    public float sizeVariation = 0f;

    public Color colour = Color.white;
    public GizmoType showSpawnRegion = GizmoType.Always;

    public Vector3Int coord;

    GameObject boidHolder;

    void Awake () {
        CreateBoidHolder();

        if(prefab != null)
        {
            for (int i = 0; i < spawnCount; i++)
            {
                instantiateBoid();
            }
        }
    }

    private void OnDrawGizmos () {
        if (showSpawnRegion == GizmoType.Always) {
            Gizmos.color = new Color(colour.r, colour.g, colour.b, 0.4f);
            Gizmos.DrawSphere(transform.position, spawnRadius);
        }
    }

    void instantiateBoid()
    {
        Boid boid = Instantiate(prefab);
        boid.transform.parent = boidHolder.transform;

        // Position and orientation of the boid
        boid.transform.position = Random.insideUnitSphere * spawnRadius + transform.position;
        boid.transform.forward = Random.insideUnitSphere;

        float randomVal = Random.Range(size - sizeVariation, size + sizeVariation);
        boid.transform.localScale = new Vector3(randomVal, randomVal, randomVal);

        boid.SetColour(colour);
    }

    public void SetUp(Boid boid)
    {
        if(prefab == null)
        {
            prefab = boid;
            for (int i = 0; i < spawnCount; i++)
            {
                instantiateBoid();
            }
        }
    }

    void CreateBoidHolder()
    {
        if (!boidHolder)
        {
            boidHolder = GameObject.Find("Boid Holder");
            if (!boidHolder)
            {
                boidHolder = new GameObject("Boid Holder");
            }
        }
    }

}