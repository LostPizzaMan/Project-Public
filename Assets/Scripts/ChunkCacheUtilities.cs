using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkCacheUtilities
{
    public static void AddSelfToNeighbourChunkCache(TerrainChunk tc)
    {
        for (int i = 0; i < 8; i++)
        {
            TerrainChunk neighbourChunk = CubeMeshData.GetChunkNeighbour(tc.chunkPos3D, i);

            if (neighbourChunk is null) { continue; }

            tc.terrainChunks[i] = neighbourChunk;
            neighbourChunk.terrainChunks[CubeMeshData.invertChunkNeighbourOffsets[i]] = tc;
        }
    }

    public static void RemoveSelfFromNeighbourChunkCache(TerrainChunk tc)
    {
        for (int i = 0; i < 8; i++)
        {
            TerrainChunk neighbourChunk = tc.terrainChunks[i];

            if (neighbourChunk is null) { continue; }

            neighbourChunk.terrainChunks[CubeMeshData.invertChunkNeighbourOffsets[i]] = null;
        }
    }

    public static bool IsChunkInUse(TerrainChunk tc)
    {
        if (tc.isUpdating)
        {
            return true;
        }

        for (int i = 0; i < 8; i++)
        {
            TerrainChunk neighbourChunk = tc.terrainChunks[i];

            if (neighbourChunk is null) { continue; }

            if (neighbourChunk.isUpdating)
            {
                return true;
            }
        }

        return false;
    }

    public static void FillChunkCache(TerrainChunk tc)
    {
        for (int i = 0; i < 8; i++)
        {
            TerrainChunk neighbourChunk = CubeMeshData.GetChunkNeighbour(tc.chunkPos3D, i);

            tc.terrainChunks[i] = neighbourChunk;
        }
    }

    public static bool VerifyChunkCache(TerrainChunk tc)
    {
        int count = 0;
        for (int i = 0; i < 8; i++)
        {
            count++;
            if (tc.terrainChunks[i] == null)
            {
                Debug.Log("ERROR : CHUNK CACHE IS INCOMPLETE");
                break;
            }
        }
        if (count == 8)
        {
            Debug.Log("SUCCESS : CHUNK CACHE IS COMPLETE");
            return true;
        }
        else
        {
            return false;
        }
    }
}
