using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// -- REFACTOR REQUIRED -- \\

public class ChunkPhysics : MonoBehaviour
{
    public GameObject physicsChunkPrefab;

    public void CheckForUnattachedBlocks(int x, int y, int z, TerrainChunk tc)
    {
        List<Vector3> initialSet = new List<Vector3>();

        for (int i = 0; i < 6; i++)
        {
            Vector3 blockNeighbor = CubeMeshData.GetNeighbor(i, new Vector3(x, y, z));
            if (ChunkDataUtilities.IsVoxelInChunk((int)blockNeighbor.x, (int)blockNeighbor.y, (int)blockNeighbor.z))
            {
                if (tc.blocks[(int)blockNeighbor.x, (int)blockNeighbor.y, (int)blockNeighbor.z] != 0)
                {
                    initialSet.Add(blockNeighbor);
                }
            }
        }

        while (initialSet.Count > 0)
        {
            Queue<Vector3> openSet = new Queue<Vector3>();
            HashSet<Vector3> closedSet = new HashSet<Vector3>();
            int maxBlocks = 500;
            int blocksSoFar = 0;

            Vector3 neighbor = initialSet[0];
            initialSet.Remove(neighbor);

            openSet.Enqueue(neighbor);

            while (openSet.Count > 0)
            {
                Vector3 pos = openSet.Dequeue();

                if (closedSet.Contains(pos))
                {
                    continue;
                }

                if (blocksSoFar > maxBlocks)
                {
                    //Debug.Log("Max block limit reached, aborting search...");
                    break;
                }

                blocksSoFar++;
                closedSet.Add(pos);

                for (int i = 0; i < 6; i++)
                {
                    Vector3 blockNeighbor = CubeMeshData.GetNeighbor(i, new Vector3(pos.x, pos.y, pos.z));
                    if (ChunkDataUtilities.IsVoxelInChunk((int)blockNeighbor.x, (int)blockNeighbor.y, (int)blockNeighbor.z))
                    {
                        if (tc.blocks[(int)blockNeighbor.x, (int)blockNeighbor.y, (int)blockNeighbor.z] != 0)
                        {
                            openSet.Enqueue(blockNeighbor);
                        }
                    }
                }
            }

            if (blocksSoFar < maxBlocks)
            {
                Debug.Log("Building Physics Chunk...");

                GameObject chunkGO = Instantiate(physicsChunkPrefab, tc.transform.position, Quaternion.identity);
                PhysicsChunk physicsChunk = chunkGO.GetComponent<PhysicsChunk>();
                physicsChunk.textureDatabase = tc.textureDatabase;
                foreach (Vector3 pos in closedSet)
                {
                    int blockType = tc.blocks[(int)pos.x, (int)pos.y, (int)pos.z];
                    physicsChunk.blocks[(int)pos.x, (int)pos.y, (int)pos.z] = blockType;
                    tc.blocks[(int)pos.x, (int)pos.y, (int)pos.z] = 0;
                }

                tc.updateFlag = true;
            }
        }
    }
}
