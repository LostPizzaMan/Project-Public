using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlatLighting 
{
    public static Color GetLightValue(TerrainChunk tc, Vector3Int blockNeighbor, int x, int z)
    {
        Color lightLevel = Color.white;
        lightLevel.a = 0;

        if (ChunkDataUtilities.IsVoxelInChunk(blockNeighbor.x, blockNeighbor.y, blockNeighbor.z))
        {
            lightLevel.a = VoxelLightHelper.GetSunlight(tc, blockNeighbor) / 15f;
            lightLevel.r = VoxelLightHelper.GetRedLight(tc, blockNeighbor) / 15f;
            lightLevel.g = VoxelLightHelper.GetGreenLight(tc, blockNeighbor) / 15f;
            lightLevel.b = VoxelLightHelper.GetBlueLight(tc, blockNeighbor) / 15f;
        }
        else
        {
            // Returns the chunk instance at the provided world position 
            TerrainChunk neighbourChunk = ChunkDataUtilities.GetChunkFromCache(tc, blockNeighbor.x, blockNeighbor.z);

            // This gives us the world position of the block
            int globalX = x + tc.chunkPos3D.x;
            int globalZ = z + tc.chunkPos3D.z;

            // Subtract by the chunk position to get the local position
            int localX = globalX - neighbourChunk.chunkPos3D.x;
            int localZ = globalZ - neighbourChunk.chunkPos3D.z;

            blockNeighbor = new Vector3Int(localX, blockNeighbor.y, localZ); // This gives us the world position of the block

            lightLevel.a = VoxelLightHelper.GetSunlight(neighbourChunk, blockNeighbor) / 15f;
            lightLevel.r = VoxelLightHelper.GetRedLight(neighbourChunk, blockNeighbor) / 15f;
            lightLevel.g = VoxelLightHelper.GetGreenLight(neighbourChunk, blockNeighbor) / 15f;
            lightLevel.b = VoxelLightHelper.GetBlueLight(neighbourChunk, blockNeighbor) / 15f;
        }

        return lightLevel;
    }
}
