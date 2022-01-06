using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemHandler : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler
{
	public ItemBase item;
	public int quantity;
	public int slotID; // The id of the slot it's sitting in

	public Text quantityText;
	public bool isDragging;

	private Vector2 offset;

	[HideInInspector]
	public Canvas parentCanvas;
	[HideInInspector]
	public Inventory inv;

	public GameObject blockPreview;
	public Image image;

	public void OnPointerDown(PointerEventData pointerEventData)
	{
		offset = pointerEventData.position - new Vector2(transform.position.x, transform.position.y);

		if (pointerEventData.button == PointerEventData.InputButton.Left) // Lets you take the full stack
		{
			if (!isDragging && !inv.isDragging)
			{
				inv.items[slotID].id = 0;
				inv.items[slotID].itemHandler = null;

				SetItemToDrag(gameObject);
			}
			else
			{
				AddItemToSameItemSlot_Stack(); // If item being dragged is the same as this one then add the full stack
			}
		}
		if (pointerEventData.button == PointerEventData.InputButton.Right) // Lets you split in stack in half
		{
			if (!isDragging && !inv.isDragging)
			{
				if (quantity > 1)
				{
					int newQuantity = Mathf.FloorToInt(quantity / 2f);

					quantity -= newQuantity;
					quantityText.text = quantity.ToString();

					GameObject obj = inv.CreateItemNoAssign(item, newQuantity);

					SetItemToDrag(obj);
				}
			}
			else
			{
				AddItemToSameItemSlot_Single(); // If item being dragged is the same as this one then add it one at a time
			}
		}
	}

	public void OnPointerEnter(PointerEventData pointerEventData)
	{
		if (inv.holdingShift && inv.currentlySelected && !inv.isOverSlot) // Lets you shift click to drag items into inventory
		{
			AddItemToSameItemSlot_Single();
			Debug.Log("Hovered Above Item: " + transform.name);
		}
	}

	void Update()
    {
		if (isDragging)
        {
			DragItem();
		}
    }

	void DragItem()
    {
		Vector2 movePos;

		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			parentCanvas.transform as RectTransform,
			Input.mousePosition, parentCanvas.worldCamera,
			out movePos);

		transform.position = parentCanvas.transform.TransformPoint(movePos);
		transform.localScale = Vector3.one; // Setting parent resets scale so we have to set it again manually.

		if (blockPreview != null)
		{
			blockPreview.transform.localPosition = new Vector3(0, 0, -96);
			quantityText.transform.localPosition = new Vector3(quantityText.transform.localPosition.x, quantityText.transform.localPosition.y, -112);

		}
		else
		{
			image.transform.localPosition = new Vector3(0, 0, -90);
			quantityText.transform.localPosition = new Vector3(quantityText.transform.localPosition.x, quantityText.transform.localPosition.y, -90);
		}
	}

	public void SetItemToSlot(int id)
	{
		isDragging = false;
		transform.SetParent(inv.slots[id].transform);
		transform.position = inv.slots[id].transform.position;
		transform.localScale = Vector3.one; // Setting parent resets scale so we have to set it again manually.

		ResetUI(this); 

		GetComponent<CanvasGroup>().blocksRaycasts = true;
		inv.currentlySelected = null;
		inv.isDragging = false;
	}

	void SetItemToDrag(GameObject obj)
    {
		inv.currentlySelected = obj;
		inv.isDragging = true;
		obj.GetComponent<ItemHandler>().isDragging = true;
		obj.transform.SetParent(transform.parent.parent.parent);
		obj.transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y) - offset;
		obj.transform.localScale = Vector3.one;
		obj.GetComponent<CanvasGroup>().blocksRaycasts = false;
	}

	public void AddItemToSameItemSlot_Stack()
	{
		ItemHandler droppedItem = inv.currentlySelected.GetComponent<ItemHandler>();

		if (inv.items[slotID].id == droppedItem.item.itemID)
		{
			droppedItem.SetItemToSlot(droppedItem.slotID);

			int availableSpace = Mathf.Abs(quantity - 64); // How much available space in the stack out of the 64.

			int droppedItemQuantity = droppedItem.quantity;

			if (droppedItemQuantity <= availableSpace) 
			{
				quantity += droppedItemQuantity;

				Destroy(droppedItem.gameObject);
			}
			else if (droppedItemQuantity > availableSpace) // Create a new stack if the quantity exceeds the available space in the stack.
			{
				droppedItemQuantity -= availableSpace;
				quantity += availableSpace;

				inv.AddItem(droppedItem.item.itemID, droppedItemQuantity);

				Destroy(droppedItem.gameObject);
			}
		}
		else if (inv.items[slotID].id != droppedItem.item.itemID) // Swaps out the item you're holding with the item you clicked (if item is different of course)
		{
			GameObject oldItem = inv.currentlySelected;
			ItemHandler oldItemHandler = oldItem.GetComponent<ItemHandler>();

			inv.items[slotID].id = oldItemHandler.item.itemID;
			inv.items[slotID].itemHandler = oldItemHandler;

			ResetUI(oldItemHandler);

			inv.currentlySelected = null;
			inv.isDragging = false;
			oldItemHandler.isDragging = false;
			oldItemHandler.slotID = slotID;

			oldItem.transform.SetParent(inv.slots[slotID].transform);
			oldItem.transform.position = inv.slots[slotID].transform.position;
			oldItem.transform.localScale = Vector3.one; 
			oldItem.GetComponent<CanvasGroup>().blocksRaycasts = true;

			SetItemToDrag(gameObject);
		}

		quantityText.text = quantity.ToString();
	}

	void AddItemToSameItemSlot_Single()
    {
		ItemHandler droppedItem = inv.currentlySelected.GetComponent<ItemHandler>();

		if (inv.items[slotID].id == droppedItem.item.itemID)
		{
			quantity += 1;
			droppedItem.quantity -= 1;

			droppedItem.quantityText.text = droppedItem.quantity.ToString();

			if (droppedItem.quantity < 1)
			{
				droppedItem.isDragging = false;
				inv.currentlySelected = null;
				inv.isDragging = false;

				Destroy(droppedItem.gameObject);
			}
		}

		quantityText.text = quantity.ToString();
	}

	void ResetUI(ItemHandler itemHandler)
    {
		if (itemHandler.blockPreview != null) // Since blockPreview is a 3D Object, we set it's localPosition so it does not interfere with the UI
		{
			itemHandler.blockPreview.transform.localPosition = new Vector3(0, 0, -32);
			itemHandler.quantityText.transform.localPosition = new Vector3(quantityText.transform.localPosition.x, quantityText.transform.localPosition.y, -64);

		}
		else
		{
			itemHandler.image.transform.localPosition = new Vector3(0, 0, 0);
			itemHandler.quantityText.transform.localPosition = new Vector3(quantityText.transform.localPosition.x, quantityText.transform.localPosition.y, 0);
		}
	}
}
