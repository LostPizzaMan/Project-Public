using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public class TerrainChunk : MonoBehaviour
{
    // ---- Chunk Settings ---- //   

    public const int chunkWidth = 16;
    public const int chunkHeight = 64;

    public bool smoothLighting;
    public bool AINodeGeneration;
    public bool drawAINodes;

    // ---- Chunk Status ---- //   

    [Header("Main")]
    public bool generatingFinished;
    public bool lightingFinished;
    public bool updateFlag;
    public bool quickUpdateFlag;

    public bool isUpdating;
    public bool threadLocked;

    float counter;

    // ---- Chunk Data ---- //   

    //0 = air, 1 = land
    public int[,,] blocks = new int[chunkWidth, chunkHeight, chunkWidth];

    public int[,,] lightMap = new int[chunkWidth, chunkHeight, chunkWidth];

    public Node3D[,,] grid;

    // ---- Other ---- //   

    public Vector3Int chunkPos3D { get; private set; }
    public Vector2Int chunkPos2D { get; private set; }

    public VoxelLightEngine voxelLightEngine;
    public TextureDatabase textureDatabase;
    public TerrainGenerator terrainGenerator;

    public TerrainChunk[] terrainChunks { get; private set; }

    // ---- Mesh Data ---- //   

    [Header("Mesh Filters")]
    public MeshFilter waterMeshFilter = null;
    public MeshFilter meshFilter = null;
    public MeshFilter transparentMeshFilter = null;

    [Header("Mesh Renderer")]
    [SerializeField] public Renderer rend = null;
    [SerializeField] public Renderer transparentRend = null;
    [SerializeField] private GameObject TransparentMesh = null;

    float offset16x;
    float offset32x;
    float offset64x;
    float offset128x;

    [Header("Chunk Setup")]
    public bool ranSearchPhase;
    public bool ranLightPhase;
    public bool ranMeshPhase;

    public string chunkID;

    void Start()
    {
        mesh = new Mesh();
        transparentMesh = new Mesh();

        int textureAtlasHeight = textureDatabase.itemDatabase.TextureAtlas.height;

        offset16x = 1.0f / (textureAtlasHeight / 16);
        offset32x = 1.0f / (textureAtlasHeight / 32);
        offset64x = 1.0f / (textureAtlasHeight / 64);
        offset128x = 1.0f / (textureAtlasHeight / 128);
    }

    void Update()
    {
        if (quickUpdateFlag) 
        {
            if (!isUpdating)
            {
                BuildMesh();
                quickUpdateFlag = false;
            }
        }

        if (updateFlag && !quickUpdateFlag) 
        {
            if (counter > 0.2f)
            {
                if (!isUpdating)
                {
                    BuildMesh();
                    updateFlag = false;
                }
                counter = 0;
            }
            counter += Time.deltaTime;
        }
    }

    public void SetupChunkCache()
    {
        terrainChunks = new TerrainChunk[8];
    }

    public void Initialize(int xPos, int zPos)
    {
        chunkPos3D = new Vector3Int(xPos, 0, zPos);
        chunkPos2D = new Vector2Int(xPos, zPos);
        generatingFinished = true;
    }

    public void AreNeighboursChunksLoaded()
    {
        for (int i = 0; i < 8; i++)
        {
            TerrainChunk neighbourChunk = terrainChunks[i];

            if (neighbourChunk is null || !neighbourChunk.generatingFinished)
            {
                return;
            }
        }

        ranLightPhase = true;
        SunlightCast();
    }

    public void AreNeighboursLightLoaded()
    {
        for (int i = 0; i < 8; i++)
        {
            TerrainChunk neighbourChunk = terrainChunks[i];

            if (neighbourChunk is null || !neighbourChunk.lightingFinished)
            {
                return;
            }
        }

        ranMeshPhase = true;
        BuildMesh();
    }

    public int GetBlockType(TerrainChunk tc, int x, int y, int z)
    {
        if (x == -1)
        {
            tc = tc.terrainChunks[2];
            x += 16;
        }
        else if (x == 16)
        {
            tc = tc.terrainChunks[3];
            x -= 16;
        }
        else if (z == -1)
        {
            tc = tc.terrainChunks[1];
            z += 16;
        }
        else if (z == 16)
        {
            tc = tc.terrainChunks[0];
            z -= 16;
        }

        if (tc.generatingFinished)
        {
            return tc.blocks[x, y, z];
        }

        Debug.Log("Grabbed block type too early. Defaulting to BlockType Air");

        return 0;
    }

    // ---- SETTING UP AI NODES ---- //

    public void CreateNodeGrid()
    {
        grid = new Node3D[16, 64, 16];

        for (int x = 0; x < chunkWidth; x++)
            for (int z = 0; z < chunkWidth; z++)
                for (int y = 0; y < chunkHeight; y++)
                {
                    grid[x, y, z] = GetAINode(x, y, z);
                }
    }

    Node3D GetAINode(int x, int y, int z)
    {
        if (blocks[x, y, z] != 0)
        {
            if (y > -1 && y < chunkHeight - 2)
            {
                if (blocks[x, y + 1, z] == 0 && blocks[x, y + 2, z] == 0)
                {
                    if (textureDatabase.itemDatabase.blocks[blocks[x, y, z]].rendertype != 1)
                    {
                        return new Node3D(true, x, y, z);
                    }
                }
            }
        }

        return new Node3D(false, x, y, z);
    }

    // ---- SETTING UP CHUNK LIGHTING ---- //

    async void SunlightCast()
    {
        isUpdating = true;

        await Task.Run(() =>
        {
            for (int x = 0; x < TerrainChunk.chunkWidth; x++)
            {
                for (int z = 0; z < TerrainChunk.chunkWidth; z++)
                {
                    SunlightCastColumn(x, z);
                }
            }

            voxelLightEngine.CalculateLight();      
        });

        voxelLightEngine.doSunlightChunkUpdate = true;
        lightingFinished = true;

        isUpdating = false;
    }

    public void SunlightCastColumn(int x, int z)
    {
        for (int y = TerrainChunk.chunkHeight - 1; y > -1; y--)
        {
            if (blocks[x, y, z] == 0) // Air
            {
                voxelLightEngine.sunlightUpdateQueue.Enqueue(new VoxelLightEngine.LightNode(this, new Vector3Int(x, y, z), 15));
                VoxelLightHelper.SetSunlight(this, new Vector3Int(x, y, z), 15);
            }

            if (blocks[x, y, z] != 0) // Air
            {            
                break;
            }
        }
    }

    // ---- SETTING UP CHUNK MESH ---- //

    List<Vector3> verts = new List<Vector3>();
    List<int> tris = new List<int>();
    List<Vector2> uvs = new List<Vector2>();
    List<Color> colors = new List<Color>();

    List<Vector3> transparentVerts = new List<Vector3>();
    List<int> transparentTris = new List<int>();
    List<Vector2> transparentUvs = new List<Vector2>();
    List<Color> transparentColors = new List<Color>();

    Mesh mesh;
    Mesh transparentMesh;

    async void BuildMesh()
    {
        isUpdating = true;

        ClearMesh();

        await Task.Run(SetupMeshData);

        CreateMesh();

        rend.enabled = true;
        transparentRend.enabled = true;

        isUpdating = false;
    }

    void ClearMesh()
    {
        verts.Clear();
        tris.Clear();
        uvs.Clear();
        colors.Clear();

        transparentVerts.Clear();
        transparentTris.Clear();
        transparentUvs.Clear();
        transparentColors.Clear();
    }

    void SetupMeshData()
    {
        threadLocked = true;

        for (int x = 0; x < chunkWidth; x++)
            for (int z = 0; z < chunkWidth; z++)
                for (int y = 0; y < chunkHeight; y++)
                {
                    if (blocks[x, y, z] == 0) // Skip meshing for air blocks
                    {
                        continue;
                    }

                    Vector3 blockPos = new Vector3(x, y, z);
                    BlockBase blockType = textureDatabase.itemDatabase.blocks[blocks[x, y, z]];

                    if (blockType.rendertype == 0)
                    {
                        AddCube(blockPos, x, y, z);
                    }
                    else if (blockType.rendertype == 1)
                    {
                        AddCross(blockPos, x, y, z);
                    }
                }

        threadLocked = false;
    }

    void CreateMesh()
    {
        mesh.Clear();
        transparentMesh.Clear();

        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.colors = colors.ToArray();

        transparentMesh.vertices = transparentVerts.ToArray();
        transparentMesh.triangles = transparentTris.ToArray();
        transparentMesh.uv = transparentUvs.ToArray();
        transparentMesh.colors = transparentColors.ToArray();

        mesh.RecalculateNormals();
        transparentMesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        TransparentMesh.GetComponent<MeshFilter>().mesh = transparentMesh;
        TransparentMesh.GetComponent<MeshCollider>().sharedMesh = transparentMesh;

        if (AINodeGeneration)
        {
            CreateNodeGrid();
        }
    }

    void AddCube(Vector3 blockPos, int x, int y, int z)
    {
        int numFaces = 0;

        for (int i = 0; i < 6; i++)
        {
            Vector3 blockNeighbor = CubeMeshData.GetNeighbor(i, new Vector3(x, y, z)); // Grabbing neighbor blocks
            Vector3Int blockNeighborInt = new Vector3Int((int)blockNeighbor.x, (int)blockNeighbor.y, (int)blockNeighbor.z);

            if (blockNeighbor.y > -1 && blockNeighbor.y < chunkHeight)
            {
                BlockBase blockType = textureDatabase.itemDatabase.blocks[GetBlockType(this, (int)blockNeighbor.x, (int)blockNeighbor.y, (int)blockNeighbor.z)];

                if (blockType.transparent)
                {
                    verts.AddRange(CubeMeshData.faceVertices(i, blockPos));

                    CaluclateLightLevelForFace(blockNeighborInt, (int)blockNeighbor.x, (int)blockNeighbor.y, (int)blockNeighbor.z, i);

                    numFaces++;

                    int blockID = blocks[x, y, z];


                    if (i > 1)
                    {
                        uvs.AddRange(textureDatabase.blocks[blockID].side);
                    }

                    if (i == 0)
                    {
                        uvs.AddRange(textureDatabase.blocks[blockID].top);
                    }

                    if (i == 1)
                    {
                        uvs.AddRange(textureDatabase.blocks[blockID].bottom);
                    }
                }
            }
        }

        int tl = verts.Count - 4 * numFaces;
        for (int i = 0; i < numFaces; i++)
        {
            int trisIndex = tl + i * 4; 

            tris.AddRange(new int[] { trisIndex, trisIndex + 1, trisIndex + 2, trisIndex, trisIndex + 2, trisIndex + 3 });
        }
    }

    void AddCross(Vector3 blockPos, int x, int y, int z)
    {
        int transparenNumFaces = 0;

        for (int i = 0; i < 4; i++)
        {
            transparentVerts.AddRange(CubeMeshData.crossFaceVertices(i, blockPos));

            Color lightLevel = Color.white;
            lightLevel.a = 0;

            lightLevel.a = VoxelLightHelper.GetSunlight(this, new Vector3Int(x, y, z)) / 15f;
            lightLevel.r = VoxelLightHelper.GetRedLight(this, new Vector3Int(x, y, z)) / 15f;
            lightLevel.g = VoxelLightHelper.GetGreenLight(this, new Vector3Int(x, y, z)) / 15f;
            lightLevel.b = VoxelLightHelper.GetBlueLight(this, new Vector3Int(x, y, z)) / 15f;

            transparentColors.Add(lightLevel);
            transparentColors.Add(lightLevel);
            transparentColors.Add(lightLevel);
            transparentColors.Add(lightLevel);

            transparenNumFaces++;

            transparentUvs.AddRange(textureDatabase.blocks[blocks[x, y, z]].side);
        }

        int tl = transparentVerts.Count - 4 * transparenNumFaces;
        for (int i = 0; i < transparenNumFaces; i++)
        {
            int trisIndex = tl + i * 4;

            transparentTris.AddRange(new int[] { trisIndex, trisIndex + 1, trisIndex + 2, trisIndex, trisIndex + 2, trisIndex + 3 });
        }
    }

    void CaluclateLightLevelForFace(Vector3Int blockNeighbor, int x, int y, int z, int i)
    {
        if (!smoothLighting)
        {
            Color lightLevel = FlatLighting.GetLightValue(this, blockNeighbor, x, z);

            colors.Add(lightLevel);
            colors.Add(lightLevel);
            colors.Add(lightLevel);
            colors.Add(lightLevel);
        }
        else
        {
            if (i == 0)
            {
                colors.Add(SmoothLighting.GetAverageLightValueForTop(this, x, y, z, 0));
                colors.Add(SmoothLighting.GetAverageLightValueForTop(this, x, y, z, 1));
                colors.Add(SmoothLighting.GetAverageLightValueForTop(this, x, y, z, 2));
                colors.Add(SmoothLighting.GetAverageLightValueForTop(this, x, y, z, 3));
            }
            else if (i == 1)
            {
                colors.Add(SmoothLighting.GetAverageLightValueForBottom(this, x, y, z, 0));
                colors.Add(SmoothLighting.GetAverageLightValueForBottom(this, x, y, z, 1));
                colors.Add(SmoothLighting.GetAverageLightValueForBottom(this, x, y, z, 2));
                colors.Add(SmoothLighting.GetAverageLightValueForBottom(this, x, y, z, 3));
            }
            else if (i == 2)
            {
                colors.Add(SmoothLighting.GetAverageLightValueForFront(this, x, y, z, 0));
                colors.Add(SmoothLighting.GetAverageLightValueForFront(this, x, y, z, 1));
                colors.Add(SmoothLighting.GetAverageLightValueForFront(this, x, y, z, 2));
                colors.Add(SmoothLighting.GetAverageLightValueForFront(this, x, y, z, 3));
            }
            else if (i == 3)
            {
                colors.Add(SmoothLighting.GetAverageLightValueForRight(this, x, y, z, 0));
                colors.Add(SmoothLighting.GetAverageLightValueForRight(this, x, y, z, 1));
                colors.Add(SmoothLighting.GetAverageLightValueForRight(this, x, y, z, 2));
                colors.Add(SmoothLighting.GetAverageLightValueForRight(this, x, y, z, 3));
            }
            else if (i == 4)
            {
                colors.Add(SmoothLighting.GetAverageLightValueForBack(this, x, y, z, 0));
                colors.Add(SmoothLighting.GetAverageLightValueForBack(this, x, y, z, 1));
                colors.Add(SmoothLighting.GetAverageLightValueForBack(this, x, y, z, 2));
                colors.Add(SmoothLighting.GetAverageLightValueForBack(this, x, y, z, 3));
            }
            else if (i == 5)
            {
                colors.Add(SmoothLighting.GetAverageLightValueForLeft(this, x, y, z, 0));
                colors.Add(SmoothLighting.GetAverageLightValueForLeft(this, x, y, z, 1));
                colors.Add(SmoothLighting.GetAverageLightValueForLeft(this, x, y, z, 2));
                colors.Add(SmoothLighting.GetAverageLightValueForLeft(this, x, y, z, 3));
            }
        }
    }

    public void OnDrawGizmos()
    {
        if (!drawAINodes) { return; }

        for (int x = 0; x < TerrainChunk.chunkWidth; x++)
            for (int z = 0; z < TerrainChunk.chunkWidth; z++)
                for (int y = 0; y < TerrainChunk.chunkHeight; y++)
                {
                    if (grid[x, y, z].walkable) 
                    {
                        Vector3 localPos = new Vector3(x, y, z);
                        Vector3 globalPos = localPos + chunkPos3D;
                        globalPos += new Vector3(0.5f, 1.5f, 0.5f);

                        Gizmos.color = Color.black;
                        Gizmos.DrawCube(globalPos, Vector3.one);
                    }
                }
    }
}

