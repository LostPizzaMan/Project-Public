using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuarryTileEntity : TileEntity
{
    public TerrainChunk tc;
    public Vector3Int localPos;

    float timer;

    public override void Init()
    {
        localPos.y--;
    }

    public override void Tick()
    {
        timer += Time.deltaTime;

        if (timer > 1 && localPos.y > 0)
        {
            TerrainModifier.CreateSphere(8, localPos, tc);

            localPos.y--;
            timer = 0;
        }
    }
}
