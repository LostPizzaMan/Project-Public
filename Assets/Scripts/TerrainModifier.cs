using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class TerrainModifier : MonoBehaviour
{
    [Header("References")]

    [SerializeField] private Camera cam;
    [SerializeField] private TerrainGenerator terrainGenerator;
    [SerializeField] private ChunkPhysics chunkPhysics;
    [SerializeField] private Hotbar hotbar;
    [SerializeField] private Inventory inventory;
    [SerializeField] private Animator itemAnimator;
    [SerializeField] private TileEntityManager tileEntityManager;

    [Header("Other")]

    public LayerMask groundLayer;
    public Text selectedItem;
    public string lampHexColor = "FFFFFF";
    public bool colliding;

    float maxDist = 4;

    private int blockTypeHeld; // field
    public int BlockTypeHeld   // property
    {
        get { return blockTypeHeld; }
        set { blockTypeHeld = value; }
    }

    [Header("Block Breaking")]

    public GameObject placeBlockPrefab;
    public GameObject highlightPrefab;
    public GameObject blockBreakPrefab;

    public Material material;
    public Texture[] breakingSprites;

    public int blockID;
    public float timeElapsed;
    public float blockHardness;
    public int toolMultiplier;

    int blockHealth;
    Vector3Int oldLocalPos;
    RaycastHit hitInfo;

    // Update is called once per frame
    void Update()
    {
        BlockTypeHeld = hotbar.blockID;
        selectedItem.text = inventory.itemDatabase.FindByItemID(hotbar.itemID).name;

        CheckInputs();

        if (hitInfo.collider == null || hitInfo.collider.tag == "PhysicsChunk")
        {
            highlightPrefab.SetActive(false);
            blockBreakPrefab.SetActive(false);

            material.SetTexture("_MainTex", breakingSprites[9]);
            timeElapsed = 0;
            blockHealth = 10;
        }
        else
        {
            highlightPrefab.SetActive(true);
            blockBreakPrefab.SetActive(true);
        }
    }

    void CheckInputs()
    {
        bool leftClick = Input.GetMouseButtonDown(0);
        bool rightClick = Input.GetMouseButtonDown(1);

        // ---- ANIMATION ---- //

        if (leftClick)
        {
            itemAnimator.SetBool("Attack", true);
        }

        if (!Input.GetMouseButton(0) || !leftClick)
        {
            itemAnimator.SetBool("Attack", false);
        }

        // ---- SETTING UP RAYCAST AND OTHER IMPORTANT INFORMATION ---- //

        if (Physics.Raycast(cam.transform.position, cam.transform.forward, out hitInfo, maxDist, groundLayer))
        {
            Vector3 pointInTargetBlock;

            //destroy
            if (!rightClick)
                pointInTargetBlock = hitInfo.point + cam.transform.forward * .01f; //move a little inside the block
            else
                pointInTargetBlock = hitInfo.point - cam.transform.forward * .01f;

            // ---- NORMAL CHUNK UPDATES ---- //

            if (hitInfo.collider.tag != "PhysicsChunk")
            {
                // ---- SETTING UP ---- //

                TerrainChunk tc = ChunkDataUtilities.GetChunk(pointInTargetBlock);
                Vector3Int localPos = ChunkDataUtilities.GetLocalBlockPosition(tc, pointInTargetBlock);

                placeBlockPrefab.transform.position = GetPlaceBlockPos(hitInfo);
                highlightPrefab.transform.position = GetRemoveBlockPos(hitInfo);
                blockBreakPrefab.transform.position = GetRemoveBlockPos(hitInfo);

                // ---- BLOCK UPDATES ---- //

                PlaceLampUpdate(localPos, tc, localPos.x, localPos.y, localPos.z);

                RemoveBlockUpdate(localPos, tc, localPos.x, localPos.y, localPos.z);

                PlaceBlockUpdate(localPos, tc, localPos.x, localPos.y, localPos.z);

                // ---- SAVING ---- //

                if (Input.GetMouseButton(1) || Input.GetMouseButton(0))
                {
                    if (!terrainGenerator.saveManager.toSave.Contains(tc))
                        terrainGenerator.saveManager.toSave.Enqueue(tc);
                }

                // ---- BOOM ---- //

                if (Input.GetButtonDown("Y"))
                {
                    CreateSphere(32, localPos, tc);
                }
            }

            // ---- PHYSICS CHUNK UPDATES ---- //

            else
            {
                PhysicsChunk physicsChunk = hitInfo.collider.GetComponent<PhysicsChunk>();

                Vector3 localPos = physicsChunk.transform.InverseTransformPoint(pointInTargetBlock);

                //index of the target block
                int bix = Mathf.FloorToInt(localPos.x);
                int biy = Mathf.FloorToInt(localPos.y);
                int biz = Mathf.FloorToInt(localPos.z);

                if (leftClick && physicsChunk.blocks[bix, biy, biz] != terrainGenerator.blockArray[7]) // Replace block with air
                {
                    physicsChunk.blocks[bix, biy, biz] = 0;
                    physicsChunk.CheckForUnattachedBlocks(bix, biy, biz);
                    physicsChunk.BuildMesh();
                }
                else if (rightClick)
                {
                    physicsChunk.blocks[bix, biy, biz] = blockTypeHeld;
                    physicsChunk.BuildMesh();
                }
            }
        }
    }

    // ------------------------------------------- //
    // ---- SECTION FOR UPDATING VOXEL BLOCKS ---- //
    // ------------------------------------------- //

    void PlaceBlockUpdate(Vector3Int localPos, TerrainChunk tc, int bix, int biy, int biz)
    {
        if (colliding) { return; }

        if (Input.GetMouseButtonDown(1) && !Input.GetButton("H") && !Input.GetButton("G"))
        {
            if (blockTypeHeld != 0)
            {
                itemAnimator.Play("SwingAnim");

                inventory.RemoveItem(hotbar.slotIndex, 1);

                ChunkUpdater.PlaceBlockUpdate(localPos, blockTypeHeld, tc);

                inventory.itemDatabase.blocks[blockTypeHeld].OnPlace(tileEntityManager, localPos, tc);
            }
        }
    }

    void RemoveBlockUpdate(Vector3Int localPos, TerrainChunk tc, int bix, int biy, int biz)
    {
        if (Input.GetMouseButtonUp(0)) // Reset block breaking information
        {
            material.SetTexture("_MainTex", breakingSprites[9]);
            timeElapsed = 0;
            blockHealth = 9;
        }

        if (tc.blocks[bix, biy, biz] != terrainGenerator.blockArray[7] && tc.blocks[bix, biy, biz] != 0)//replace block with air
        {
            if (Input.GetMouseButton(0))
            {
                float breakTime = blockHardness * 1.5f / toolMultiplier / 9f; // How long to break a block divided by 9
                timeElapsed += Time.deltaTime;

                itemAnimator.SetBool("Attack", true);

                // Reset and setup with new information

                if (oldLocalPos != localPos)
                {
                    oldLocalPos = localPos;
                    blockID = tc.blocks[bix, biy, biz];
                    blockHardness = inventory.itemDatabase.blocks[blockID].hardness;
                    timeElapsed = 0;
                    blockHealth = 9;
                }

                if (timeElapsed >= breakTime && blockHealth > 0)
                {
                    blockHealth -= 1;
                    material.SetTexture("_MainTex", breakingSprites[blockHealth]);
                    timeElapsed = 0;
                }

                if (blockHealth == 0)
                {
                    material.SetTexture("_MainTex", breakingSprites[9]);
                    timeElapsed = 0;
                    blockHealth = 9;

                    inventory.AddItem(terrainGenerator.itemDatabase.FindByBlockID(blockID).itemID, 1); // Add item to inventory

                    ChunkUpdater.RemoveBlockUpdate(chunkPhysics, localPos, tc);

                    inventory.itemDatabase.blocks[blockID].OnDestroy(tileEntityManager, tc.chunkPos3D + localPos);
                }
            }
        }
    }

    // ------------------------------------------- //
    // ---- SECTION FOR UPDATING VOXEL LIGHTS ---- //
    // ------------------------------------------- //

    void PlaceLampUpdate(Vector3Int localPos, TerrainChunk tc, int bix, int biy, int biz)
    {
        if (Input.GetButton("H") && Input.GetMouseButtonDown(1))
        {
            if (tc.blocks[localPos.x, localPos.y, localPos.z] == 0)
            {
                int intensity = VoxelLightHelper.GetCombinedLight(tc, localPos);

                if (blockTypeHeld == terrainGenerator.itemDatabase.ReturnBlockIDByName("Sponge"))
                {
                    intensity = intensity & 0xFFF0 | 15; //Blue
                }
                else if (blockTypeHeld == terrainGenerator.itemDatabase.ReturnBlockIDByName("Planks"))
                {
                    intensity = intensity & 0xFF0F | (15 << 4); //Green
                }
                else if (blockTypeHeld == terrainGenerator.itemDatabase.ReturnBlockIDByName("Gold"))
                {
                    intensity = intensity & 0xF0FF | (15 << 8); //Red
                }
                else if (blockTypeHeld == terrainGenerator.itemDatabase.ReturnBlockIDByName("Lamp"))
                {
                    Vector3Int color = CalculateLuminanceFromHex(lampHexColor);

                    intensity = intensity & 0xF0FF | (color.x << 8); //Red
                    intensity = intensity & 0xFF0F | (color.y << 4); //Green
                    intensity = intensity & 0xFFF0 | color.z; //Blue
                }

                tc.blocks[bix, biy, biz] = terrainGenerator.itemDatabase.ReturnBlockIDByName("Lamp");

                tc.voxelLightEngine.lampLightUpdateQueue.Enqueue(new VoxelLightEngine.LightNode(tc, localPos, intensity));
                tc.voxelLightEngine.CalculateLight();

                tc.quickUpdateFlag = true;
            }
        }

        if (Input.GetButton("G"))
        {
            Vector3 block = hitInfo.point - cam.transform.forward * .01f;

            //index of the target block
            int x = Mathf.FloorToInt(block.x) - tc.chunkPos3D.x;
            int y = Mathf.FloorToInt(block.y);
            int z = Mathf.FloorToInt(block.z) - tc.chunkPos3D.z;

            Vector3Int blockPos = new Vector3Int(x, y, z);

            Debug.Log(VoxelLightHelper.GetRedLight(tc, blockPos));
            Debug.Log(VoxelLightHelper.GetGreenLight(tc, blockPos));
            Debug.Log(VoxelLightHelper.GetBlueLight(tc, blockPos));
            Debug.Log(VoxelLightHelper.GetSunlight(tc, blockPos));
        }
    }

    // ------------------------------------------- //
    // ------------- OTHER FUNCTIONS ------------- //
    // ------------------------------------------- //

    public static async void CreateSphere(int r, Vector3 center, TerrainChunk tc)
    {
        List<TerrainChunk> toUpdate = new List<TerrainChunk>();

        await Task.Run(() =>
        {
            int r2 = r * r;
            for (int y = -r; y <= r; ++y)
                for (int x = -r; x <= r; ++x)
                    for (int z = -r; z <= r; ++z)
                        if (new Vector3(x, y, z).sqrMagnitude <= r2)
                        {
                            ChunkDataUtilities.SetBlockType((int)center.x + x, (int)center.y + y, (int)center.z + z, tc, 0, toUpdate);
                        }

            foreach (TerrainChunk chunk in toUpdate)
            {
                chunk.voxelLightEngine.CalculateLight();
            }

            tc.voxelLightEngine.CalculateLight();
            tc.updateFlag = true;
        });
    }

    Vector3 GetRemoveBlockPos(RaycastHit hitInfo)
    {
        Vector3 pointInTargetBlock = hitInfo.point + cam.transform.forward * .01f;

        //index of the target block
        int bix = Mathf.FloorToInt(pointInTargetBlock.x);
        int biy = Mathf.FloorToInt(pointInTargetBlock.y);
        int biz = Mathf.FloorToInt(pointInTargetBlock.z);

        return new Vector3(bix + 0.5f, biy + 0.5f, biz + 0.5f);
    }

    Vector3 GetPlaceBlockPos(RaycastHit hitInfo)
    {
        Vector3 pointInTargetBlock = hitInfo.point - cam.transform.forward * .01f;

        //index of the target block
        int bix = Mathf.FloorToInt(pointInTargetBlock.x);
        int biy = Mathf.FloorToInt(pointInTargetBlock.y);
        int biz = Mathf.FloorToInt(pointInTargetBlock.z);

        return new Vector3(bix + 0.5f, biy + 0.5f, biz + 0.5f);
    }

    // ------------------------------------------- //
    // ------------- COLOR FUNCTIONS ------------- //
    // ------------------------------------------- //

    public static Vector3Int CalculateLuminanceFromHex(string hex)
    {
        int rgb = Convert.ToInt32(hex, 16);

        int r = (rgb & 0xff0000) >> 16;
        int g = (rgb & 0xff00) >> 8;
        int b = (rgb & 0xff);

        int cR = (int)Math.Floor(r / 16f);
        int cG = (int)Math.Floor(g / 16f);
        int cB = (int)Math.Floor(b / 16f);

        return new Vector3Int(cR, cG, cB);
    }

    public static Vector3Int CalculateRandomColor()
    {
        Color color = UnityEngine.Random.ColorHSV(0f, 1f, 1f, 1f, 1f, 1f);

        int cR = (int)Math.Floor(color.r * 255 / 16);
        int cG = (int)Math.Floor(color.g * 255 / 16);
        int cB = (int)Math.Floor(color.b * 255 / 16);

        return new Vector3Int(cR, cG, cB);
    }
}




