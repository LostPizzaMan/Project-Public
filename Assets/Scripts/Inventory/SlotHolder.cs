using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class SlotHolder : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
	public int id;
	private Inventory inv;

	void Start()
	{
		inv = GameObject.Find("Inventory").GetComponent<Inventory>();
	}

	//Detect if the Cursor starts to pass over the GameObject
	public void OnPointerEnter(PointerEventData pointerEventData)
	{
		inv.isOverSlot = true;

		inv.currentSlotHovered = id;

		if (inv.holdingShift && inv.currentlySelected)
		{
			ItemHandler droppedItem = inv.currentlySelected.GetComponent<ItemHandler>();

			if (inv.items[id].id == 0)
			{
				if (droppedItem.quantity > 1)
				{				
					GameObject newObj = inv.CreateItem(id, droppedItem.item);

					inv.items[id].id = droppedItem.item.itemID;
					inv.items[id].itemHandler.quantity = 1;
					newObj.GetComponent<ItemHandler>().quantityText.text = inv.items[id].itemHandler.quantity.ToString();

					droppedItem.quantity -= 1;
					droppedItem.quantityText.text = droppedItem.quantity.ToString();
				}
			}
		}
	}

	//Do this when the cursor exits the rect area of this selectable UI object.
	public void OnPointerExit(PointerEventData eventData)
	{
		inv.isOverSlot = false;
	}

	public void OnPointerClick(PointerEventData pointerEventData)
	{
		if (inv.currentlySelected != null && !inv.holdingShift)
		{
			ItemHandler droppedItem = inv.currentlySelected.GetComponent<ItemHandler>();

			if (pointerEventData.button == PointerEventData.InputButton.Left)
			{
				if (inv.items[id].id == 0)
				{
					droppedItem.SetItemToSlot(id);

					inv.items[id].id = droppedItem.item.itemID;
					inv.items[id].itemHandler = droppedItem;

					droppedItem.slotID = id;
				}
			}

			if (pointerEventData.button == PointerEventData.InputButton.Right)
			{
				if (inv.items[id].id == 0)
				{
					if (droppedItem.quantity > 1)
					{
						GameObject newObj = inv.CreateItem(id, droppedItem.item);

						inv.items[id].id = droppedItem.item.itemID;
						inv.items[id].itemHandler.quantity = 1;
						newObj.GetComponent<ItemHandler>().quantityText.text = inv.items[id].itemHandler.quantity.ToString();

						droppedItem.quantity -= 1;
						droppedItem.quantityText.text = droppedItem.quantity.ToString();
					}
					else // Same logic as left click 
					{
						droppedItem.SetItemToSlot(id);

						inv.items[id].id = droppedItem.item.itemID;
						inv.items[id].itemHandler = droppedItem;				

						droppedItem.slotID = id;
					}
				}
			}
		}
	}
}

