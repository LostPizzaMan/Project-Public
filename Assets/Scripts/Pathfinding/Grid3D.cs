using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR;

public class Grid3D : MonoBehaviour
{
	public Pathfinding3D pathfinding3D;

	TerrainChunk tc;
	Vector2Int currentCP;

	public void StartAI()
	{
		tc = TerrainGenerator.chunks[currentCP];
	}

	public List<Node3D> GetNeighbours(Node3D node)
	{
		List<Node3D> neighbours = new List<Node3D>();

		for (int x = -1; x <= 1; x++)
		{
			for (int z = -1; z <= 1; z++)
			{
				for (int y = -1; y <= 1; y++)
				{
					if (x == 0 && y == 0 && z == 0)
						continue;

					int checkX = node.gridX + x;
					int checkY = node.gridY + y;
					int checkZ = node.gridZ + z;

					neighbours.Add(ReturnNode3DFromWorld(new Vector3(checkX, checkY, checkZ)));
				}
			}
		}

		return neighbours;
	}

	public Node3D ReturnNode3DFromWorld(Vector3 pointInTargetBlock)
	{
		if (pointInTargetBlock.y < 0 || pointInTargetBlock.y > 63) { pathfinding3D.AbortFindPath(); return new Node3D(false, 0, 0, 0); }

		int chunkPosX = Mathf.FloorToInt(pointInTargetBlock.x / 16) * 16;
		int chunkPosZ = Mathf.FloorToInt(pointInTargetBlock.z / 16) * 16;

		Vector2Int newCP = new Vector2Int(chunkPosX, chunkPosZ);

		if (currentCP.x != newCP.x || currentCP.y != newCP.y)
		{
			currentCP = newCP;

			tc = TerrainGenerator.chunks[currentCP];
		}

		//index of the target block
		int bix = (int)pointInTargetBlock.x - chunkPosX;
		int biy = (int)pointInTargetBlock.y;
		int biz = (int)pointInTargetBlock.z - chunkPosZ;

		Node3D node3D = tc.grid[bix, biy, biz];

		node3D.gridX = bix + chunkPosX;
		node3D.gridY = biy;
		node3D.gridZ = biz + chunkPosZ;

		node3D.worldPosition = new Vector3(node3D.gridX, node3D.gridY, node3D.gridZ);

		return node3D;
	}
}