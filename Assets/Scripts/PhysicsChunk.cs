using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// -- REFACTOR REQUIRED -- \\

public class PhysicsChunk : MonoBehaviour
{
    //chunk size
    public const int chunkWidth = 16;
    public const int chunkHeight = 64;

    public bool updateChunk;
    public float counter;

    //0 = air, 1 = land
    public int[,,] blocks = new int[chunkWidth * 4, chunkHeight, chunkWidth * 4];

    public Node3D[,,] grid;

    public GameObject physicsObject;
    public GameObject physicsChunkPrefab;

    public TextureDatabase textureDatabase;

    void Start()
    {
        BuildMesh();
    }

    void Update()
    {
        transform.position = physicsObject.transform.position;
        transform.rotation = physicsObject.transform.rotation;

        if (updateChunk)
        {
            if (counter > 0.5)
            {
                BuildMesh();
                updateChunk = false;
                counter = 0;
            }
            counter += Time.deltaTime;
        }
    }

    bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > chunkWidth - 1 || z < 0 || z > chunkWidth - 1)
            return false;
        else
            return true;
    }

    int CheckBlock(Vector3Int pos)
    {
        if (!IsVoxelInChunk(pos.x, pos.y, pos.z))
            return 0;
        return blocks[pos.x, pos.y, pos.z];
    }

    Mesh mesh;

    public void BuildMesh()
    {
        if (!mesh) { mesh = new Mesh(); }

        mesh.Clear();

        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        List<Color> colors = new List<Color>();

        for (int x = 0; x < chunkWidth; x++)
            for (int z = 0; z < chunkWidth; z++)
                for (int y = 0; y < chunkHeight; y++)
                {
                    if (blocks[x, y, z] != 0)
                    {
                        Vector3 blockPos = new Vector3(x, y, z);
                        int numFaces = 0;

                        for (int i = 0; i < 6; i++)
                        {
                            Vector3 blockNeighbor = CubeMeshData.GetNeighbor(i, new Vector3(x, y, z));

                            if (blockNeighbor.y > -1 && blockNeighbor.y < chunkHeight)
                            {
                                if (CheckBlock(new Vector3Int((int)blockNeighbor.x, (int)blockNeighbor.y, (int)blockNeighbor.z)) == 0)
                                {
                                    verts.AddRange(CubeMeshData.faceVertices(i, blockPos));
                                    numFaces++;

                                    if (i > 1)
                                    {
                                        uvs.AddRange(textureDatabase.blocks[(int)blocks[x, y, z]].side);
                                    }

                                    if (i == 0)
                                    {
                                        uvs.AddRange(textureDatabase.blocks[(int)blocks[x, y, z]].top);
                                    }

                                    if (i == 1)
                                    {
                                        uvs.AddRange(textureDatabase.blocks[(int)blocks[x, y, z]].bottom);
                                    }
                                }
                            }
                        }

                        Color lightLevel = Color.white;

                        //lightLevel = UnityEngine.Random.ColorHSV();

                        //lightLevel.a = UnityEngine.Random.Range(0.0f, 1.0f);

                        lightLevel.a = 1;

                        int tl = verts.Count - 4 * numFaces;
                        for (int i = 0; i < numFaces; i++)
                        {
                            colors.Add(lightLevel);
                            colors.Add(lightLevel);
                            colors.Add(lightLevel);
                            colors.Add(lightLevel);

                            tris.AddRange(new int[] { tl + i * 4, tl + i * 4 + 1, tl + i * 4 + 2, tl + i * 4, tl + i * 4 + 2, tl + i * 4 + 3 });
                        }
                    }
                }

        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.colors = colors.ToArray();

        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        Physics.IgnoreCollision(physicsObject.GetComponent<MeshCollider>(), GetComponent<MeshCollider>());
        physicsObject.transform.parent = null;
        physicsObject.GetComponent<Rigidbody>().ResetCenterOfMass();
        physicsObject.GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    public void CheckForUnattachedBlocks(int x, int y, int z)
    {
        List<Vector3> initialSet = new List<Vector3>();
        HashSet<Vector3> completeClosedSet = new HashSet<Vector3>();

        for (int i = 0; i < 6; i++)
        {
            Vector3 blockNeighbor = CubeMeshData.GetNeighbor(i, new Vector3(x, y, z));
            if (CheckBlock(new Vector3Int((int)blockNeighbor.x, (int)blockNeighbor.y, (int)blockNeighbor.z)) != 0)
            {
                initialSet.Add(blockNeighbor);
            }
        }

        while (initialSet.Count > 0)
        {
            HashSet<Vector3> openSet = new HashSet<Vector3>();
            HashSet<Vector3> closedSet = new HashSet<Vector3>();
            HashSet<Vector3> iterator = new HashSet<Vector3>();

            Vector3 neighbor = initialSet[0];
            initialSet.Remove(neighbor);

            openSet.Add(neighbor);

            while (openSet.Count > 0)
            {
                iterator.UnionWith(openSet);

                foreach (Vector3 pos in iterator)
                {
                    openSet.Remove(pos);

                    if (closedSet.Contains(pos) || completeClosedSet.Contains(pos))
                    {
                        continue;
                    }

                    closedSet.Add(pos);

                    for (int i = 0; i < 6; i++)
                    {
                        Vector3 blockNeighbor = CubeMeshData.GetNeighbor(i, new Vector3(pos.x, pos.y, pos.z));
                        if (CheckBlock(new Vector3Int((int)blockNeighbor.x, (int)blockNeighbor.y, (int)blockNeighbor.z)) != 0)
                        {
                            openSet.Add(blockNeighbor);
                        }
                    }
                }
            }

            if (closedSet.Count > 0)
            {
                Debug.Log("Building Physics Chunk...");

                GameObject chunkGO = Instantiate(physicsChunkPrefab, transform.position, transform.rotation);
                PhysicsChunk physicsChunk = chunkGO.GetComponent<PhysicsChunk>();
                foreach (Vector3 pos in closedSet)
                {
                    completeClosedSet.Add(pos);
                    int blockType = blocks[(int)pos.x, (int)pos.y, (int)pos.z];
                    physicsChunk.blocks[(int)pos.x, (int)pos.y, (int)pos.z] = blockType;
                }

                GameObject physicsObjectGO = Instantiate(physicsObject, transform.position, transform.rotation);

                physicsChunk.physicsObject = physicsObjectGO;
                physicsChunk.enabled = true;

                Destroy(physicsObject);
                Destroy(gameObject);
            }
        }
        if (initialSet.Count < 1)
        {
            Destroy(physicsObject);
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        Destroy(GetComponent<MeshFilter>().sharedMesh);
    }
}
