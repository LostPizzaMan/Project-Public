using System;
using System.Collections.Generic;
using UnityEngine;

public class ChunkDataUtilities : MonoBehaviour
{
    public static bool IsVoxelInChunk(int x, int y, int z)
    {
        if (x < 0 || x > TerrainChunk.chunkWidth - 1 || z < 0 || z > TerrainChunk.chunkWidth - 1 || y < 0 || y > 63)
            return false;
        else
            return true;
    }

    public static Color GetLightLevel(Vector3 globalPos)
    {
        TerrainChunk tc = GetChunk(globalPos);
        Vector3Int localPos = GetLocalBlockPosition(tc, globalPos);

        int lightValue = VoxelLightHelper.GetCombinedLight(tc, localPos);

        return VoxelLightHelper.GetUnpackedLightColor(lightValue);
    }

    public static Vector3Int GetGridSnappedPosition(Vector3 globalPos)
    {
        //index of the target block
        int bix = Mathf.FloorToInt(globalPos.x);
        int biy = Mathf.FloorToInt(globalPos.y);
        int biz = Mathf.FloorToInt(globalPos.z);

        return new Vector3Int(bix, biy, biz);
    }

    public static TerrainChunk GetChunk(Vector3 globalPos)
    {
        Vector2Int cp = GetChunkPosition(globalPos);
        TerrainChunk tc = TerrainGenerator.chunks[cp];

        return tc;
    }

    public static Vector2Int GetChunkPosition(Vector3 globalPos)
    {
        int chunkPosX = Mathf.FloorToInt(globalPos.x / 16) * 16;
        int chunkPosZ = Mathf.FloorToInt(globalPos.z / 16) * 16;

        Vector2Int cp = new Vector2Int(chunkPosX, chunkPosZ);

        return cp;
    }

    public static Vector3Int GetLocalBlockPosition(TerrainChunk tc, Vector3 globalPos)
    {
        //index of the target block
        int bix = Mathf.FloorToInt(globalPos.x) - tc.chunkPos2D.x;
        int biy = Mathf.FloorToInt(globalPos.y);
        int biz = Mathf.FloorToInt(globalPos.z) - tc.chunkPos2D.y;

        Vector3Int localPos = new Vector3Int(bix, biy, biz);

        return localPos;
    }

    // Faster than GetChunkPosition but can only access neighbouring chunks
    public static Vector2Int GetNeighborChunkPosition(int chunkX, int chunkZ, int localX, int localZ)
    {
        if (localX == 16)
        {
            chunkX = chunkX + 16;          
        }
        else if (localX == -1)
        {
            chunkX = chunkX - 16;
        }

        if (localZ == 16)
        {
            chunkZ = chunkZ + 16;
        }
        else if (localZ == -1)
        {
            chunkZ = chunkZ - 16;
        }

        return new Vector2Int(chunkX, chunkZ);
    }

    // -------- CHUNK CACHE SYSTEM -------- \\

    // Faster than GetNeighborChunkPosition but can only access neighbouring chunks as well, uses a cache system
    public static TerrainChunk GetChunkFromCache(TerrainChunk tc, int x, int z)
    {       
        if (x == 16 && z == 16)
        {
            return tc.terrainChunks[7];
        }
        else if (x == -1 && z == -1)
        {
            return tc.terrainChunks[4];
        }
        else if (x == 16 && z == -1)
        {
            return tc.terrainChunks[6];
        }
        else if (x == -1 && z == 16)
        {
            return tc.terrainChunks[5];
        }
        else if (x == -1)
        {
            return tc.terrainChunks[2];
        }
        else if (x == 16)
        {
            return tc.terrainChunks[3];
        }
        else if (z == -1)
        {
            return tc.terrainChunks[1];
        }
        else if (z == 16)
        {
            return tc.terrainChunks[0];
        }

        Debug.Log("INVALID BLOCK COORD, MUST BE COORD CORRESPONDING TO EDGE BLOCK");

        return null;
    }

    public static void SetChunkToCache(TerrainChunk tc, int x, int z, TerrainChunk toCache)
    {
        if (x == 16 && z == 16)
        {
            tc.terrainChunks[7] = toCache;
        }
        else if (x == -1 && z == -1)
        {
            tc.terrainChunks[4] = toCache;
        }
        else if (x == 16 && z == -1)
        {
            tc.terrainChunks[6] = toCache;
        }
        else if (x == -1 && z == 16)
        {
            tc.terrainChunks[5] = toCache;
        }
        else if (x == -1)
        {
            tc.terrainChunks[2] = toCache;
        }
        else if (x == 16)
        {
            tc.terrainChunks[3] = toCache;
        }
        else if (z == -1)
        {
            tc.terrainChunks[1] = toCache;
        }
        else if (z == 16)
        {
            tc.terrainChunks[0] = toCache;
        }
    }

    public static int GetChunkCacheIndex(int x, int z)
    {
        if (x == -1)
        {
            return 2;
        }
        else if (x == 16)
        {
            return 3;
        }
        else if (z == -1)
        {
            return 1;
        }
        else if (z == 16)
        {
            return 0;
        }

        return 10;
    }

    // -------- OTHER -------- \\

    public static void SetBlockType(int x, int y, int z, TerrainChunk tc, int blockType, List<TerrainChunk> toUpdate)
    {
        Vector3Int blockPos = new Vector3Int(x, y, z);

        if (x < 0 || x > 15 || z < 0 || z > 15) // If outside current chunk
        {
            SetBlock(blockType, blockPos, tc, toUpdate);
        }
        else if (y >= 1 && y < 63) // If inside current chunk
        {
            tc.blocks[blockPos.x, blockPos.y, blockPos.z] = blockType;
            ChunkUpdater.RemoveBlockLightNoUpdate(tc, blockPos);
        }
    }

    static void SetBlock(int blockType, Vector3Int blockPos, TerrainChunk tc, List<TerrainChunk> toUpdate)
    {
        blockPos += tc.chunkPos3D; // This gives us the world position of the block
        Vector2Int newChunkPos = GetChunkPosition(blockPos); // Returns the chunk instance at the provided world position 
       
        if (TerrainGenerator.chunks.TryGetValue(newChunkPos, out tc))
        {
            blockPos -= tc.chunkPos3D; // Subtract by the chunk position to get the local position

            if (blockPos.y >= 1 & blockPos.y < 63)
            {
                tc.blocks[blockPos.x, blockPos.y, blockPos.z] = blockType;
                ChunkUpdater.RemoveBlockLightNoUpdate(tc, blockPos);

                if (!toUpdate.Contains(tc))
                {
                    toUpdate.Add(tc);
                }
            }
        }
    }
}
