using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    [Header("GUI")]

    public Canvas canvas;

    public Transform hotbar;
    public Transform inventory;
    public Transform crafting; // temp

    public Slot[] items = new Slot[40];
    public Slot[] craftingGrid = new Slot[40]; // temp

    public List<GameObject> slots = new List<GameObject>();

    [Header("Database")]

    public ItemDatabase itemDatabase;

    [Header("Templates")]

    public GameObject inventoryItem;
    public GameObject inventoryBlockItem;
    public GameObject slotItem;

    int count;

    // Drag and Drop Variables

    [Header("Drag n' Drop")]

    public GameObject currentlySelected;

    public bool isDragging;
    public bool holdingShift { get; private set; }
    public bool isOverSlot;
    public int currentSlotHovered;

    void Start()
    {
        CreateSlots(hotbar, 9);
        CreateSlots(inventory, 27);
        CreateSlots(crafting, 4);
        AddItem(itemDatabase.FindByStringID("Apple"), 19);
        AddItem(itemDatabase.FindByStringID("Diamond_Sword"), 1);
        AddItem(itemDatabase.FindByStringID("Cookie"), 3);
        AddItem(itemDatabase.FindByStringID("Cobblestone"), 37);
    }

    void Update()
    {
        holdingShift = Input.GetKey(KeyCode.LeftShift) && Input.GetMouseButton(1);
    }

    void UpdateCraftingGrid()
    {
        for (int i = 39; i > 35; i--)
        {
            craftingGrid[i].id = items[i].id;
        }
    }

    void CreateSlots(Transform container, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            GameObject obj = Instantiate(slotItem, container.transform);
            slots.Add(obj);

            SlotHolder slot = obj.GetComponent<SlotHolder>();
            slot.id = count;
            count++;
        }
    }

    public void AddItem(int id, int quantity)
    {
        int i = -1;
        int foundSlot = -1;
        bool sameItemSlotFound = false;
        bool emptySlotFound = false;

        ItemBase item = itemDatabase.FindByItemID(id);

        foreach (Slot element in items)
        {
            i++;

            if (element.id == id)
            {
                if (items[i].itemHandler.quantity < 64)
                {
                    sameItemSlotFound = true;
                    foundSlot = i;
                    break;
                }
            }
            else if (element.id == 0)
            {
                emptySlotFound = true;
                foundSlot = i;
                break;
            }
        }

        if (sameItemSlotFound)
        {           
            int availableSpace = Mathf.Abs(items[foundSlot].itemHandler.quantity - 64); // How much available space in the stack out of the 64.

            if (quantity <= availableSpace)
            {
                items[foundSlot].itemHandler.quantity += quantity;
            }
            else if (quantity > availableSpace) // Create a new stack if the quantity exceeds the available space in the stack.
            {
                quantity -= availableSpace;
                items[foundSlot].itemHandler.quantity += availableSpace;

                AddItem(id, quantity);
            }

            slots[foundSlot].transform.GetChild(0).GetComponent<ItemHandler>().quantityText.text = items[foundSlot].itemHandler.quantity.ToString(); 
        }
        else if (emptySlotFound)
        {
            CreateItem(foundSlot, item);

            items[foundSlot].id = id;
            items[foundSlot].itemHandler.quantity += quantity;
            items[foundSlot].itemHandler.quantityText.text = items[foundSlot].itemHandler.quantity.ToString();
        }
        else
        {
            Debug.Log("No space found");
        }
    }

    public GameObject CreateItem(int _slotID, ItemBase _item)
    {
        GameObject itemObj;

        if (_item.itemType == "Item")
        {
            itemObj = Instantiate(inventoryItem, transform);
        }
        else 
        {
            itemObj = Instantiate(inventoryBlockItem, transform);
        }

        ItemHandler itemHandler = itemObj.GetComponent<ItemHandler>();

        items[_slotID].itemHandler = itemHandler;

        if (_item.itemType == "Block")
        {
            BlockItem blockItem = _item as BlockItem;

            BlockPreview blockPreview = itemObj.transform.GetChild(0).GetComponent<BlockPreview>();
            blockPreview.blockID = blockItem.blockID;
            blockPreview.textureDatabase = itemDatabase.textureDatabase;
            blockPreview.BuildMesh();

        }

        itemHandler.image.sprite = _item.sprite;
        itemHandler.slotID = _slotID;
        itemHandler.item = _item;
        itemHandler.inv = this;
        itemHandler.parentCanvas = canvas;

        itemObj.transform.SetParent(slots[_slotID].transform);
        itemObj.transform.position = slots[_slotID].transform.position;
        itemObj.transform.name = _item.name;
        itemObj.transform.localScale = Vector3.one;
        itemObj.transform.localPosition = Vector3.zero;

        return itemObj;
    }

    public GameObject CreateItemNoAssign(ItemBase _item, int quantity)
    {
        GameObject itemObj;

        if (_item.itemType == "Item")
        {
            itemObj = Instantiate(inventoryItem, transform);
        }
        else
        {
            itemObj = Instantiate(inventoryBlockItem, transform);
        }

        ItemHandler itemHandler = itemObj.GetComponent<ItemHandler>();

        if (_item.itemType == "Block")
        {
            BlockItem blockItem = _item as BlockItem;

            BlockPreview blockPreview = itemObj.transform.GetChild(0).GetComponent<BlockPreview>();
            blockPreview.blockID = blockItem.blockID;
            blockPreview.textureDatabase = itemDatabase.textureDatabase;
            blockPreview.BuildMesh();
        }

        itemHandler.image.sprite = _item.sprite;
        itemHandler.item = _item;
        itemHandler.inv = this;
        itemHandler.parentCanvas = canvas;

        itemObj.transform.name = _item.name;
        itemObj.transform.localScale = Vector3.one;
        itemObj.transform.localPosition = Vector3.zero;

        itemHandler.quantity += quantity;
        itemHandler.quantityText.text = itemHandler.quantity.ToString();

        return itemObj;
    }

    public void RemoveItem(int slotID, int quantity)
    {
        items[slotID].itemHandler.quantity -= quantity;
        slots[slotID].transform.GetChild(0).GetComponent<ItemHandler>().quantityText.text = items[slotID].itemHandler.quantity.ToString();

        if (items[slotID].itemHandler.quantity < 1)
        {
            items[slotID].id = 0;
            Destroy(slots[slotID].transform.GetChild(0).gameObject);
        }
    }

    public struct Slot
    {
        public int id;
        public ItemHandler itemHandler;

        public Slot(int _id, int _quantity, ItemHandler _itemHandler)
        {
            id = _id;
            itemHandler = _itemHandler;
        }
    }
}

