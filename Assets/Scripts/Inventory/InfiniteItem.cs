using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InfiniteItem : MonoBehaviour, IPointerClickHandler
{
    public Inventory inventory { private get; set; }

    public int itemID;

    public BlockPreview blockPreview;

    public GameObject blockPreviewOBJ;

    public TextureDatabase textureDatabase;

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        inventory.AddItem(itemID, 64);
    }
}
