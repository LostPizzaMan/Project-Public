using EasyButtons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceBuilder : MonoBehaviour
{
    Mesh mesh;
    int x;

    List<Vector3> vertices;
    List<int> triangles;
    List<Vector2> uvs;
    List<Vector3> frameData;

    [SerializeField] TextureDatabase textureDatabase;

    void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;

        vertices = new List<Vector3>();
        triangles = new List<int>();
        uvs = new List<Vector2>();
        frameData = new List<Vector3>();
    }

    [Button]
    void Run()
    {
        MakeMeshData();
        CreateMesh();
    }

    int faceCount = 0;

    void MakeMeshData()
    {
        faceCount = 0;

        float offset16x = 1.0f / (textureDatabase.itemDatabase.TextureAtlas.height / 16);
        float offset32x = 1.0f / (textureDatabase.itemDatabase.TextureAtlas.height / 32);
        float offset64x = 1.0f / (textureDatabase.itemDatabase.TextureAtlas.height / 64);
        float offset128x = 1.0f / (textureDatabase.itemDatabase.TextureAtlas.height / 128);

        AddFaceVertices(0, new Vector3(0, 0, 0));
        AddFaceVertices(0, new Vector3(1, 0, 0));
        AddFaceVertices(0, new Vector3(-1, 0, 0));
        AddFaceVertices(0, new Vector3(0, 0, 1));
        AddFaceVertices(0, new Vector3(0, 0, -1));
        AddFaceVertices(0, new Vector3(-1, 0, -1));

        AddAnimatedFace(offset16x, 26, 12f, 250);
        AddAnimatedFace(offset32x, 27, 6f, 100);
        AddAnimatedFace(offset32x, 28, 32f, 250);
        AddAnimatedFace(offset128x, 29, 10f, 250);
        AddAnimatedFace(offset128x, 30, 8f, 1000);

        uvs.AddRange(textureDatabase.blocks[2].top);
        AddFrameData(0, 0, 0);

        int tl = vertices.Count - 4 * faceCount;
        for (int i = 0; i < faceCount; i++)
        {
            triangles.AddRange(new int[] { tl + i * 4, tl + i * 4 + 1, tl + i * 4 + 2, tl + i * 4, tl + i * 4 + 2, tl + i * 4 + 3 });
        }
    }

    void AddFaceVertices(int dir, Vector3 pos)
    {
        vertices.AddRange(CubeMeshData.faceVertices(dir, pos));
        faceCount++;
    }

    void AddAnimatedFace(float offset, int blockID, float frameAmount, int speed)
    {
        AddFrameData(offset, frameAmount, speed);

        Vector2[] customUVs = textureDatabase.blocks[blockID].top;

        customUVs[0].y = customUVs[2].y - offset;
        customUVs[3].y = customUVs[2].y - offset;

        uvs.AddRange(customUVs);
    }

    void AddFrameData(float offset, float frameAmount, int speed)
    {
        frameData.Add(new Vector3(offset, frameAmount, speed));
        frameData.Add(new Vector3(offset, frameAmount, speed));
        frameData.Add(new Vector3(offset, frameAmount, speed));
        frameData.Add(new Vector3(offset, frameAmount, speed));
    }

    void CreateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.SetUVs(3, frameData.ToArray());

        mesh.RecalculateNormals();
    }
}
