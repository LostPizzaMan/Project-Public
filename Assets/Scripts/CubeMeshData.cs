using UnityEngine;
using System.Collections;
using System;

public static class CubeMeshData {

	public static Vector3[] vertices = {
		new Vector3(0, 0, 0), //0
		new Vector3(1, 0, 0), //1
		new Vector3(1, 0, 1), //2
		new Vector3(0, 0, 1), //3
		new Vector3(0, 1, 0), //4
		new Vector3(1, 1, 0), //5
		new Vector3(1, 1, 1), //6
		new Vector3(0, 1, 1), //7
	};

	public static int[][] faceTriangles = {
		new int[] { 4, 7, 6, 5 }, // top
		new int[] { 0, 1, 2, 3 }, // bottom
		new int[] { 0, 4, 5, 1 }, // front 
		new int[] { 1, 5, 6, 2 }, // right 
		new int[] { 2, 6, 7, 3 }, // back
		new int[] { 3, 7, 4, 0 } // left
	};

	public static int[][] crossFaceTriangles = {
		new int[] { 0, 4, 6, 2 }, // left back
		new int[] { 2, 6, 4, 0 }, // left back
		new int[] { 3, 7, 5, 1 }, // right
		new int[] { 1, 5, 7, 3 }, // right back
	};

	readonly static Vector3[] offsets = {
		new Vector3 (0, 1, 0), // top
		new Vector3 (0, -1, 0), // bottom
		new Vector3 (0, 0, -1), // front
		new Vector3 (1, 0, 0), // right
		new Vector3 (0, 0, 1), // back
		new Vector3 (-1, 0, 0) // left
	};

	public static Vector3[] chunkNeighbourOffsets = {
		new Vector3 (0, 0, 16), // front
		new Vector3 (0, 0, -16), // back
		new Vector3 (-16, 0, 0), // right
		new Vector3 (16, 0, 0), // left
		new Vector3 (-16, 0, -16), // bottom left corner
		new Vector3 (-16, 0, 16), // top left corner
		new Vector3 (16, 0, -16), // bottom right corner
		new Vector3 (16, 0, 16) // top right corner
	};

	public static int[] invertChunkNeighbourOffsets = {
		1, // back
		0, // front
		3, // left
		2, // right
		7, // top right corner	
		6, // bottom right corner
		5, // top left corner
		4 // bottom left corner
	};

	// Returns the list of vertices associated with that one side.
	public static Vector3[] faceVertices(int dir, Vector3 pos) {
		Vector3[] fv = new Vector3[4];
		for (int i = 0; i < fv.Length; i++) {
			fv[i] = (pos + vertices[faceTriangles[dir][i]]);
		}
		return fv;
	}

	public static Vector3[] crossFaceVertices(int dir, Vector3 pos)
	{
		Vector3[] fv = new Vector3[4];
		for (int i = 0; i < fv.Length; i++)
		{
			fv[i] = (pos + vertices[crossFaceTriangles[dir][i]]);
		}
		return fv;
	}

	public static Vector3 GetNeighbor(int dir, Vector3 pos)
	{
		Vector3 offsetToCheck = offsets[dir];
		Vector3 neighborCoord = new Vector3(pos.x + offsetToCheck.x, pos.y + offsetToCheck.y, pos.z + offsetToCheck.z);

		return neighborCoord;
	}

	public static TerrainChunk GetChunkNeighbour(Vector3 pos, int dir)
	{
		Vector3 chunkNeighbour = pos + chunkNeighbourOffsets[dir];

		Vector2Int cp = new Vector2Int((int)chunkNeighbour.x, (int)chunkNeighbour.z);

		TerrainGenerator.chunks.TryGetValue(cp, out TerrainChunk tc);

		return tc;
	}

	static int to1D(int x, int y, int z)
	{
		return (z * 15 * 64) + (y * 15) + x;
	}

	static Vector3 to3D(int idx)
	{
		int z = idx / (15 * 64);
		idx -= (z * 15 * 64);
		int y = idx / 15;
		int x = idx % 15;
		return new Vector3(x, y, z);
	}
}
