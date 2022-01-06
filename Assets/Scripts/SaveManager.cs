using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    [SerializeField] TerrainGenerator terrainGenerator;

    RegionFileManager regionFileManager = new RegionFileManager();

    public Queue<TerrainChunk> toSave = new Queue<TerrainChunk>();
    public Queue<TerrainChunk> toLoad = new Queue<TerrainChunk>();

    public bool threadLocked;

    public void RunSaveCycle()
    {
        if (toSave.Count > 0)
        {
            if (!threadLocked && !terrainGenerator.threadLocked)
            {
                threadLocked = true;

                TerrainChunk tc = toSave.Dequeue();

                if (tc.lightingFinished)
                {
                    ProcessSave(tc.blocks, tc.chunkPos3D.x, tc.chunkPos3D.z);
                }
                else
                {
                    toSave.Enqueue(tc);
                    threadLocked = false;
                }
            }
        }
    }

    async void ProcessSave(int[,,] blocks, int xPos, int zPos)
    {
        await Task.Run(() =>
        {
            SaveChunk(blocks, xPos, zPos);
            threadLocked = false;
        });
    }   

    void SaveChunk(int[,,] blocks, int xPos, int zPos)
    {
        regionFileManager.SaveChunk(blocks, xPos, zPos, regionFileManager.OpenRegionFile(xPos, zPos));
    }

    public bool TryLoadChunk(TerrainChunk tc, int xPos, int zPos)
    {        
        return regionFileManager.TryLoadChunk(tc, xPos, zPos, regionFileManager.OpenRegionFile(xPos, zPos));
    }

    // Make sure we close the FileStreams otherwise bad things happen
    private void OnDestroy()
    {
        regionFileManager.ClearRegionFileCache();
    }

    public void SetupSaveFolder(string saveFolderPath)
    {
        regionFileManager.saveFolderPath = saveFolderPath;
    }
}
