using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System;
using Debug = UnityEngine.Debug;

public class Pathfinding3D : MonoBehaviour
{
	public int maxSearchLength = 512;

	PathRequestManager requestManager;
	Grid3D grid;

	void Awake()
	{
		requestManager = GetComponent<PathRequestManager>();
		grid = GetComponent<Grid3D>();
	}

	public void StartFindPath(Vector3 startPos, Vector3 targetPos)
	{
		StartCoroutine(FindPath(startPos, targetPos));
	}

	public void AbortFindPath()
    {
		abortSearch = true;

		Vector3[] waypoints = new Vector3[0];
		bool pathSuccess = false;

		requestManager.FinishedProcessingPath(waypoints, pathSuccess);

		abortSearch = false;
	}

	Stopwatch sw = new Stopwatch();

	bool abortSearch;
	IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
	{
		//sw.Start();

		Node3D startNode = grid.ReturnNode3DFromWorld(ChunkDataUtilities.GetGridSnappedPosition(startPos));
		Node3D targetNode = grid.ReturnNode3DFromWorld(ChunkDataUtilities.GetGridSnappedPosition(targetPos));

		Vector3[] waypoints = new Vector3[0];
		bool pathSuccess = false;

		Heap<Node3D> openSet = new Heap<Node3D>(maxSearchLength);
		HashSet<Node3D> closedSet = new HashSet<Node3D>();
		Node3D currentNode = targetNode;
		openSet.Add(startNode);

		while (openSet.Count > 0)
		{
			currentNode = openSet.RemoveFirst();
			closedSet.Add(currentNode);

			if (currentNode.gridX == targetNode.gridX)
			{
				if (currentNode.gridZ == targetNode.gridZ)
				{
					//sw.Stop();
					//print(sw.ElapsedMilliseconds + " ms");
					pathSuccess = true;
					break;
				}
			}

			foreach (Node3D neighbour in grid.GetNeighbours(currentNode))
			{
				if (abortSearch || closedSet.Count > 256) { AbortFindPath(); yield break; }

				if (!neighbour.walkable || closedSet.Contains(neighbour))
				{
					continue;
				}

				int newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour);
				if (newCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
				{
					neighbour.gCost = newCostToNeighbour;
					neighbour.hCost = GetDistance(neighbour, targetNode);
					neighbour.parent = currentNode;

					if (!openSet.Contains(neighbour))
						if (!openSet.Add(neighbour)) { AbortFindPath(); yield break; }
						else
							openSet.UpdateItem(neighbour);
				}
			}
		}

		yield return null;

		if (pathSuccess)
		{
			waypoints = RetracePath(startNode, currentNode);
			pathSuccess = waypoints.Length > 0;
		}

		requestManager.FinishedProcessingPath(waypoints, pathSuccess);
	}

	Vector3[] RetracePath(Node3D startNode, Node3D endNode)
	{
		List<Node3D> path = new List<Node3D>();
		Node3D currentNode = endNode;

		while (currentNode != startNode)
		{
			if (currentNode.parent != null && path.Count < 256)
			{
				path.Add(currentNode);
				currentNode = currentNode.parent;
			}
            else
            {
				Vector3[] waypointsZero = new Vector3[0];
				return waypointsZero;
            }
		}

		Vector3[] waypoints = SimplifyPath(path);
		Array.Reverse(waypoints);
		return waypoints;
	}

	Vector3[] SimplifyPath(List<Node3D> path)
	{
		List<Vector3> waypoints = new List<Vector3>();
		Vector3 directionOld = Vector2.zero;

		for (int i = 1; i < path.Count; i++)
		{
			//Vector3 directionNew = new Vector3(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY, path[i - 1].gridZ - path[i].gridZ);
			//if (directionNew != directionOld)
			//{
				path[i].worldPosition.x = path[i].worldPosition.x + 0.5f;
				path[i].worldPosition.y = path[i].worldPosition.y + 2f;
				path[i].worldPosition.z = path[i].worldPosition.z + 0.5f;
				waypoints.Add(path[i].worldPosition);
			//}
			//directionOld = directionNew;
		}
		return waypoints.ToArray();
	}

	int GetDistance(Node3D nodeA, Node3D nodeB)
	{
		int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
		int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);
		int dstZ = Mathf.Abs(nodeA.gridZ - nodeB.gridZ);

		if (dstX > dstZ)
			return 14 * dstZ + 10 * (dstX - dstZ) + dstY;
		return 14 * dstX + 10 * (dstZ - dstX) + dstY;
	}
}
