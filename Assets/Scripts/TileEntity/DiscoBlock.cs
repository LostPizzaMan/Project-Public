using UnityEngine;

public class DiscoBlock : BlockBase
{
    public DiscoBlock(float _hardness, bool _transparent, int _rendertype) : base(0, "", _hardness, _transparent, _rendertype)
    {

    }

    public override void OnPlace(TileEntityManager tileEntityManager, Vector3 localPos, TerrainChunk tc)
    {
        Vector3Int localPosInt = new Vector3Int((int)localPos.x, (int)localPos.y, (int)localPos.z);

        DiscoTileEntity discoTileEntity = new DiscoTileEntity();
        discoTileEntity.tc = tc;
        discoTileEntity.localPos = localPosInt;

        tileEntityManager.AddTileEntity(discoTileEntity, tc.chunkPos3D + localPosInt);
    }

    public override void OnDestroy(TileEntityManager tileEntityManager, Vector3Int globalPos)
    {
        if (tileEntityManager.HasTileEntity(globalPos))
        {
            DiscoTileEntity discoTileEntity = (DiscoTileEntity)tileEntityManager.GetTileEntity(globalPos);
            discoTileEntity.stopTicking = true;

            tileEntityManager.RemoveTileEntity(globalPos);
        }
    }
}
