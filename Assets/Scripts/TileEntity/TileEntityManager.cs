using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileEntityManager : MonoBehaviour
{
    List<TileEntity> tickableTileEntities = new List<TileEntity>();
    Dictionary<Vector3Int, TileEntity> loadedTileEntities = new Dictionary<Vector3Int, TileEntity>();

    void FixedUpdate()
    {
        foreach(TileEntity tileEntity in tickableTileEntities)
        {
            tileEntity.Tick();
        }
    }

    public void AddTileEntity(TileEntity tileEntity, Vector3Int globalPos)
    {
        tileEntity.Init();

        tickableTileEntities.Add(tileEntity);
        loadedTileEntities.Add(globalPos, tileEntity);
    }

    public void RemoveTileEntity(Vector3Int globalPos)
    {
        tickableTileEntities.Remove(loadedTileEntities[globalPos]);
        loadedTileEntities.Remove(globalPos);
    }

    public TileEntity GetTileEntity(Vector3Int globalPos)
    {
        return loadedTileEntities[globalPos];
    }

    public bool HasTileEntity(Vector3Int globalPos)
    {
        return loadedTileEntities.ContainsKey(globalPos);
    }
}

public class TileEntity 
{
    public virtual void Init() { }

    public virtual void Tick() { }
}