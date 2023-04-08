using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MeshGenerator : MonoBehaviour
{
    const int threadGroupSize = 8;

    [Header("General Settings")]
    public NoiseDensity densityGenerator;

    [Space()]
    public ComputeShader shader;
    public Material mat;

    [Header("Voxel Settings")]
    public float isoLevel;
    public float boundsSize = 1;
    public Vector3 offset = Vector3.zero;

    [Range(2, 100)]
    public int pointsPerAxis = 30;

    [Header("Gizmos")]
    public bool showBoundsGizmo = true;
    public Color boundsGizmoCol = Color.white;

    GameObject chunkHolder;
    const string chunkHolderName = "Chunks Holder";
    List<Chunk> chunks;
    Dictionary<Vector3Int, Chunk> existingChunks;
    Queue<Chunk> recycleableChunks;

    [Header("Map")]
    public bool fixedMapSize;
    public Vector3Int numChunks = Vector3Int.one;
    public Transform cam;
    public float viewDistance = 30;

    ComputeBuffer triangleBuffer;
    ComputeBuffer pointsBuffer;
    ComputeBuffer triCountBuffer;

    bool settingsUpdated;

    private void Awake()
    {
        // I have defined a few default chunks. If fixedMapSize is not true, we don't use those
        if (Application.isPlaying && !fixedMapSize)
        {
            chunks = new List<Chunk>();
            recycleableChunks = new Queue<Chunk>();
            existingChunks = new Dictionary<Vector3Int, Chunk>();

            Cleanup();
        }
    }

    private void Cleanup()
    {
        var currentChunks = FindObjectsOfType<Chunk>();
        for (int i = 0; i < currentChunks.Length; i++)
        {
            Destroy(currentChunks[i].gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if((Application.isPlaying && !fixedMapSize))
        {
            Run();
        }

        if(settingsUpdated)
        {
            RequestMeshUpdate();
            settingsUpdated = false;
        }
    }

    public void RequestMeshUpdate()
    {
        if ((Application.isPlaying) || (!Application.isPlaying))
        {
            Run();
        }
    }



    public void Run ()
    {
        CreateBuffers();

        if(!fixedMapSize && Application.isPlaying)
        {
            InitUnfixedChunks();
        } 
        else
        {
            InitFixedChunks();
        }

        if (!Application.isPlaying)
        {
            ReleaseBuffers();
        }
    }

    void CreateBuffers()
    {
        // Buffer precision (basically how many voxels will be in a cube + nr of triangles)
        int cubeCountPerAxis = pointsPerAxis - 1;
        int cubeCount = cubeCountPerAxis * cubeCountPerAxis * cubeCountPerAxis;

        int pointCount = pointsPerAxis * pointsPerAxis * pointsPerAxis;

        // There can be maximum 5 triangles per cube.
        int maxTriangleCount = cubeCount * 5;

        if (!Application.isPlaying || (pointsBuffer == null || pointCount != pointsBuffer.count))
        {
            if (Application.isPlaying)
            {
                ReleaseBuffers();
            }
            // public ComputeBuffer(int count, int stride, ComputeBufferType type);
            // A buffer of maxTriangleCount triangles of sizeof(int) * 3 * 3 size that can be appended
            //  - Size is made by 3 points represented in 3D space by 3 int coordinates
            triangleBuffer = new ComputeBuffer(maxTriangleCount, sizeof(int) * 9, ComputeBufferType.Append);
            // Position + density (3 coords + 1 float)
            pointsBuffer = new ComputeBuffer(pointCount, sizeof(int) * 4);
            // Value that determines which triangles to be selected from the tables 
            // (int value which tells what edges are below the isosurface)
            triCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.Raw);
        }
    }

    void ReleaseBuffers()
    {
        if (triangleBuffer != null)
        {
            triangleBuffer.Release();
            pointsBuffer.Release();
            triCountBuffer.Release();
        }
    }

    void InitFixedChunks()
    {
        CreateChunkHolder();
        chunks = new List<Chunk>();
        List<Chunk> oldChunks = new List<Chunk>(FindObjectsOfType<Chunk>());
        int count = 0;

        for (int x = 0; x < numChunks.x; x++)
        {
            for (int y = 0; y < numChunks.y; y++)
            {
                for (int z = 0; z < numChunks.z; z++)
                {
                    Vector3Int coord = new Vector3Int(x, y, z);
                    bool chunkExists = false;

                    for (int i = 0; i < oldChunks.Count && !chunkExists; i++)
                    {
                        if (oldChunks[i].coord == coord)
                        {
                            chunks.Add(oldChunks[i]);
                            oldChunks.RemoveAt(i);
                            chunkExists = true;
                        }
                    }

                    if (!chunkExists)
                    {
                        var newChunk = CreateChunk(coord);
                        chunks.Add(newChunk);
                    }

                    UpdateChunkMesh(chunks[count]);
                    count++;
                }
            }
        }

        // Delete all unused chunks
        for (int i = 0; i < oldChunks.Count; i++)
        {
            oldChunks[i].DestroyOrDisable();
        }
    }

    void CreateChunkHolder()
    {
        if (!chunkHolder)
        {
            chunkHolder = GameObject.Find(chunkHolderName);
            if (!chunkHolder)
            {
                chunkHolder = new GameObject(chunkHolderName);
            }
        }
    }

    Chunk CreateChunk(Vector3Int coord)
    {
        GameObject chunkGO = new GameObject($"Chunk ({coord.x}, {coord.y}, {coord.z})");
        chunkGO.transform.parent = chunkHolder.transform;

        Chunk chunk = chunkGO.AddComponent<Chunk>();
        chunk.coord = coord;
        return chunk;
    }

    public void UpdateChunkMesh(Chunk chunk)
    {
        int cubeCountPerAxis = pointsPerAxis - 1;
        int threadCountPerAxis = Mathf.CeilToInt(cubeCountPerAxis / (float)threadGroupSize);

        float pointSpacing = boundsSize / (pointsPerAxis - 1);

        Vector3Int coord = chunk.coord;
        Vector3 centre = CentreFromCoord(coord);
        Vector3 worldBounds = new Vector3(numChunks.x, numChunks.y, numChunks.z) * boundsSize;

        densityGenerator.Generate(pointsBuffer, pointsPerAxis, boundsSize, worldBounds, centre, offset, pointSpacing);

        triangleBuffer.SetCounterValue(0);
        shader.SetBuffer(0, "points", pointsBuffer);
        shader.SetBuffer(0, "triangles", triangleBuffer);
        shader.SetInt("pointsPerAxis", pointsPerAxis);
        shader.SetFloat("isoLevel", isoLevel);

        shader.Dispatch(0, threadCountPerAxis, threadCountPerAxis, threadCountPerAxis);

        ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
        int[] triCountArray = { 0 };
        triCountBuffer.GetData(triCountArray);
        int triCount = triCountArray[0];

        // Get triangle data from shader
        Triangle[] tris = new Triangle[triCount];
        triangleBuffer.GetData(tris, 0, 0, triCount);

        Mesh mesh = chunk.mesh;
        mesh.Clear();

        var vertices = new Vector3[triCount * 3];
        var meshTriangles = new int[triCount * 3];

        for (int i = 0; i < triCount; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                meshTriangles[i * 3 + j] = i * 3 + j;
                vertices[i * 3 + j] = tris[i][j];
            }
        }
        mesh.vertices = vertices;
        mesh.triangles = meshTriangles;

        mesh.RecalculateNormals();
    }

    struct Triangle
    {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public Vector3 this[int i]
        {
            get
            {
                if(i == 0)
                {
                    return a;
                } else if(i == 1)
                {
                    return b;
                } else
                {
                    return c;
                }
            }
        }
    }

    void InitUnfixedChunks()
    {
        if (chunks == null)
        {
            return;
        }
        CreateChunkHolder();

        int maxChunksInView = Mathf.CeilToInt(viewDistance / boundsSize);
        float diagonalViewDistance = viewDistance * viewDistance;

        Vector3 cameraPos = cam.position;
        Vector3 positionInChunk = cameraPos / boundsSize;
        Vector3Int cameraIntPos = new Vector3Int(Mathf.RoundToInt(positionInChunk.x), 
                                                Mathf.RoundToInt(positionInChunk.y), 
                                                Mathf.RoundToInt(positionInChunk.z));

        RecycleOutOfDistance(diagonalViewDistance);

        for (int x = -maxChunksInView; x <= maxChunksInView; x++)
        {
            for (int y = -maxChunksInView; y <= maxChunksInView; y++)
            {
                for (int z = -maxChunksInView; z <= maxChunksInView; z++)
                {
                    Vector3Int coord = new Vector3Int(x, y, z) + cameraIntPos;

                    if (!existingChunks.ContainsKey(coord))
                    {
                        float chunkDistance = GetDistanceToChunkCoord(coord);
                        if (chunkDistance <= diagonalViewDistance)
                        {
                            Bounds bounds = new Bounds(CentreFromCoord(coord), Vector3.one * boundsSize);
                            if (IsVisibleFrom(bounds, Camera.main))
                            {
                                Chunk chunk = null;
                                if (recycleableChunks.Count > 0)
                                {
                                    chunk = recycleableChunks.Dequeue();
                                }
                                else
                                {
                                    chunk = CreateChunk(coord);
                                }

                                chunk.coord = coord;
                                existingChunks.Add(coord, chunk);
                                chunks.Add(chunk);
                                UpdateChunkMesh(chunk);
                            }
                        }
                    }
                }
            }
        }
    }

    Vector3 CentreFromCoord(Vector3Int coord)
    {
        if (fixedMapSize)
        {
            Vector3 totalBounds = (Vector3)numChunks * boundsSize;
            return -totalBounds / 2 + (Vector3)coord * boundsSize + Vector3.one * boundsSize / 2;
        }

        return new Vector3(coord.x, coord.y, coord.z) * boundsSize;
    }

    void RecycleOutOfDistance(float maxDistance)
    {
        for (int i = 0; i < chunks.Count; i++)
        {
            float chunkDistance = GetDistanceToChunkCoord(chunks[i].coord);
            if (maxDistance < chunkDistance)
            {
                RecycleChunkAtPosition(chunks[i], i);
            }
        }
    }

    float GetDistanceToChunkCoord(Vector3Int chunkCoord)
    {
        Vector3 result = new Vector3(0f, 0f, 0f);
        Vector3 cameraOffset = cam.position - CentreFromCoord(chunkCoord);
        Vector3 origin = new Vector3(Mathf.Abs(cameraOffset.x), Mathf.Abs(cameraOffset.y), Mathf.Abs(cameraOffset.z)); 
        origin -= Vector3.one * boundsSize / 2;

        result.x = origin.x > 0 ? origin.x : 0;
        result.y = origin.y > 0 ? origin.y : 0;
        result.z = origin.z > 0 ? origin.z : 0;
        return result.sqrMagnitude;
    }

    void RecycleChunkAtPosition(Chunk chunk, int pos)
    {
        existingChunks.Remove(chunk.coord);
        recycleableChunks.Enqueue(chunk);
        chunks.RemoveAt(pos);
    }

    public bool IsVisibleFrom(Bounds bounds, Camera camera)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }

    void OnDrawGizmos()
    {
        if (showBoundsGizmo)
        {
            Gizmos.color = boundsGizmoCol;

            List<Chunk> chunks = (this.chunks == null) ? new List<Chunk>(FindObjectsOfType<Chunk>()) : this.chunks;
            foreach (var chunk in chunks)
            {
                Bounds bounds = new Bounds(CentreFromCoord(chunk.coord), Vector3.one * boundsSize);
                Gizmos.color = boundsGizmoCol;
                Gizmos.DrawWireCube(CentreFromCoord(chunk.coord), Vector3.one * boundsSize);
            }
        }
    }

}
