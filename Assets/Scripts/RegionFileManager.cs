using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

class RegionFileManager
{
    public string saveFolderPath;

    static int RegionHeaderSize = 4;
    static int TableHeaderSize = 4096;
    static int SectorSize = 1024;

    static int maxCacheSize = 4;

    public static Dictionary<Vector2Int, FileStream> regionFileCache = new Dictionary<Vector2Int, FileStream>();
    Queue<Vector2Int> loadedRegionFiles = new Queue<Vector2Int>();

    public FileStream OpenRegionFile(int x, int z)
    {
        x = (int)Mathf.Floor(x / 512f);
        z = (int)Mathf.Floor(z / 512f);

        Vector2Int regionPos = new Vector2Int(x, z);

        if (regionFileCache.ContainsKey(regionPos))
        {
            return regionFileCache[regionPos];
        }
        else
        {
            string region = GetRegionString(x, z);
            var fileName = Path.Combine(saveFolderPath + region + ".sav");
            FileStream fileStream = File.Open(fileName, FileMode.OpenOrCreate);

            loadedRegionFiles.Enqueue(regionPos);
            regionFileCache.Add(regionPos, fileStream);

            if (loadedRegionFiles.Count > maxCacheSize)
            {
                Vector2Int oldFileStream = loadedRegionFiles.Dequeue();

                if (oldFileStream != regionPos)
                {
                    Debug.Log("Unloading old filestream for region : " + oldFileStream);

                    regionFileCache[oldFileStream].Close();
                    regionFileCache.Remove(oldFileStream);
                }
            }

            return regionFileCache[regionPos];
        }
    }

    public void ClearRegionFileCache() 
    {
        foreach (KeyValuePair<Vector2Int, FileStream> entry in regionFileCache)
        {
            entry.Value.Close();
        }
    }

    //Attempt to load a chunk. Returns false on failure
    public bool TryLoadChunk(TerrainChunk tc, int x, int z, FileStream fileStream)
    {
        x = ConvertToLocalPosition(x);
        z = ConvertToLocalPosition(z);

        //Get the chunk sector offset
        int chunkSectorOffset = GetChunkSectorOffset(x, z, fileStream);
        Console.WriteLine("TryLoadChunk: " + chunkSectorOffset);
        //If chunkOffset is zero, it hasnt been saved
        if (chunkSectorOffset == 0)
        {
            return false;
        }

        //Location is not stored zero indexed, so that 0 indicates that it hasnt been saved
        chunkSectorOffset -= 1;

        //Seek to the Chunk Header
        SeekToChunk(chunkSectorOffset, fileStream);

        // Grabbing Chunk Header 
        byte[] chunkHeader = new byte[4];
        fileStream.Read(chunkHeader, 0, 4);
        int dataLength = ExtractIntFromFourByteArray(chunkHeader);

        // Grabbing Chunk Data
        byte[] byteBuffer = new byte[dataLength];
        fileStream.Read(byteBuffer, 0, dataLength);

        // Uncompressing Chunk Data
        var uncompressed = Ionic.Zlib.ZlibStream.UncompressBuffer(byteBuffer);

        // Copying Uncompressed Data To Chunk Instance
        Buffer.BlockCopy(uncompressed, 0, tc.blocks, 0, uncompressed.Length);

        return true;
    }

    public void SaveChunk(int[,,] chunkData, int x, int z, FileStream fileStream)
    {
        x = ConvertToLocalPosition(x);
        z = ConvertToLocalPosition(z);

        BinaryWriter binWriter = new BinaryWriter(fileStream);

        byte[] _copySectorsBuffer = new byte[0];

        int tableOffset = GetTableOffset(x, z);
        int chunkSectorOffset = GetChunkSectorOffset(x, z, fileStream);
        int totalSectors = ExtractTotalSectors(fileStream);

        int numOldSectors = 0;

        //If chunkOffset is zero, then we need to add the entry
        if (chunkSectorOffset == 0)
        {
            //Set the sector offset in the table
            fileStream.Seek(tableOffset + RegionHeaderSize, 0);
            binWriter.Write(ExtractFourByteArrayFromInt(totalSectors + 1));

            chunkSectorOffset = totalSectors;
        }
        else
        {
            //Convert sector offset from 1 indexed to 0 indexed
            chunkSectorOffset--;
            //seek to the chunk
            SeekToChunk(chunkSectorOffset, fileStream);

            // Get the chunk header 
            byte[] chunkHeader = new byte[4];
            fileStream.Read(chunkHeader, 0, 4);

            int oldDataLength = ExtractIntFromFourByteArray(chunkHeader);
            numOldSectors = SectorsFromBytes(oldDataLength + 4);
        }

        // Copying Data From Chunk Instance to Byte Array
        byte[] byteBuffer = new byte[chunkData.Length * sizeof(int)];
        Buffer.BlockCopy(chunkData, 0, byteBuffer, 0, byteBuffer.Length);

        // Compressing Chunk Data
        byte[] compressed = Ionic.Zlib.ZlibStream.CompressBuffer(byteBuffer);

        int numSectors = SectorsFromBytes(compressed.Length + 4);
        int sectorDiff = numSectors - numOldSectors;

        // If we need to resize the chunk, happens rarely
        if ((sectorDiff != 0) && ((chunkSectorOffset + numOldSectors) != totalSectors))
        {
            Debug.Log("REWRITING PAST SECTOR " + chunkSectorOffset + numOldSectors);

            SeekToChunk(chunkSectorOffset + numOldSectors, fileStream);

            int _copySectorsBufferSize = (totalSectors - (chunkSectorOffset + numOldSectors)) * SectorSize;

            _copySectorsBuffer = new byte[_copySectorsBufferSize];

            fileStream.Read(_copySectorsBuffer, 0, _copySectorsBufferSize);
        }

        // File Seeking to the Correct Spot
        SeekToChunk(chunkSectorOffset, fileStream);
        
        // Padding the Chunk
        int padLength = 0;

        if ((compressed.Length + 4) % SectorSize != 0)
        {
            padLength = SectorSize - (compressed.Length + 4) % SectorSize;
            if (padLength == SectorSize) padLength = 0;
        }

        byte[] padding = new byte[padLength];

        // Writing the Chunk Header and Chunk Data
        binWriter.Write(ExtractFourByteArrayFromInt(compressed.Length));
        binWriter.Write(compressed);
        binWriter.Write(padding);

        // Update Total Sectors Count
        InsertTotalSectors(fileStream, totalSectors += sectorDiff);

        if (_copySectorsBuffer.Length > 0)
        {
            SeekToChunk(chunkSectorOffset + numSectors, fileStream);

            //Write the buffer of sectors
            fileStream.Write(_copySectorsBuffer, 0, _copySectorsBuffer.Length);

            //Update the table
            int nextChunkSectorOffset;
            for (int i = 0; i < TableHeaderSize; i += 4)
            {
                byte[] bytes = new byte[4];
                fileStream.Seek(i + RegionHeaderSize, 0);
                fileStream.Read(bytes, 0, 4);

                nextChunkSectorOffset = ExtractIntFromFourByteArray(bytes);

                //See if the 1 indexed nextChunkSectorOffset is > the 0 indexed chunkSectorOffset
                if (nextChunkSectorOffset > (chunkSectorOffset + 1))
                {
                    fileStream.Seek(i + RegionHeaderSize, 0);
                    binWriter.Write(ExtractFourByteArrayFromInt(nextChunkSectorOffset + sectorDiff));
                }
            }
        }
    }

    public static int SectorsFromBytes(float dataLength)
    {
        //Adding 0.1f to be damn sure the cast is right
        return (int)(Math.Ceiling(dataLength / SectorSize) + 0.1f);
    }

    static void SeekToChunk(int chunkSectorOffset, FileStream fs)
    {
        int FullRegionHeaderSize = RegionHeaderSize + TableHeaderSize;
        int offset = FullRegionHeaderSize + (chunkSectorOffset * SectorSize);

        fs.Seek(offset, 0);
    }

    int ExtractTotalSectors(FileStream fs)
    {
        byte[] bytes = new byte[4];

        fs.Seek(0, 0);
        fs.Read(bytes, 0, 4);       

        return ExtractIntFromFourByteArray(bytes);
    }

    void InsertTotalSectors(FileStream fs, int value)
    {
        fs.Seek(0, 0);
        fs.Write(ExtractFourByteArrayFromInt(value), 0, 4);
    }

    static int GetTableOffset(int chunkX, int chunkZ)
    {
        int x = chunkX;
        int z = chunkZ;

        int tableOffset = 4 * ((x & 31) + (z & 31) * 32);

        return tableOffset;
    }

    static int GetChunkSectorOffset(int chunkX, int chunkZ, FileStream fs)
    {
        int tableOffset = GetTableOffset(chunkX, chunkZ);

        byte[] bytes = new byte[4];

        fs.Seek(tableOffset + RegionHeaderSize, SeekOrigin.Begin);
        fs.Read(bytes, 0, 4);

        return ExtractIntFromFourByteArray(bytes);
    }

    static byte[] ExtractFourByteArrayFromInt(int data)
    {
        byte[] bytes = new byte[4];

        bytes[0] = (byte)(data >> 24);
        bytes[1] = (byte)(data >> 16);
        bytes[2] = (byte)(data >> 8);
        bytes[3] = (byte)(data);

        return bytes;
    }

    static int ExtractIntFromFourByteArray(byte[] bytes)
    {
        int offset = 0;

        offset = offset | ((bytes[0] & 0xFF) << 24);
        offset = offset | ((bytes[1] & 0xFF) << 16);
        offset = offset | ((bytes[2] & 0xFF) << 8);
        offset = offset | (bytes[3] & 0xFF);

        return offset;
    }

    static void PrintByteArray(byte[] bytes)
    {
        var sb = new StringBuilder("new byte[] { ");
        foreach (var b in bytes)
        {
            if (b != 0)
            {
                sb.Append(b + ", ");
            }
        }
        sb.Append("}");
        Console.WriteLine(sb.ToString());
    }

    static int ConvertToLocalPosition(int value)
    {
        int regionPos = (int)Mathf.Floor(value / 512f);

        value -= (regionPos * 512);
        value /= 16;

        return value;
    }

    string GetRegionString(int x, int z)
    {
        return "r." + x + "." + z;
    }
}

