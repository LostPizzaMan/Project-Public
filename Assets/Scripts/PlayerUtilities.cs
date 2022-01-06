using UnityEngine;

public class PlayerUtilities
{
    public static RaycastHit ShootRaycast(Camera cam, int maxDistance, LayerMask groundLayer)
    {
        RaycastHit hitInfo;

        Physics.Raycast(cam.transform.position, cam.transform.forward, out hitInfo, maxDistance, groundLayer);

        return hitInfo;
    }

    public static void SetBlockAt(PlayerInstance player, Vector3 pointInTargetBlock, int blockID)
    {
        TerrainChunk tc = ChunkDataUtilities.GetChunk(pointInTargetBlock);
        Vector3Int localPos = ChunkDataUtilities.GetLocalBlockPosition(tc, pointInTargetBlock);

        if (blockID > 0)
        {
            ChunkUpdater.PlaceBlockUpdate(localPos, blockID, tc);
        }
        else
        {
            ChunkUpdater.RemoveBlockUpdate(player.chunkPhysics, localPos, tc);
        }
    }

    public static void PlaceLightSourceAt(Vector3 pointInTargetBlock, int blockID)
    {
        TerrainChunk tc = ChunkDataUtilities.GetChunk(pointInTargetBlock);
        Vector3Int localPos = ChunkDataUtilities.GetLocalBlockPosition(tc, pointInTargetBlock);

        tc.blocks[localPos.x, localPos.y, localPos.z] = blockID;

        int intensity = VoxelLightHelper.GetCombinedLight(tc, localPos);
        Vector3Int color = TerrainModifier.CalculateRandomColor();

        intensity = intensity & 0xF0FF | (color.x << 8); //Red
        intensity = intensity & 0xFF0F | (color.y << 4); //Green
        intensity = intensity & 0xFFF0 | color.z; //Blue

        tc.voxelLightEngine.lampLightUpdateQueue.Enqueue(new VoxelLightEngine.LightNode(tc, localPos, intensity));
        tc.voxelLightEngine.CalculateLight();

        tc.quickUpdateFlag = true;
    }
}

