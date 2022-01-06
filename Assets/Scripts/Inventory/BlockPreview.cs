using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPreview : MonoBehaviour
{
    public TextureDatabase textureDatabase;

    public MeshFilter meshFilter;

    public int blockID = 1;

    void Start()
    {
        BuildMesh();
    }

    Mesh mesh;

    public void BuildMesh()
    {
        if (!mesh) { mesh = new Mesh(); }

        mesh.Clear();

        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        List<Color> colors = new List<Color>();

        Vector3 blockPos = new Vector3(-0.5f, -0.5f, -0.5f);
        int numFaces = 0;

        for (int i = 0; i < 6; i++)
        {
            verts.AddRange(CubeMeshData.faceVertices(i, blockPos));

            Color lightLevel = Color.white;
            lightLevel.a = 1;

            colors.Add(lightLevel);
            colors.Add(lightLevel);
            colors.Add(lightLevel);
            colors.Add(lightLevel);

            numFaces++;

            if (i > 1)
            {
                uvs.AddRange(textureDatabase.blocks[blockID].side);
            }

            if (i == 0)
            {
                uvs.AddRange(textureDatabase.blocks[blockID].top);
            }

            if (i == 1)
            {
                uvs.AddRange(textureDatabase.blocks[blockID].bottom);
            }
        }

        int tl = verts.Count - 4 * numFaces;
        for (int i = 0; i < numFaces; i++)
        {
            tris.AddRange(new int[] { tl + i * 4, tl + i * 4 + 1, tl + i * 4 + 2, tl + i * 4, tl + i * 4 + 2, tl + i * 4 + 3 });
        }

        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.colors = colors.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    void OnDestroy()
    {
        if (meshFilter.sharedMesh) { Destroy(meshFilter.sharedMesh); }
    }
}
