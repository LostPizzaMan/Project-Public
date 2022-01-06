using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothLighting 
{
	public static Color GetAverageLightValueForFront(TerrainChunk tc, int x, int y, int z, int vertNumber)
	{
		int n = GetLightValue(tc, x, y + 1, z);
		int s = GetLightValue(tc, x, y - 1, z);
		int w = GetLightValue(tc, x - 1, y, z);
		int e = GetLightValue(tc, x + 1, y, z);

		int nw = GetLightValue(tc, x - 1, y + 1, z);
		int ne = GetLightValue(tc, x + 1, y + 1, z);

		int sw = GetLightValue(tc, x - 1, y - 1, z);
		int se = GetLightValue(tc, x + 1, y - 1, z);

		var c = GetLightValue(tc, x, y, z);

		if (vertNumber == 0)
		{
			return CalculateAverage(c, w, sw, s);
		}
		else if (vertNumber == 1)
		{
			return CalculateAverage(c, w, nw, n);
		}
		else if (vertNumber == 2)
		{
			return CalculateAverage(c, e, ne, n);
		}
		else if (vertNumber == 3)
		{
			return CalculateAverage(c, e, se, s);
		}
		else
		{
			return Color.white;
		}
	}

	public static Color GetAverageLightValueForBack(TerrainChunk tc, int x, int y, int z, int vertNumber)
	{
		int n = GetLightValue(tc, x, y + 1, z);
		int s = GetLightValue(tc, x, y - 1, z);
		int w = GetLightValue(tc, x + 1, y, z);
		int e = GetLightValue(tc, x - 1, y, z);

		int nw = GetLightValue(tc, x + 1, y + 1, z);
		int ne = GetLightValue(tc, x - 1, y + 1, z);

		int sw = GetLightValue(tc, x + 1, y - 1, z);
		int se = GetLightValue(tc, x - 1, y - 1, z);

		var c = GetLightValue(tc, x, y, z);

		if (vertNumber == 0)
		{
			return CalculateAverage(c, w, sw, s);
		}
		else if (vertNumber == 1)
		{
			return CalculateAverage(c, w, nw, n);
		}
		else if (vertNumber == 2)
		{
			return CalculateAverage(c, e, ne, n);
		}
		else if (vertNumber == 3)
		{
			return CalculateAverage(c, e, se, s);
		}
		else
		{
			return Color.white;
		}
	}

	public static Color GetAverageLightValueForRight(TerrainChunk tc, int x, int y, int z, int vertNumber)
	{
		int n = GetLightValue(tc, x, y + 1, z);
		int s = GetLightValue(tc, x, y - 1, z);
		int w = GetLightValue(tc, x, y, z - 1);
		int e = GetLightValue(tc, x, y, z + 1);

		int nw = GetLightValue(tc, x, y + 1, z - 1);
		int ne = GetLightValue(tc, x, y + 1, z + 1);

		int sw = GetLightValue(tc, x, y - 1, z - 1);
		int se = GetLightValue(tc, x, y - 1, z + 1);

		var c = GetLightValue(tc, x, y, z);

		if (vertNumber == 0)
		{
			return CalculateAverage(c, w, sw, s);
		}
		else if (vertNumber == 1)
		{
			return CalculateAverage(c, w, nw, n);
		}
		else if (vertNumber == 2)
		{
			return CalculateAverage(c, e, ne, n);
		}
		else if (vertNumber == 3)
		{
			return CalculateAverage(c, e, se, s);
		}
		else
		{
			return Color.white;
		}
	}

	public static Color GetAverageLightValueForLeft(TerrainChunk tc, int x, int y, int z, int vertNumber)
	{
		int n = GetLightValue(tc, x, y + 1, z);
		int s = GetLightValue(tc, x, y - 1, z);
		int w = GetLightValue(tc, x, y, z + 1);
		int e = GetLightValue(tc, x, y, z - 1);

		int nw = GetLightValue(tc, x, y + 1, z + 1);
		int ne = GetLightValue(tc, x, y + 1, z - 1);

		int sw = GetLightValue(tc, x, y - 1, z + 1);
		int se = GetLightValue(tc, x, y - 1, z - 1);

		var c = GetLightValue(tc, x, y, z);

		if (vertNumber == 0)
		{
			return CalculateAverage(c, w, sw, s);
		}
		else if (vertNumber == 1)
		{
			return CalculateAverage(c, w, nw, n);
		}
		else if (vertNumber == 2)
		{
			return CalculateAverage(c, e, ne, n);
		}
		else if (vertNumber == 3)
		{
			return CalculateAverage(c, e, se, s);
		}
		else
		{
			return Color.white;
		}
	}

	public static Color GetAverageLightValueForTop(TerrainChunk tc, int x, int y, int z, int vertNumber)
	{
		int n = GetLightValue(tc, x, y, z + 1);
		int s = GetLightValue(tc, x, y, z - 1);
		int w = GetLightValue(tc, x - 1, y, z);
		int e = GetLightValue(tc, x + 1, y, z);

		int nw = GetLightValue(tc, x - 1, y, z + 1);
		int ne = GetLightValue(tc, x + 1, y, z + 1);

		int sw = GetLightValue(tc, x - 1, y, z - 1);
		int se = GetLightValue(tc, x + 1, y, z - 1);

		var c = GetLightValue(tc, x, y, z);

		if (vertNumber == 0)
		{
			return CalculateAverage(c, w, sw, s);
		}
		else if (vertNumber == 1)
		{
			return CalculateAverage(c, w, nw, n);
		}
		else if (vertNumber == 2)
		{
			return CalculateAverage(c, e, ne, n);
		}
		else if (vertNumber == 3)
		{
			return CalculateAverage(c, e, se, s);
		}
		else
		{
			return Color.white;
		}
	}

	public static Color GetAverageLightValueForBottom(TerrainChunk tc, int x, int y, int z, int vertNumber)
	{
		int n = GetLightValue(tc, x, y, z - 1);
		int s = GetLightValue(tc, x, y, z + 1);
		int w = GetLightValue(tc, x - 1, y, z);
		int e = GetLightValue(tc, x + 1, y, z);

		int nw = GetLightValue(tc, x - 1, y, z - 1);
		int ne = GetLightValue(tc, x + 1, y, z - 1);

		int sw = GetLightValue(tc, x - 1, y, z + 1);
		int se = GetLightValue(tc, x + 1, y, z + 1);

		var c = GetLightValue(tc, x, y, z);

		if (vertNumber == 0)
		{
			return CalculateAverage(c, w, nw, n);
		}
		else if (vertNumber == 1)
		{
			return CalculateAverage(c, e, ne, n);
		}
		else if (vertNumber == 2)
		{
			return CalculateAverage(c, e, se, s);
		}
		else if (vertNumber == 3)
		{
			return CalculateAverage(c, w, sw, s);
		}
        else
        {
			return Color.white;
        }
	}

	// ---- LIGHT UTILITIES ---- //

	static Color CalculateAverage(int x, int y, int z, int w)
	{
		if (y > 0 || w > 0)
		{
			float s = CalculateAverageForIndividual(x, y, z, w, 12);
			float r = CalculateAverageForIndividual(x, y, z, w, 8);
			float g = CalculateAverageForIndividual(x, y, z, w, 4);
			float b = CalculateAverageForIndividual(x, y, z, w, 0);

			return new Color(r, g, b, s);
		}
        else
        {
			return VoxelLightHelper.GetPackedLightColor(x); // x is centre block
        }
	}

	static float CalculateAverageForIndividual(int x, int y, int z, int w, int byteOffset)
    {
		int count = 0;
		float average = 0;

		int xValue = x >> byteOffset & 0xF;
		if (xValue != 0)
		{
			count++;
			average += xValue;
		}

		int yValue = y >> byteOffset & 0xF;
		if (yValue != 0)
		{
			count++;
			average += yValue;
		}

		int zValue = z >> byteOffset & 0xF;
		if (zValue != 0)
		{
			count++;
			average += zValue;
		}

		int wValue = w >> byteOffset & 0xF;
		if (wValue != 0)
		{
			count++;
			average += wValue;
		}

		if (count != 0)
		{
			return average / count / 15f;
		}
        else
        {
			return 0;
		}
	}

	static int GetLightValue(TerrainChunk tc, int x, int y, int z)
	{
		if (y < 1 || y >= 63) { return 0; }

		if (ChunkDataUtilities.IsVoxelInChunk(x, y, z))
		{
			return VoxelLightHelper.GetCombinedLight(tc, x, y, z);
		}
		else
		{
			// Returns the chunk instance at the provided world position 
			TerrainChunk newChunk = ChunkDataUtilities.GetChunkFromCache(tc, x, z);

			// This gives us the world position of the block
			int globalX = x + tc.chunkPos3D.x; 
			int globalZ = z + tc.chunkPos3D.z;

			// Subtract by the chunk position to get the local position
			int localX = globalX - newChunk.chunkPos3D.x; 
			int localZ = globalZ - newChunk.chunkPos3D.z; 

			return VoxelLightHelper.GetCombinedLight(newChunk, localX, y, localZ);
		}
	}
}
