using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Hotbar : MonoBehaviour
{
    [Header("References")]

    public Inventory inventory;
    public TerrainModifier terrainModifier;
    public PlayerInstance playerInstance;
    public Q3Movement.Q3PlayerController Q3PlayerController;
    public Image hotbarHighlight;
    public GameObject inventoryUI;

    [Header("Item/Block Previews")]

    public GameObject itemPreview;
    public GameObject blockPreview;
    public Material sprite;

    [Header("Item/Block Info")]

    public int itemID;
    public int blockID;
    public int quantity;
    public string itemType;

    public int slotIndex { get; private set; }

    int oldValue;
    bool toggle;

    ItemBase heldItem;

    public Renderer itemPreviewRend;
    public Renderer blockPreviewRend;
    private MaterialPropertyBlock tintPropertyBlock;

    public Light itemHolderLight;

    void Start()
    {
        // Unrelated to hotbar, should move it...
        StartCoroutine(IsPlayersChunkLoaded());

        tintPropertyBlock = new MaterialPropertyBlock();
    }

    void FixedUpdate()
    {
        if (Q3PlayerController.enabled)
        {
            Vector3 playerPos = Q3PlayerController.gameObject.transform.position;
            
            if (playerPos.y > 0 && playerPos.y < 64)
            {
                Color tintColor = ChunkDataUtilities.GetLightLevel(playerPos);
                tintPropertyBlock.SetColor("_Color", tintColor);

                itemHolderLight.intensity = Mathf.Max(tintColor.r, tintColor.g, tintColor.b) / 2f;
            }
         
            itemPreviewRend.SetPropertyBlock(tintPropertyBlock);
            blockPreviewRend.SetPropertyBlock(tintPropertyBlock);
        }
    }

    void Update()
    {
        if (heldItem != null) { heldItem.OnUse(playerInstance); }

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {
            if (scroll > 0)
                slotIndex--;
            else
                slotIndex++;
        }

        if (Input.GetButtonDown("E"))
        {
            toggle = !toggle;

            if (toggle)
            {
                inventoryUI.SetActive(true);
                terrainModifier.enabled = false;
                Q3PlayerController.m_MouseLook.SetCursorLock(false);
            }
            else
            {
                inventoryUI.SetActive(false);
                terrainModifier.enabled = true;
                Q3PlayerController.m_MouseLook.SetCursorLock(true);
            }
        }

        UpdateHotbar();
    }

    IEnumerator IsPlayersChunkLoaded()
    {
        bool loaded = false;

        Vector3 playerPos = blockPreview.transform.position;

        while (!loaded)
        {
            Vector2Int cp = ChunkDataUtilities.GetChunkPosition(playerPos);
            TerrainGenerator.chunks.TryGetValue(cp, out TerrainChunk neighbourChunk);

            if (neighbourChunk && neighbourChunk.meshFilter.sharedMesh)
            {
                loaded = true;
            }

            yield return new WaitForSeconds(0.2f);
        }

        Q3PlayerController.enabled = true;
    }

    void UpdateHotbar()
    {
        if (slotIndex > 8)
            slotIndex = 0;
        if (slotIndex < 0)
            slotIndex = 8;

        hotbarHighlight.transform.position = inventory.slots[slotIndex].transform.position;
        itemID = inventory.items[slotIndex].id;

        if (itemID != oldValue)
        {
            oldValue = itemID;

            // -- ITEM PREVIEW LOGIC -- \\

            if (itemID != 0)
            {
                UpdatePreviewObjects();
                quantity = inventory.items[slotIndex].itemHandler.quantity;

                if (itemType == "Item")
                {
                    blockPreview.SetActive(false);
                    itemPreview.gameObject.SetActive(true);
                }
                else if (itemType == "Block")
                {
                    blockPreview.SetActive(true);
                    itemPreview.gameObject.SetActive(false);
                }
            }
            else
            {
                heldItem = null;
                quantity = 0;
                itemID = 0;
                blockID = 0;
                oldValue = 0;
                blockPreview.SetActive(false);
                itemPreview.gameObject.SetActive(false);
            }
        }
    }

    void UpdatePreviewObjects()
    {    
        heldItem = inventory.itemDatabase.FindByItemID(inventory.items[slotIndex].id);

        if (heldItem is Item item)
        {
            blockID = 0;

            sprite.SetTexture("_MainTex", Resources.Load<Texture2D>("Art/" + heldItem.sprite.name));
            itemPreview.GetComponent<ExtrudeSprite>().GenerateMesh();

            itemType = "Item";
        }
        else if (heldItem is BlockItem blockItem)
        {
            blockID = blockItem.blockID;

            BlockPreview blockPreviewScript = blockPreview.GetComponent<BlockPreview>();

            blockPreviewScript.blockID = blockID;
            blockPreviewScript.BuildMesh();

            itemType = "Block";
        }
    }   
}
