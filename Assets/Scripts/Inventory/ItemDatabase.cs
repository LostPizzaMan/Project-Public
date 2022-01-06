using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using LitJson;
using System;

public class ItemDatabase : MonoBehaviour
{
    // -- Database -- //
    public List<ItemBase> items = new List<ItemBase>();
    public List<BlockBase> blocks = new List<BlockBase>();
    public Dictionary<string, int> stringItemsLookup = new Dictionary<string, int>();

    // -- Json Data -- //
    private JsonData blockData, itemData;

    // -- Texture Atlas Data -- //
    public TextureDatabase textureDatabase;
    public Dictionary<string, Vector4> uvCoords = new Dictionary<string, Vector4>();

    public Texture2D TextureAtlas;

    public Material terrainAtlas;
    public Material transparentTerrainAtlas;
    public Material infiniteBlock;

    void Awake()
    {
        Debug.Log("[Core] Loading JSON Data...");
        try
        {
            blockData = JsonMapper.ToObject(File.ReadAllText(Application.dataPath + "/Resources/Data/Blocks.json"));
            itemData = JsonMapper.ToObject(File.ReadAllText(Application.dataPath + "/Resources/Data/Items.json"));
        }
        catch
        {
            Debug.Log("[Core] ERROR! JSON DATA MISSING OR INVAILD.");
        }

        BuildTextureAtlas();
        ReloadItemDatabase();
        //LoadTexturesAsBlocks();
    }

    void BuildTextureAtlas()
    {
        Texture2D[] textures = Resources.LoadAll<Texture2D>("Art/Blocks/");

        TextureAtlas = new Texture2D(256, 256, TextureFormat.ARGB32, false);
        TextureAtlas.filterMode = FilterMode.Point;

        Rect[] rect = TextureAtlas.PackTextures(textures, 0, 16384);

        for (int i = 0; i < textures.Length; i++)
        {
            Vector4 uvCoord = new Vector4(rect[i].xMin, rect[i].xMax, rect[i].yMin, rect[i].yMax);

            uvCoords.Add(textures[i].name, uvCoord);
        }

        terrainAtlas.SetTexture("_MainTex", TextureAtlas);
        transparentTerrainAtlas.SetTexture("_MainTex", TextureAtlas);
        infiniteBlock.SetTexture("_MainTex", TextureAtlas);
    }

    void LoadTexturesAsBlocks()
    {
        Texture2D[] textures = Resources.LoadAll<Texture2D>("Art/Blocks/Custom");

        for (int i = 0; i < textures.Length; i++)
        {
            int newBlockID = blocks.Count;

            textureDatabase.AddTexture(newBlockID, textures[i].name);

            // Registering Blocks
            blocks.Add(new BlockBase(newBlockID, textures[i].name, 2,  true, 0));

            // Registering Block Items
            RegisterItem(new BlockItem(newBlockID, items.Count, textures[i].name, textures[i].name));
        }
        
    }

    public void ReloadItemDatabase()
    {
        items.Clear();

        items.Add(new BlockItem(0, 0, "Air", "[Error]"));

        // Loading JSON Items

        foreach (JsonData item in itemData)
        {
            RegisterItem(new Item(items.Count, (string)item["stringID"], (string)item["itemName"], (string)item["itemSprite"]));
        }

        // Loading JSON Blocks

        foreach (JsonData item in blockData)
        {
            double hardness = (double)item["hardness"];
            int newBlockID = blocks.Count;

            for (int i = 0; i < item["tiles"].Count; i++)
            {
                if (!textureDatabase.blocks.ContainsKey(newBlockID))
                {
                    if (item["tiles"].Count > 1)
                    {
                        textureDatabase.AddTexture(newBlockID, (string)item["tiles"][0], (string)item["tiles"][1], (string)item["tiles"][2]);
                    }
                    else if (item["tiles"].Count == 1)
                    {
                        textureDatabase.AddTexture(newBlockID, (string)item["tiles"][0]);
                    }
                }
            }

            // Registering Blocks
            blocks.Add(new BlockBase(newBlockID, (string)item["name"], (float)hardness, (bool)item["transparent"], (int)item["rendertype"]));

            // Registering Block Items
            RegisterItem(new BlockItem(newBlockID, items.Count, (string)item["stringID"], (string)item["name"]));
        }

        // Loading Custom Blocks

        RegisterCustomBlock(new DiscoBlock(0.25f, false, 0), "disco", "Disco", "DiscoTexture");
        RegisterCustomBlock(new QuarryBlock(0.25f, false, 0), "quarry", "Quarry", "Dirt");
        RegisterCustomBlock(new MushroomBlock(0.0f, true, 1), "fungus", "Warped Fungus", "warped_fungus");

        // Loading Custom Items

        RegisterItem(new Stick(items.Count, "stick", "Stick", "stick"));
        RegisterItem(new Boom(items.Count, "boom", "Boom", "boom"));
        RegisterItem(new LightSource(items.Count, "light", "Light Source", "light"));
        RegisterItem(new SpawnLad(items.Count, "SpawnLad", "Spawn Lad", "SpawnLad"));
    }

    void RegisterItem(ItemBase itemBase)
    {
        stringItemsLookup.Add(itemBase.stringID, items.Count);
        items.Add(itemBase);
    }

    void RegisterCustomBlock(BlockBase customBlock, string stringID, string name, string textureName)
    {
        int blockID = blocks.Count;

        customBlock.id = blockID;
        customBlock.name = name;

        textureDatabase.AddTexture(blockID, textureName);
        blocks.Add(customBlock);
        RegisterItem(new BlockItem(blockID, items.Count, stringID, name));
    }

    public ItemBase FindByItemID(int id)
    {
        if (id < items.Count && id > -1)
        {
            return items[id];
        }

        return null;
    }

    public int FindByStringID(string id)
    {
        if (stringItemsLookup.TryGetValue(id, out int itemID))
        {
            return itemID;
        }

        return 0;
    }

    public ItemBase FindByBlockID(int id)
    {
        foreach (ItemBase item in items)
        {
            if (item is BlockItem blockItem)
            {
                if (blockItem.blockID == id)
                {
                    return item;
                }
            }
        }
        return null;
    }

    public int ReturnBlockIDByName(string id)
    {
        if (stringItemsLookup.TryGetValue(id, out int itemID))
        {
            BlockItem blockItem = FindByItemID(itemID) as BlockItem;

            return blockItem.blockID;
        };

        return 0;
    }

    public Vector4 FindTextureCoordByName(string name)
    {
        foreach (KeyValuePair<string, Vector4> entry in uvCoords)
        {
            if (entry.Key == name)
            {
                return entry.Value;
            }
        }

        Debug.LogError("Failed to return texture coordinates for texture \"" + name + "\" Is file missing or misspelled?");

        return uvCoords["[Error]"]; 
    }
}

public abstract class ItemBase
{
    public int itemID;
    public string stringID;
    public string name;
    public Sprite sprite;
    public string itemType;

    public ItemBase()
    {
        itemID = 0;
        stringID = "MissingNo.";
        name = "MissingNo.";
        sprite = Resources.Load<Sprite>("Art/" + "[Error]");
        itemType = "MissingNo.";
    }

    public virtual void OnUse(PlayerInstance playerInstance) { }
}

public class BlockItem : ItemBase
{
    public int blockID;

    public BlockItem(int _blockID, int _itemID, string _stringID, string _name)
    {
        blockID = _blockID;
        itemID = _itemID;
        stringID = _stringID;
        name = _name;
        sprite = Resources.Load<Sprite>("Art/" + "[Error]");
        itemType = "Block";
    }
}

public class Item : ItemBase
{
    public Item(int _itemID, string _stringID, string _name, string _sprite)
    {
        itemID = _itemID;
        stringID = _stringID;
        name = _name;
        sprite = Resources.Load<Sprite>("Art/" + _sprite);
        itemType = "Item";
    }
}

public class BlockBase
{
    public int id;
    public string name;
    public float hardness;
    public bool transparent;
    public int rendertype;

    public BlockBase(int _id, string _name, float _hardness, bool _transparent, int _rendertype)
    {
        id = _id;
        name = _name;
        hardness = _hardness;
        transparent = _transparent;
        rendertype = _rendertype;
    }

    public virtual void OnPlace(TileEntityManager tileEntityManager, Vector3 blockPos, TerrainChunk tc) { }

    public virtual void OnDestroy(TileEntityManager tileEntityManager, Vector3Int globalPos)  { }
}

