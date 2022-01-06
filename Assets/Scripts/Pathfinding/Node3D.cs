using UnityEngine;
using System.Collections;

public class Node3D : IHeapItem<Node3D>
{
	public bool walkable;
	public Vector3 worldPosition;
	public int gridX;
	public int gridY;
	public int gridZ;

	public int gCost;
	public int hCost;
	public Node3D parent;
	int heapIndex;

	public Node3D(bool _walkable, int _gridX, int _gridY, int _gridZ)
	{
		walkable = _walkable;
		gridX = _gridX;
		gridY = _gridY;
		gridZ = _gridZ;
	}

	public int fCost
	{
		get
		{
			return gCost + hCost;
		}
	}

	public int HeapIndex
	{
		get
		{
			return heapIndex;
		}
		set
		{
			heapIndex = value;
		}
	}

	public int CompareTo(Node3D nodeToCompare)
	{
		int compare = fCost.CompareTo(nodeToCompare.fCost);
		if (compare == 0)
		{
			compare = hCost.CompareTo(nodeToCompare.hCost);
		}
		return -compare;
	}
}
