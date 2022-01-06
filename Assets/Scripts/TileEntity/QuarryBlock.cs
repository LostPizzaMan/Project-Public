using UnityEngine;

public class QuarryBlock : BlockBase
{
    public QuarryBlock(float _hardness, bool _transparent, int _rendertype) : base(0, "", _hardness, _transparent, _rendertype)
    {

    }

    public override void OnPlace(TileEntityManager tileEntityManager, Vector3 localPos, TerrainChunk tc)
    {
        Vector3Int localPosInt = new Vector3Int((int)localPos.x, (int)localPos.y, (int)localPos.z);

        QuarryTileEntity quarryTileEntity = new QuarryTileEntity();
        quarryTileEntity.tc = tc;
        quarryTileEntity.localPos = localPosInt;

        tileEntityManager.AddTileEntity(quarryTileEntity, tc.chunkPos3D + localPosInt);
    }

    public override void OnDestroy(TileEntityManager tileEntityManager, Vector3Int globalPos)
    {
        if (tileEntityManager.HasTileEntity(globalPos))
        {
            tileEntityManager.RemoveTileEntity(globalPos);
        }
    }
}
