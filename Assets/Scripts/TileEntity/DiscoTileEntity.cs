using UnityEngine;

public class DiscoTileEntity : TileEntity
{
    public TerrainChunk tc;
    public Vector3Int localPos;

    public bool stopTicking;

    float timer;

    public override void Tick()
    {
        timer += Time.deltaTime;

        if (timer > 0.25 && !stopTicking)
        {
            RemoveLightSource();
            AddLightSource();

            timer = 0;
        }
        else if (stopTicking)
        {
            RemoveLightSource();
        }
    }

    void AddLightSource()
    {
        int intensity = VoxelLightHelper.GetCombinedLight(tc, localPos);
        Vector3Int color = TerrainModifier.CalculateRandomColor();

        intensity = intensity & 0xF0FF | (color.x << 8); //Red
        intensity = intensity & 0xFF0F | (color.y << 4); //Green
        intensity = intensity & 0xFFF0 | color.z; //Blue

        tc.voxelLightEngine.lampLightUpdateQueue.Enqueue(new VoxelLightEngine.LightNode(tc, localPos, intensity));
        tc.voxelLightEngine.CalculateLight();

        tc.quickUpdateFlag = true;
    }

    public void RemoveLightSource()
    {
        tc.voxelLightEngine.lampLightRemovalQueue.Enqueue(new VoxelLightEngine.LightNode(tc, localPos, VoxelLightHelper.GetCombinedLight(tc, localPos)));
        VoxelLightHelper.SetLampLightToZero(tc, localPos);
        tc.voxelLightEngine.CalculateLight();
    }
}
