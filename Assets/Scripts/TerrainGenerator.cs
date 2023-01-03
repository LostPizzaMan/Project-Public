using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    // -- Core -- // 
    [Range(0f, 1f)]
    public float globalLightLevel;

    public GameObject terrainChunk;
    public Transform player;

    public static Dictionary<Vector2Int, TerrainChunk> chunks = new Dictionary<Vector2Int, TerrainChunk>();

    // -- Chunk Generation -- // 
    FastNoise noise;

    int chunkDist = 8;

    List<Vector2Int> toGenerate = new List<Vector2Int>();
    List<TerrainChunk> toSetup = new List<TerrainChunk>();

    List<TerrainChunk> pooledChunks = new List<TerrainChunk>();

    // -- References -- // 
    [Header("[References]")]
    [SerializeField] private TextureDatabase textureDatabase = null;
    [SerializeField] public ItemDatabase itemDatabase = null;
    [SerializeField] public SaveManager saveManager = null;

    [SerializeField] private Transform chunkObjects;
    [HideInInspector] public bool loadingChunksAllowed;

    public bool threadLocked;

    // -- World Data -- // 
    [Header("[World Data]")]

    public string worldName = "New World";
    public int[] blockArray;

    [Header("[Debug]")]
    public int toGenerateCount;
    public int toSetupCount;
    public int pooledChunksCount;
    public int totalChunksCount;

    float timer;

    public void LoadSave()
    {
        InitializeBlockIds();

        noise = new FastNoise(worldName.GetHashCode());
        var saveFolder = Directory.CreateDirectory(Application.persistentDataPath + " Saves/" + worldName + "/region/").FullName; // returns a DirectoryInfo object
        string saveFolderPath = Path.Combine(saveFolder);

        saveManager.SetupSaveFolder(saveFolderPath);
    }

    private void Update()
    {
        toGenerateCount = toGenerate.Count;
        toSetupCount = toSetup.Count;
        pooledChunksCount = pooledChunks.Count;
        totalChunksCount = chunks.Count;

        Shader.SetGlobalFloat("_sunlightIntensity", globalLightLevel);

        if (loadingChunksAllowed)
        {
            LoadChunks();

            DelayBuildChunks();

            if (timer > 0.2f)
            {
                SetupChunks();

                timer = 0;
            }

            timer += Time.deltaTime;
        }
    }

    void InitializeBlockIds()
    {
        blockArray = new int[10];

        blockArray[0] = itemDatabase.ReturnBlockIDByName("Dirt");
        blockArray[1] = itemDatabase.ReturnBlockIDByName("Grass");
        blockArray[2] = itemDatabase.ReturnBlockIDByName("Stone");
        blockArray[3] = itemDatabase.ReturnBlockIDByName("Trunk");
        blockArray[4] = itemDatabase.ReturnBlockIDByName("Leaves");
        blockArray[5] = itemDatabase.ReturnBlockIDByName("Flower");
        blockArray[6] = itemDatabase.ReturnBlockIDByName("Cross_Grass");
        blockArray[7] = itemDatabase.ReturnBlockIDByName("Bedrock");
        blockArray[8] = itemDatabase.ReturnBlockIDByName("IronOre");
        blockArray[9] = itemDatabase.ReturnBlockIDByName("Sand");
    }

    // -- CHUNK CREATION -- \\

    async void BuildChunk(int xPos, int zPos)
    {
        // Wait until all chunks are saved before creating or adding new ones
        if (saveManager.threadLocked && saveManager.toSave.Count > 0) { return; }

        threadLocked = true;

        TerrainChunk chunk;
        if (pooledChunks.Count > 0) // Look in the pool first
        {
            chunk = pooledChunks[0];
            chunk.gameObject.SetActive(true);
            pooledChunks.RemoveAt(0);
            Array.Clear(chunk.lightMap, 0, chunk.lightMap.Length);
            chunk.generatingFinished = false;
            chunk.lightingFinished = false;
            chunk.voxelLightEngine.doSunlightChunkUpdate = false;
            chunk.ranSearchPhase = false;
            chunk.ranLightPhase = false;
            chunk.ranMeshPhase = false;

            chunk.transform.position = new Vector3(xPos, 0, zPos);
            chunk.transform.name = new Vector2Int(xPos, zPos).ToString();
        }
        else
        {
            GameObject chunkGO = Instantiate(terrainChunk, new Vector3(xPos, 0, zPos), Quaternion.identity);
            chunkGO.transform.SetParent(chunkObjects.transform);
            chunkGO.transform.name = new Vector2Int(xPos, zPos).ToString();
            chunk = chunkGO.GetComponent<TerrainChunk>();
            chunk.textureDatabase = textureDatabase;
            chunk.voxelLightEngine.itemDatabase = itemDatabase;
            chunk.terrainGenerator = this;

            chunk.chunkID = Guid.NewGuid().ToString();
        }

        chunk.SetupChunkCache();

        chunk.rend.enabled = false;
        chunk.transparentRend.enabled = false;

        toGenerate.Remove(new Vector2Int(xPos, zPos));
        chunks.Add(new Vector2Int(xPos, zPos), chunk);

        bool loadedFromFile = saveManager.TryLoadChunk(chunk, xPos, zPos);

        if (!loadedFromFile)
        {
            await Task.Run(() =>
            {
                System.Random RNG = new System.Random(xPos + zPos);

                for (int x = 0; x < TerrainChunk.chunkWidth; x++)
                {
                    for (int z = 0; z < TerrainChunk.chunkWidth; z++)
                    {
                        var noiseLayer = GetNoiseLayer(xPos + x, zPos + z);

                        for (int y = 0; y < TerrainChunk.chunkHeight; y++)
                        {
                            chunk.blocks[x, y, z] = GetBlockType(xPos + x, y, zPos + z, RNG, noiseLayer);
                        }
                    }
                }

                GenerateTrees(chunk.blocks, xPos, zPos);
            });
        }

        if (!loadedFromFile)
        {
            saveManager.toSave.Enqueue(chunk);
        }

        chunk.Initialize(xPos, zPos);
        toSetup.Add(chunk);

        WaterChunk wat = chunk.transform.GetComponentInChildren<WaterChunk>();
        wat.SetLocs(chunk.blocks);
        wat.BuildMesh();

        threadLocked = false;
    }

    // -- CHUNK MANAGEMENT -- \\

    Vector2Int curChunk = new Vector2Int(-1, -1);
    public void LoadChunks(bool instant = false)
    {
        saveManager.RunSaveCycle();

        //the current chunk the player is in
        int curChunkPosX = Mathf.FloorToInt(player.position.x / 16) * 16;
        int curChunkPosZ = Mathf.FloorToInt(player.position.z / 16) * 16;

        //entered a new chunk
        if (curChunk.x != curChunkPosX || curChunk.y != curChunkPosZ)
        {
            curChunk.x = curChunkPosX;
            curChunk.y = curChunkPosZ;

            for (int i = curChunkPosX - 16 * chunkDist; i <= curChunkPosX + 16 * chunkDist; i += 16)
                for (int j = curChunkPosZ - 16 * chunkDist; j <= curChunkPosZ + 16 * chunkDist; j += 16)
                {
                    Vector2Int cp = new Vector2Int(i, j);

                    if (!chunks.ContainsKey(cp) && !toGenerate.Contains(cp))
                    {
                        if (instant)
                            BuildChunk(i, j);
                        else
                            toGenerate.Add(cp);
                    }
                }

            // -- Remove chunks that are too far away -- \\
            List<TerrainChunk> toDestroy = new List<TerrainChunk>();
            foreach (KeyValuePair<Vector2Int, TerrainChunk> c in chunks)
            {
                Vector2Int cp = c.Key;
                if (Mathf.Abs(curChunkPosX - cp.x) > 16 * (chunkDist + 3) ||
                    Mathf.Abs(curChunkPosZ - cp.y) > 16 * (chunkDist + 3))
                {
                    TerrainChunk chunk = chunks[cp];

                    if (!ChunkCacheUtilities.IsChunkInUse(chunk) && chunk.generatingFinished)
                    {
                        toDestroy.Add(chunk);
                    }
                }
            }

            foreach (TerrainChunk chunk in toDestroy)
            {
                ChunkCacheUtilities.RemoveSelfFromNeighbourChunkCache(chunk);
                chunk.gameObject.SetActive(false);
                pooledChunks.Add(chunk);
                chunks.Remove(chunk.chunkPos2D);
                toSetup.Remove(chunk);
            }

            // -- Remove any up for generation -- \\
            foreach (Vector2Int cp in toGenerate.ToArray())
            {
                if (Mathf.Abs(curChunkPosX - cp.x) > 16 * (chunkDist + 1) ||
                    Mathf.Abs(curChunkPosZ - cp.y) > 16 * (chunkDist + 1))
                    toGenerate.Remove(cp);
            }
        }
    }

    void DelayBuildChunks()
    {
        if (toGenerate.Count > 0)
        {
            BuildChunk(toGenerate[0].x, toGenerate[0].y);
        }
    }

    void SetupChunks()
    {
        foreach (TerrainChunk chunk in toSetup.ToArray())
        {
            if (chunk.ranLightPhase && chunk.ranMeshPhase)
            {
                toSetup.Remove(chunk);
                continue;
            }

            if (!chunk.ranSearchPhase)
            {
                chunk.ranSearchPhase = true;

                ChunkCacheUtilities.AddSelfToNeighbourChunkCache(chunk);
            }

            if (!chunk.isUpdating)
            {
                if (!chunk.ranLightPhase)
                {
                    chunk.AreNeighboursChunksLoaded();
                }

                if (chunk.ranLightPhase && !chunk.ranMeshPhase)
                {
                    chunk.AreNeighboursLightLoaded();
                }
            }
        }
    }

    // -- NOISE GENERATION -- \\

    int GetBlockType(int x, int y, int z, System.Random RNG, NoiseLayer noiseLayer)
    {
        if (y == 0)
            return blockArray[7]; // Bedrock

        // -- NOISE -- \\

        float baseLandHeight = noiseLayer.LandHeight;
        float baseStoneHeight = noiseLayer.StoneHeight;

        //3d noise for caves and overhangs and such
        float caveNoise = noise.GetPerlinFractal(x * 5f, y * 10f, z * 5f);
        float caveMask = noiseLayer.CaveMask;

        int oreSize = 10;
        float oreNoise = noise.GetSimplexFractal(x * 2.5f * oreSize, y * 5.0f * oreSize, z * 2.5f * oreSize);

        int blockType = 0;

        bool isLake = baseLandHeight <= WaterChunk.waterHeight - 1;

        // -- EVALUATION -- \\

        if (y >= baseLandHeight - 2 && y <= baseLandHeight && isLake)
        {
            blockType = blockArray[9];
        }
        else if (y <= baseLandHeight)
        {
            //under the surface, dirt block
            blockType = blockArray[0]; // Dirt

            //just on the surface, use a grass type
            if (y > baseLandHeight - 1 && y > WaterChunk.waterHeight - 2)
                blockType = blockArray[1]; // Grass

            if (y <= baseStoneHeight)
            {
                blockType = blockArray[2]; // Stone

                if (oreNoise > 0.6f) // Higher number the less the frequency
                {
                    blockType = blockArray[8]; // Iron Ore
                }
            }
        }

        if (y > baseLandHeight && y < baseLandHeight + 1 && y > WaterChunk.waterHeight - 1 && !isLake)
        {
            int range = RNG.Next(1, 100);

            if (range < 6)
            {
                blockType = blockArray[6]; // Cross Grass
            }
            if (range < 2)
            {
                blockType = blockArray[5]; // Flower 
            }
        }

        if (caveNoise > Mathf.Max(caveMask, .2f))
            blockType = 0;

        return blockType;
    }

    void GenerateTrees(int[,,] blocks, int x, int z)
    {
        System.Random rand = new System.Random(x * 10000 + z);

        float simplex = noise.GetSimplex(x * .8f, z * .8f);

        if (simplex > 0)
        {
            simplex *= 2f;
            int treeCount = Mathf.FloorToInt((float)rand.NextDouble() * 5 * simplex);

            for (int i = 0; i < treeCount; i++)
            {
                int xPos = (int)(rand.NextDouble() * 14) + 1;
                int zPos = (int)(rand.NextDouble() * 14) + 1;

                int y = TerrainChunk.chunkHeight - 1;
                //find the ground
                while (y > 0 && blocks[xPos, y, zPos] == 0)
                {
                    y--;
                }
                y++;

                int treeHeight = 4 + (int)(rand.NextDouble() * 4);

                for (int j = 0; j < treeHeight; j++)
                {
                    if (y + j < 64)
                        blocks[xPos, y + j, zPos] = blockArray[3];
                }

                int leavesWidth = 1 + (int)(rand.NextDouble() * 6);
                int leavesHeight = (int)(rand.NextDouble() * 3);

                int iter = 0;
                for (int m = y + treeHeight - 1; m <= y + treeHeight - 1 + treeHeight; m++)
                {
                    for (int k = xPos - (int)(leavesWidth * .5) + iter / 2; k <= xPos + (int)(leavesWidth * .5) - iter / 2; k++)
                        for (int l = zPos - (int)(leavesWidth * .5) + iter / 2; l <= zPos + (int)(leavesWidth * .5) - iter / 2; l++)
                        {
                            if (k >= 0 && k < 16 && l >= 0 && l < 16 && m >= 0 && m < 64 && rand.NextDouble() < .8f)
                                blocks[k, m, l] = blockArray[4];
                        }

                    iter++;
                }
            }
        }
    }

    NoiseLayer GetNoiseLayer(int x, int z)
    {
        // land layer 
        float simplex1 = noise.GetSimplex(x * .8f, z * .8f) * 10;
        float simplex2 = noise.GetSimplex(x * 3f, z * 3f) * 10 * (noise.GetSimplex(x * .3f, z * .3f) + .5f);

        float heightMap = simplex1 + simplex2;
        float baseLandHeight = TerrainChunk.chunkHeight * .5f + heightMap;

        // stone layer 
        float simplexStone1 = noise.GetSimplex(x * 1f, z * 1f) * 10;
        float simplexStone2 = (noise.GetSimplex(x * 5f, z * 5f) + .5f) * 20 * (noise.GetSimplex(x * .3f, z * .3f) + .5f);

        float stoneHeightMap = simplexStone1 + simplexStone2;
        float baseStoneHeight = TerrainChunk.chunkHeight * .25f + stoneHeightMap;

        // cave mask 
        float caveMask = noise.GetSimplex(x * .3f, z * .3f) + .3f;

        return new NoiseLayer()
        {
            LandHeight = baseLandHeight,
            StoneHeight = baseStoneHeight,
            CaveMask = caveMask,
        };
    }
}
