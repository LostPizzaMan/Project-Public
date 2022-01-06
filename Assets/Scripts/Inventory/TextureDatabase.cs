using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureDatabase : MonoBehaviour
{
    public ItemDatabase itemDatabase;
    public Dictionary<int, BlockTexture> blocks = new Dictionary<int, BlockTexture>();

    Vector4 coord;  // The vector4 coord represents the (xMin, xMax, yMin, yMax) of the rect

    public Vector2[] GetTextureUVS(string name)
    {
        coord = itemDatabase.FindTextureCoordByName(name);

        Vector2[] uvs = new Vector2[]
        {
            new Vector2(coord.x, coord.z),
            new Vector2(coord.x, coord.w),
            new Vector2(coord.y, coord.w),
            new Vector2(coord.y, coord.z),
        };

        return uvs;
    }

    public class BlockTexture
    {
        public Vector2[] top, side, bottom;

        public BlockTexture(string tile, TextureDatabase textureDatabase)
        {
            Vector2[] array = textureDatabase.GetTextureUVS(tile);
            top = side = bottom = array;
        }

        public BlockTexture(string top, string side, string bottom, TextureDatabase textureDatabase)
        {
            this.top = textureDatabase.GetTextureUVS(top);
            this.side = textureDatabase.GetTextureUVS(side);
            this.bottom = textureDatabase.GetTextureUVS(bottom);
        }
    }

    public void AddTexture(int id, string name)
    {
        blocks.Add(id, new BlockTexture(name, this));
    }

    public void AddTexture(int id, string top, string side, string bottom)
    {
        blocks.Add(id, new BlockTexture(top, side, bottom, this));
    }
}
