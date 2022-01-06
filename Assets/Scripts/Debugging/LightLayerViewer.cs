using EasyButtons;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class LightLayerViewer : MonoBehaviour
{
    public RawImage image;
    public TerrainChunk terrainChunk;

    [Button]
    async void RunTask()
    {
        image.enabled = true;

        Texture2D texture = new Texture2D(16, 16);
        texture.filterMode = FilterMode.Point;

        for (int i = 0; i < 64; i++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    Color tintColor = ChunkDataUtilities.GetLightLevel(terrainChunk.chunkPos3D + new Vector3Int(x, i, y));

                    texture.SetPixel(x, y, tintColor);
                }
            }

            texture.Apply();
            image.texture = texture;

            await Task.Delay(250);
        }

        image.enabled = false;
    }
}
