using UnityEngine;

public class ChunkUpdater : MonoBehaviour
{
    // ------------------------------------------- //
    // ---- SECTION FOR UPDATING VOXEL BLOCKS ---- //
    // ------------------------------------------- //

    public static void PlaceBlockUpdate(Vector3Int localPos, int blockID, TerrainChunk tc)
    {
        tc.blocks[localPos.x, localPos.y, localPos.z] = blockID;

        PlaceBlockLightUpdate(tc, localPos);
        UpdateNeighbourChunks(tc, localPos.x, localPos.z);

        tc.quickUpdateFlag = true;
    }

    public static void RemoveBlockUpdate(ChunkPhysics chunkPhysics, Vector3Int localPos, TerrainChunk tc)
    {
        tc.blocks[localPos.x, localPos.y, localPos.z] = 0;

        chunkPhysics.CheckForUnattachedBlocks(localPos.x, localPos.y, localPos.z, tc);

        RemoveBlockLightUpdate(tc, localPos);
        UpdateNeighbourChunks(tc, localPos.x, localPos.z);

        tc.quickUpdateFlag = true;
    }

    public static void RemoveBlockUpdateNoPhysics(Vector3Int localPos, TerrainChunk tc)
    {
        tc.blocks[localPos.x, localPos.y, localPos.z] = 0;

        RemoveBlockLightUpdate(tc, localPos);
        UpdateNeighbourChunks(tc, localPos.x, localPos.z);

        tc.quickUpdateFlag = true;
    }

    // ------------------------------------------- //
    // ---- SECTION FOR UPDATING VOXEL LIGHTS ---- //
    // ------------------------------------------- //

    public static void PlaceBlockLightUpdate(TerrainChunk tc, Vector3Int localPos)
    {
        if (VoxelLightHelper.GetSunlight(tc, localPos) == 15)
        {
            tc.voxelLightEngine.sunRemovalList.Add(new VoxelLightEngine.LightNode(tc, localPos, 0));
            tc.voxelLightEngine.CalculateSunlightRemoval();
            tc.voxelLightEngine.CalculateLight();
        }
        else
        {
            tc.voxelLightEngine.sunlightRemovalQueue.Enqueue(new VoxelLightEngine.LightNode(tc, localPos, VoxelLightHelper.GetSunlight(tc, localPos)));
            VoxelLightHelper.SetSunlight(tc, localPos, 0);
            tc.voxelLightEngine.CalculateLight();
        }

        tc.voxelLightEngine.lampLightRemovalQueue.Enqueue(new VoxelLightEngine.LightNode(tc, localPos, VoxelLightHelper.GetCombinedLight(tc, localPos)));
        VoxelLightHelper.SetLampLightToZero(tc, localPos);
        tc.voxelLightEngine.CalculateLight();
    }

    public static void RemoveBlockLightUpdate(TerrainChunk tc, Vector3Int localPos)
    {
        if (VoxelLightHelper.GetSunlight(tc, new Vector3Int(localPos.x, localPos.y + 1, localPos.z)) == 15)
        {
            tc.voxelLightEngine.sunExtendList.Add(new VoxelLightEngine.LightNode(tc, localPos, 0));
            tc.voxelLightEngine.CalculateSunlightExtend();
            tc.voxelLightEngine.CalculateLight();
        }
        else
        {
            tc.voxelLightEngine.sunlightRemovalQueue.Enqueue(new VoxelLightEngine.LightNode(tc, localPos, VoxelLightHelper.GetSunlight(tc, localPos)));
            VoxelLightHelper.SetSunlight(tc, localPos, 0);
            tc.voxelLightEngine.CalculateLight();
        }

        tc.voxelLightEngine.lampLightRemovalQueue.Enqueue(new VoxelLightEngine.LightNode(tc, localPos, VoxelLightHelper.GetCombinedLight(tc, localPos)));
        VoxelLightHelper.SetLampLightToZero(tc, localPos);
        tc.voxelLightEngine.CalculateLight();
    }

    public static void RemoveBlockLightNoUpdate(TerrainChunk tc, Vector3Int localPos)
    {
        if (VoxelLightHelper.GetSunlight(tc, new Vector3Int(localPos.x, localPos.y + 1, localPos.z)) == 15)
        {
            tc.voxelLightEngine.sunExtendList.Add(new VoxelLightEngine.LightNode(tc, localPos, 0));
            tc.voxelLightEngine.CalculateSunlightExtend();
        }
        else
        {
            tc.voxelLightEngine.sunlightRemovalQueue.Enqueue(new VoxelLightEngine.LightNode(tc, localPos, VoxelLightHelper.GetSunlight(tc, localPos)));
            VoxelLightHelper.SetSunlight(tc, localPos, 0);
        }

        tc.voxelLightEngine.lampLightRemovalQueue.Enqueue(new VoxelLightEngine.LightNode(tc, localPos, VoxelLightHelper.GetCombinedLight(tc, localPos)));
        VoxelLightHelper.SetLampLightToZero(tc, localPos);
    }

    // ------------------------------------------- //
    // ------------- OTHER FUNCTIONS ------------- //
    // ------------------------------------------- //

    static void UpdateNeighbourChunks(TerrainChunk chunk, int bix, int biz)
    {
        if (bix == TerrainChunk.chunkWidth - 1)
        {
            UpdateNeighbour(chunk, 3);
        }

        if (bix == 0)
        {
            UpdateNeighbour(chunk, 2);
        }

        if (biz == TerrainChunk.chunkWidth - 1)
        {
            UpdateNeighbour(chunk, 0);
        }

        if (biz == 0)
        {
            UpdateNeighbour(chunk, 1);
        }
    }

    static void UpdateNeighbour(TerrainChunk chunk, int dir)
    {
        TerrainChunk neighbourChunk = CubeMeshData.GetChunkNeighbour(chunk.chunkPos3D, dir);

        neighbourChunk.quickUpdateFlag = true;
    }
}
