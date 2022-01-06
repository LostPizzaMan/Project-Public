using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelLightEngine : MonoBehaviour
{
    public Queue<LightNode> sunlightUpdateQueue = new Queue<LightNode>();
    public Queue<LightNode> sunlightRemovalQueue = new Queue<LightNode>();

    public Queue<LightNode> lampLightUpdateQueue = new Queue<LightNode>();
    public Queue<LightNode> lampLightRemovalQueue = new Queue<LightNode>();

    public List<LightNode> sunExtendList = new List<LightNode>();
    public List<LightNode> sunRemovalList = new List<LightNode>();

    LightNode dequeueNode;
    [HideInInspector]
    public ItemDatabase itemDatabase;

    public bool doSunlightChunkUpdate;

    int lampBlockID;

    void Start()
    {
        lampBlockID = itemDatabase.ReturnBlockIDByName("Lamp");
    }

    TerrainChunk GetChunk(TerrainChunk tc, Vector3Int blockIndex, Vector2Int chunkPos, bool updateChunk)
    {
        int cacheIndex = ChunkDataUtilities.GetChunkCacheIndex(blockIndex.x, blockIndex.z);
        TerrainChunk chunk = tc.terrainChunks[cacheIndex];

        if (chunk is null)
        {
            chunk = TerrainGenerator.chunks[ChunkDataUtilities.GetNeighborChunkPosition(chunkPos.x, chunkPos.y, blockIndex.x, blockIndex.z)];
            ChunkDataUtilities.SetChunkToCache(tc, blockIndex.x, blockIndex.z, chunk);
        }

        if (updateChunk)
        {
            chunk.updateFlag = true;
        }

        return chunk;
    }

    public struct LightNode
    {
        public Vector3Int blockIndex;
        public int lightVal;
        public TerrainChunk chunk;

        public LightNode(TerrainChunk _chunk, Vector3Int _blockIndex, int _lightVal)
        {
            blockIndex = _blockIndex;
            lightVal = _lightVal;
            chunk = _chunk;
        }
    }

    public void CalculateLight()
    {
        //Sunlight Calculation

        //Removal
        while (sunlightRemovalQueue.Count > 0)
        {
            dequeueNode = sunlightRemovalQueue.Dequeue();
            RemoveSunlightBFS(dequeueNode.chunk, dequeueNode.blockIndex, dequeueNode.lightVal);
        }

        //Addition
        while (sunlightUpdateQueue.Count > 0)
        {
            dequeueNode = sunlightUpdateQueue.Dequeue();
            PlaceSunlightBFS(dequeueNode.chunk, dequeueNode.blockIndex, dequeueNode.lightVal);
        }

        //Voxel Light Calculation

        //Removal
        while (lampLightRemovalQueue.Count > 0)
        {
            dequeueNode = lampLightRemovalQueue.Dequeue();
            RemoveLampLightBFS(dequeueNode.chunk, dequeueNode.blockIndex, dequeueNode.lightVal);
        }

        //Addition
        while (lampLightUpdateQueue.Count > 0)
        {
            dequeueNode = lampLightUpdateQueue.Dequeue();
            PlaceLampLightBFS(dequeueNode.chunk, dequeueNode.blockIndex, dequeueNode.lightVal);
        }
    }

    public void CalculateSunlightExtend()
    {
        foreach (LightNode lightNode in sunExtendList)
        {
            ExtendSunRay(lightNode.chunk, lightNode.blockIndex);
        }

        sunExtendList.RemoveRange(0, sunExtendList.Count);
    }

    public void CalculateSunlightRemoval()
    {
        foreach (LightNode lightNode in sunRemovalList)
        {
            BlockSunRay(lightNode.chunk, lightNode.blockIndex);
        }

        sunRemovalList.RemoveRange(0, sunRemovalList.Count);
    }

    void BlockSunRay(TerrainChunk chunk, Vector3Int blockIndex)
    {
        int i = blockIndex.y;

        while (true) //do the light removal iteration
        {
            blockIndex.y = i;

            if (VoxelLightHelper.GetSunlight(chunk, blockIndex) == 15)
            {
                VoxelLightHelper.SetSunlight(chunk, blockIndex, 0);
                RemoveSunlightBFS(chunk, blockIndex, 15);
            }
            else
            {
                return;
            }
            i--;
        }
    }

    void ExtendSunRay(TerrainChunk chunk, Vector3Int blockIndex)
    {
        int i = blockIndex.y; //start at the current block, for extension to other chunks

        while (true)
        {
            blockIndex.y = i;

            if (itemDatabase.blocks[chunk.blocks[blockIndex.x, blockIndex.y, blockIndex.z]].transparent)
            {
                if (VoxelLightHelper.GetSunlight(chunk, blockIndex) != 15)
                {
                    sunlightUpdateQueue.Enqueue(new LightNode(chunk, blockIndex, 15));
                }
            }
            else
            {
                return;
            }
            i--;
        }
    }

    void RemoveSunlightNeighborUpdate(TerrainChunk chunk, Vector3Int blockIndex, int light)
    {
        int lightVal = VoxelLightHelper.GetSunlight(chunk, blockIndex);

        LightNode node = new LightNode(chunk, blockIndex, light);

        if (lightVal > 0)
        {
            if (lightVal <= light)
            {
                VoxelLightHelper.SetSunlight(chunk, blockIndex, 0);
                sunlightRemovalQueue.Enqueue(node);
            }
            else
            {
                node.lightVal = 0;
                sunlightUpdateQueue.Enqueue(node);
            }
        }
    }

    void RemoveSunlightBFS(TerrainChunk chunk, Vector3 blockIndex, int oldLightVal)
    {
        int nextIntensity;

        if (oldLightVal > 0)
        {
            nextIntensity = oldLightVal - 1;
        }
        else
        {
            nextIntensity = 0;
        }

        for (int i = 0; i < 6; i++)
        {
            Vector3 currentVoxel = CubeMeshData.GetNeighbor(i, blockIndex);
            Vector3Int neighbor = new Vector3Int((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z);

            if (ChunkDataUtilities.IsVoxelInChunk(neighbor.x, neighbor.y, neighbor.z) && itemDatabase.blocks[chunk.blocks[neighbor.x, neighbor.y, neighbor.z]].transparent)
            {
                RemoveSunlightNeighborUpdate(chunk, neighbor, nextIntensity);
            }
            else if (!ChunkDataUtilities.IsVoxelInChunk(neighbor.x, neighbor.y, neighbor.z) && neighbor.y > 0 && neighbor.y < 64)
            {
                Vector3Int globalNeighbor = neighbor + chunk.chunkPos3D;

                TerrainChunk localNChunk = GetChunk(chunk, neighbor, chunk.chunkPos2D, doSunlightChunkUpdate);
                Vector3Int localNPos = globalNeighbor - localNChunk.chunkPos3D;

                if (itemDatabase.blocks[localNChunk.blocks[localNPos.x, localNPos.y, localNPos.z]].transparent)
                {
                    RemoveSunlightNeighborUpdate(localNChunk, localNPos, nextIntensity);
                }
            }
        }
    }

    void PlaceSunlightNeighborUpdate(TerrainChunk chunk, Vector3Int blockIndex, int light)
    {
        if (VoxelLightHelper.GetSunlight(chunk, blockIndex) < light)
        {
            VoxelLightHelper.SetSunlight(chunk, blockIndex, light);
            sunlightUpdateQueue.Enqueue(new LightNode(chunk, blockIndex, light));
        }
    }

    void PlaceSunlightBFS(TerrainChunk chunk, Vector3Int blockIndex, int intensity)
    {
        if (intensity > VoxelLightHelper.GetSunlight(chunk, blockIndex))
        {
            //Set the light value
            VoxelLightHelper.SetSunlight(chunk, blockIndex, intensity);
        }
        else
        {
            //If intensity is less that the actual light value, use the actual
            intensity = VoxelLightHelper.GetSunlight(chunk, blockIndex);
        }

        if (intensity <= 1) return;
        //Reduce by 1 to prevent a bunch of - 1
        int newIntensity = (intensity - 1);

        for (int i = 0; i < 6; i++)
        {
            Vector3 currentVoxel = CubeMeshData.GetNeighbor(i, blockIndex);
            Vector3Int neighbor = new Vector3Int((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z);

            if (ChunkDataUtilities.IsVoxelInChunk(neighbor.x, neighbor.y, neighbor.z))
            {
                if (itemDatabase.blocks[chunk.blocks[neighbor.x, neighbor.y, neighbor.z]].transparent)
                {
                    PlaceSunlightNeighborUpdate(chunk, neighbor, newIntensity);
                }
            }
            else if (!ChunkDataUtilities.IsVoxelInChunk(neighbor.x, neighbor.y, neighbor.z) && neighbor.y > 0 && neighbor.y < 64)
            {
                Vector3Int globalNeighbor = neighbor + chunk.chunkPos3D;

                TerrainChunk localNChunk = GetChunk(chunk, neighbor, chunk.chunkPos2D, doSunlightChunkUpdate);
                Vector3Int localNPos = globalNeighbor - localNChunk.chunkPos3D;

                if (itemDatabase.blocks[localNChunk.blocks[localNPos.x, localNPos.y, localNPos.z]].transparent)
                {
                    PlaceSunlightNeighborUpdate(localNChunk, localNPos, newIntensity);
                }
            }
        }
    }

    int GetMaxLampColors(int redA, int greenA, int blueA, int b)
    {
        int sunLightB = b >> 12 & 0xF;
        int redB = b >> 8 & 0xF;
        int greenB = b >> 4 & 0xF;
        int blueB = b & 0xF;

        int newLight = 0;

        newLight = (newLight & 0xFFF) | sunLightB << 12;
        newLight = (newLight & 0xF0FF) | Mathf.Max(redA, redB) << 8;
        newLight = (newLight & 0xFF0F) | Mathf.Max(greenA, greenB) << 4;
        newLight = (newLight & 0xFFF0) | Mathf.Max(blueA, blueB);

        return newLight;
    }

    void RemoveLampNeighborUpdate(TerrainChunk chunk, Vector3Int blockIndex, int intensityRed, int intensityGreen, int intensityBlue, int light)
    {
        int nextRed, nextGreen, nextBlue;

        int nextLight = VoxelLightHelper.GetCombinedLight(chunk, blockIndex);

        nextRed = nextLight >> 8 & 0xF;
        nextGreen = nextLight >> 4 & 0xF;
        nextBlue = nextLight & 0xF;

        int lampLight = nextRed + nextGreen + nextBlue;

        if (nextRed > 0 || nextGreen > 0 || nextBlue > 0)
        {
            if (nextRed <= intensityRed || nextGreen <= intensityGreen || nextBlue <= intensityBlue)
            {
                if (chunk.blocks[blockIndex.x, blockIndex.y, blockIndex.z] != lampBlockID)
                {
                    VoxelLightHelper.SetLampLightToZero(chunk, blockIndex);
                    lampLightRemovalQueue.Enqueue(new LightNode(chunk, blockIndex, light));
                }

                if (nextRed > intensityRed || nextGreen > intensityGreen || nextBlue > intensityBlue)
                {
                    lampLightUpdateQueue.Enqueue(new LightNode(chunk, blockIndex, 0));
                }
            }
            else if (lampLight > 0)
            {
                lampLightUpdateQueue.Enqueue(new LightNode(chunk, blockIndex, 0));
            }
        }
    }

    void RemoveLampLightBFS(TerrainChunk chunk, Vector3Int blockIndex, int light)
    {
        int intensityRed = light >> 8 & 0xF;
        int intensityGreen = light >> 4 & 0xF;
        int intensityBlue = light & 0xF;

        //Reduce by 1
        if (intensityRed != 0)
        {
            intensityRed -= 1;
            light = (light & 0xF0FF) | intensityRed << 8;
        }
        if (intensityGreen != 0)
        {
            intensityGreen -= 1;
            light = (light & 0xFF0F) | intensityGreen << 4;
        }
        if (intensityBlue != 0)
        {
            intensityBlue -= 1;
            light = (light & 0xFFF0) | intensityBlue;
        }

        for (int i = 0; i < 6; i++)
        {
            Vector3 currentVoxel = CubeMeshData.GetNeighbor(i, blockIndex);
            Vector3Int neighbor = new Vector3Int((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z);

            if (ChunkDataUtilities.IsVoxelInChunk(neighbor.x, neighbor.y, neighbor.z))
            {
                RemoveLampNeighborUpdate(chunk, neighbor, intensityRed, intensityGreen, intensityBlue, light);
            }
            else if (!ChunkDataUtilities.IsVoxelInChunk(neighbor.x, neighbor.y, neighbor.z) && neighbor.y > 0 && neighbor.y < 64)
            {
                Vector3Int globalNeighbor = neighbor + chunk.chunkPos3D;

                TerrainChunk localNChunk = GetChunk(chunk, neighbor, chunk.chunkPos2D, true);
                Vector3Int localNPos = globalNeighbor - localNChunk.chunkPos3D;

                RemoveLampNeighborUpdate(localNChunk, localNPos, intensityRed, intensityGreen, intensityBlue, light);
            }
        }
    }

    void PlaceLampNeighborUpdate(TerrainChunk chunk, Vector3Int blockIndex, int intensityRed, int intensityGreen, int intensityBlue)
    {
        int currentLight = VoxelLightHelper.GetCombinedLight(chunk, blockIndex);

        int nextLight = GetMaxLampColors(intensityRed, intensityGreen, intensityBlue, currentLight);

        if (nextLight != currentLight)
        {
            VoxelLightHelper.SetCombinedLight(chunk, blockIndex, nextLight);
            lampLightUpdateQueue.Enqueue(new LightNode(chunk, blockIndex, nextLight));
        }
    }

    void PlaceLampLightBFS(TerrainChunk chunk, Vector3Int blockIndex, int intensity)
    {
        int currentLight = VoxelLightHelper.GetCombinedLight(chunk, blockIndex);

        int currentRed = currentLight >> 8 & 0xF;
        int currentGreen = currentLight >> 4 & 0xF;
        int currentBlue = currentLight & 0xF;

        int intensityRed = intensity >> 8 & 0xF;
        int intensityGreen = intensity >> 4 & 0xF;
        int intensityBlue = intensity & 0xF;

        intensityRed = Mathf.Max(currentRed, intensityRed);
        intensityGreen = Mathf.Max(currentGreen, intensityGreen);
        intensityBlue = Mathf.Max(currentBlue, intensityBlue);

        intensity = (intensity & 0xFFF) | VoxelLightHelper.GetSunlight(chunk, blockIndex) << 12;
        intensity = (intensity & 0xF0FF) | intensityRed << 8;
        intensity = (intensity & 0xFF0F) | intensityGreen << 4;
        intensity = (intensity & 0xFFF0) | intensityBlue;

        if (intensity != currentLight)
        {
            //Set the light value
            VoxelLightHelper.SetCombinedLight(chunk, blockIndex, intensity);
        }

        if (intensityRed <= 1 && intensityGreen <= 1 && intensityBlue <= 1) return;

        //Reduce by 1
        if (intensityRed != 0)
        {
            intensityRed -= 1;
        }
        if (intensityGreen != 0)
        {
            intensityGreen -= 1;
        }
        if (intensityBlue != 0)
        {
            intensityBlue -= 1;
        }

        for (int i = 0; i < 6; i++)
        {
            Vector3 currentVoxel = CubeMeshData.GetNeighbor(i, blockIndex);
            Vector3Int neighbor = new Vector3Int((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z);

            if (ChunkDataUtilities.IsVoxelInChunk(neighbor.x, neighbor.y, neighbor.z) && itemDatabase.blocks[chunk.blocks[neighbor.x, neighbor.y, neighbor.z]].transparent)
            {
                PlaceLampNeighborUpdate(chunk, neighbor, intensityRed, intensityGreen, intensityBlue);
            }
            else if (!ChunkDataUtilities.IsVoxelInChunk(neighbor.x, neighbor.y, neighbor.z) && neighbor.y > 0 && neighbor.y < 64)
            {
                Vector3Int globalNeighbor = neighbor + chunk.chunkPos3D;

                TerrainChunk localNChunk = GetChunk(chunk, neighbor, chunk.chunkPos2D, true);
                Vector3Int localNPos = globalNeighbor - localNChunk.chunkPos3D;

                if (itemDatabase.blocks[localNChunk.blocks[localNPos.x, localNPos.y, localNPos.z]].transparent)
                {
                    PlaceLampNeighborUpdate(localNChunk, localNPos, intensityRed, intensityGreen, intensityBlue);
                }
            }
        }
    }
}
