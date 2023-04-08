using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerGenerator : MonoBehaviour
{
    // Don't spread spawners. Spread invisible walls.
    List<Spawner> spawners;
    Dictionary<Vector3Int, Spawner> existingSpawners;
    Queue<Spawner> recycleableSpawners;

    GameObject spawnerHolder;

    public Transform cam;
    public Boid boid;
    public float viewDistance;
    public float boundsSize = 20f;

    private void Awake()
    {
        // I have defined a few default chunks. If fixedMapSize is not true, we don't use those
        if (Application.isPlaying)
        {
            spawners = new List<Spawner>();
            recycleableSpawners = new Queue<Spawner>();
            existingSpawners = new Dictionary<Vector3Int, Spawner>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Application.isPlaying)
        {
            InitSpawners();
        }
    }

    void InitSpawners()
    {

        CreateSpawnerHolder();
        if (spawners == null)
        {
            return;
        }

        int maxSpawnersInView = Mathf.CeilToInt(viewDistance / boundsSize);
        float diagonalViewDistance = viewDistance * viewDistance;

        Vector3 cameraPos = cam.position;
        Vector3 positionInBounds = cameraPos / boundsSize;
        Vector3Int cameraIntPos = new Vector3Int(Mathf.RoundToInt(positionInBounds.x),
                                                Mathf.RoundToInt(positionInBounds.y),
                                                Mathf.RoundToInt(positionInBounds.z));

        // You could remove the object and its children. That way you get rid of boids and spawners
        RecycleOutOfDistance(diagonalViewDistance);

        for (int x = -maxSpawnersInView; x <= maxSpawnersInView; x++)
        {
            for (int z = -maxSpawnersInView; z <= maxSpawnersInView; z++)
            {
                Vector3Int coord = new Vector3Int(x, 1, z) + cameraIntPos;

                float spawnerDistance = GetDistanceToSpawnerCoord(coord);

                if (!existingSpawners.ContainsKey(coord))
                {
                    if (spawnerDistance <= diagonalViewDistance)
                    {
                        Bounds bounds = new Bounds(new Vector3(coord.x, coord.y, coord.z) * boundsSize, Vector3.one * boundsSize);

                        if (IsVisibleFrom(bounds, Camera.main))
                        {
                            Spawner spawner = null;

                            if (recycleableSpawners.Count > 0)
                            {
                                spawner = recycleableSpawners.Dequeue();
                            }
                            else
                            {
                                spawner = CreateSpawner(coord);
                            }

                            spawner.coord = coord;
                            existingSpawners.Add(coord, spawner);
                            spawners.Add(spawner);
                        }
                    }
                }
            }
        }
    }

    void CreateSpawnerHolder()
    {
        if (!spawnerHolder)
        {
                spawnerHolder = GameObject.Find("Spawner Holder");
            if (!spawnerHolder)
            {
                    spawnerHolder = new GameObject("Spawner Holder");
            }
        }
    }

    float GetDistanceToSpawnerCoord(Vector3Int spawnerCoord)
    {
        Vector3 result = new Vector3(0f, 0f, 0f);
        Vector3 cameraOffset = cam.position - new Vector3(spawnerCoord.x, spawnerCoord.y, spawnerCoord.z) * boundsSize;
        Vector3 origin = new Vector3(Mathf.Abs(cameraOffset.x), Mathf.Abs(cameraOffset.y), Mathf.Abs(cameraOffset.z));
        origin -= Vector3.one * boundsSize / 2;

        result.x = origin.x > 0 ? origin.x : 0;
        result.y = origin.y > 0 ? origin.y : 0;
        result.z = origin.z > 0 ? origin.z : 0;

        return result.sqrMagnitude;
    }

    void RecycleOutOfDistance(float maxDistance)
    {
        for (int i = 0; i < spawners.Count; i++)
        {
            float chunkDistance = GetDistanceToSpawnerCoord(spawners[i].coord);
            if (maxDistance < chunkDistance)
            {
                RecycleSpawnerAtPosition(spawners[i], i);
            }
        }
    }

    void RecycleSpawnerAtPosition(Spawner spawner, int pos)
    {
        existingSpawners.Remove(spawner.coord);
        recycleableSpawners.Enqueue(spawner);
        spawners.RemoveAt(pos);
    }

    Spawner CreateSpawner(Vector3Int coord)
    {
        GameObject spawnerGO = new GameObject($"Spawner ({coord.x}, {coord.y}, {coord.z})");
        spawnerGO.transform.parent = spawnerHolder.transform;

        Spawner spawner = spawnerGO.AddComponent<Spawner>();
        spawner.transform.position = new Vector3(coord.x * boundsSize, coord.y, coord.z * boundsSize);
        spawner.SetUp(boid);
        //spawner.coord = coord;
        return spawner;
    }

    bool IsVisibleFrom(Bounds bounds, Camera camera)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }

    void Cleanup()
    {
        /*var currentChunks = FindObjectsOfType<Chunk>();
        for (int i = 0; i < currentChunks.Length; i++)
        {
            Destroy(currentChunks[i].gameObject);
        }*/
    }
}
