using UnityEngine;

public class MushroomBlock : BlockBase
{
    public TerrainChunk tc;
    public Vector3Int localPos;

    public MushroomBlock(float _hardness, bool _transparent, int _rendertype) : base(0, "", _hardness, _transparent, _rendertype)
    {

    }

    public override void OnPlace(TileEntityManager tileEntityManager, Vector3 _localPos, TerrainChunk _tc)
    {
        Vector3Int localPosInt = new Vector3Int((int)_localPos.x, (int)_localPos.y, (int)_localPos.z);

        tc = _tc;
        localPos = localPosInt;

        AddLightSource();
    }

    public override void OnDestroy(TileEntityManager tileEntityManager, Vector3Int globalPos)
    {
        RemoveLightSource();
    }

    void AddLightSource()
    {
        int intensity = VoxelLightHelper.GetCombinedLight(tc, localPos);
        Vector3Int color = TerrainModifier.CalculateLuminanceFromHex("155555");

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
