using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelLightHelper : MonoBehaviour
{
    public static int GetSunlight(TerrainChunk tc, Vector3Int blockIndex)
    {
        return (tc.lightMap[blockIndex.x, blockIndex.y, blockIndex.z] >> 12) & 0xF;
    }

    public static void SetSunlight(TerrainChunk tc, Vector3Int blockIndex, int val)
    {
        tc.lightMap[blockIndex.x, blockIndex.y, blockIndex.z] = (tc.lightMap[blockIndex.x, blockIndex.y, blockIndex.z] & 0xFFF) | (val << 12);
    }

    public static int GetRedLight(TerrainChunk tc, Vector3Int blockIndex)
    {
        return (tc.lightMap[blockIndex.x, blockIndex.y, blockIndex.z] >> 8) & 0xF;
    }

    public static void SetRedLight(TerrainChunk tc, Vector3Int blockIndex, int val)
    {
        tc.lightMap[blockIndex.x, blockIndex.y, blockIndex.z] = (tc.lightMap[blockIndex.x, blockIndex.y, blockIndex.z] & 0xF0FF) | (val << 8);
    }

    public static int GetGreenLight(TerrainChunk tc, Vector3Int blockIndex)
    {
        return (tc.lightMap[blockIndex.x, blockIndex.y, blockIndex.z] >> 4) & 0xF;
    }

    public static void SetGreenLight(TerrainChunk tc, Vector3Int blockIndex, int val)
    {
        tc.lightMap[blockIndex.x, blockIndex.y, blockIndex.z] = (tc.lightMap[blockIndex.x, blockIndex.y, blockIndex.z] & 0xFF0F) | (val << 4);
    }

    public static int GetBlueLight(TerrainChunk tc, Vector3Int blockIndex)
    {
        return (tc.lightMap[blockIndex.x, blockIndex.y, blockIndex.z]) & 0xF;
    }

    public static void SetBlueLight(TerrainChunk tc, Vector3Int blockIndex, int val)
    {
        tc.lightMap[blockIndex.x, blockIndex.y, blockIndex.z] = (tc.lightMap[blockIndex.x, blockIndex.y, blockIndex.z] & 0xFFF0) | (val);
    }

    public static int GetCombinedLight(TerrainChunk tc, Vector3Int blockIndex)
    {
        return tc.lightMap[blockIndex.x, blockIndex.y, blockIndex.z];
    }
    public static int GetCombinedLight(TerrainChunk tc, int x, int y, int z)
    {
        return tc.lightMap[x, y, z];
    }

    public static void SetCombinedLight(TerrainChunk tc, Vector3Int blockIndex, int val)
    {
        tc.lightMap[blockIndex.x, blockIndex.y, blockIndex.z] = val;
    }

    public static void SetLampLightToZero(TerrainChunk tc, Vector3Int blockIndex)
    {
        int intensty = VoxelLightHelper.GetCombinedLight(tc, blockIndex);
        intensty = (intensty & 0xF0FF) | 0 << 8;
        intensty = (intensty & 0xFF0F) | 0 << 4;
        intensty = (intensty & 0xFFF0) | 0;
        SetCombinedLight(tc, blockIndex, intensty);
    }

    public static void SetLampLight(TerrainChunk tc, Vector3Int blockIndex, int r, int g, int b)
    {
        int intensty = VoxelLightHelper.GetCombinedLight(tc, blockIndex);
        intensty = (intensty & 0xF0FF) | r << 8;
        intensty = (intensty & 0xFF0F) | g << 4;
        intensty = (intensty & 0xFFF0) | b;
        SetCombinedLight(tc, blockIndex, intensty);
    }

    // -- GET COLOR FOR TINTING -- \\

    public static Color GetPackedLightColor(int value)
    {
        Color lightLevel = new Color
        {
            a = ((value >> 12) & 0xF) / 15f,
            r = ((value >> 8) & 0xF) / 15f,
            g = ((value >> 4) & 0xF) / 15f,
            b = ((value) & 0xF) / 15f
        };

        return lightLevel;
    }

    public static Color GetUnpackedLightColor(int value)
    {
        Color colorPacked = VoxelLightHelper.GetPackedLightColor(value);

        Color lightIntensity = new Color
        {
            a = 1,
            r = Mathf.Clamp(colorPacked.r + colorPacked.a, 0.0625f, 1),
            g = Mathf.Clamp(colorPacked.g + colorPacked.a, 0.0625f, 1),
            b = Mathf.Clamp(colorPacked.b + colorPacked.a, 0.0625f, 1),
        };

        // colorPacked.a is sunlight, colorPacked.rgb is lamplight

        return lightIntensity;
    }
}

