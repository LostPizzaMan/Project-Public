using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemGiver : MonoBehaviour
{
    [SerializeField] private Inventory inventory;

    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private GameObject blockPrefab;

    // Start is called before the first frame update
    void Start()
    {
        foreach (ItemBase item in inventory.itemDatabase.items)
        {
            if (item.itemID == 0 || item.name == "Air")
            {
                continue;
            }

            if (item.itemType == "Item")
            {
                GameObject prefab = Instantiate(itemPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                prefab.transform.SetParent(transform);
                prefab.transform.localScale = Vector3.one;
                prefab.transform.localPosition = Vector3.zero;

                InfiniteItem infiniteItem = prefab.GetComponent<InfiniteItem>();
                infiniteItem.inventory = inventory;
                infiniteItem.itemID = item.itemID;

                prefab.GetComponent<Image>().sprite = item.sprite;
            }

            if (item.itemType == "Block")
            {
                BlockItem blockItem = item as BlockItem;

                GameObject prefab = Instantiate(blockPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                prefab.transform.SetParent(transform);
                prefab.transform.localScale = Vector3.one;
                prefab.transform.localPosition = Vector3.zero;

                InfiniteItem infiniteItem = prefab.GetComponentInChildren<InfiniteItem>();
                infiniteItem.inventory = inventory;
                infiniteItem.itemID = item.itemID;
                infiniteItem.blockPreview.blockID = blockItem.blockID;
                infiniteItem.blockPreview.textureDatabase = inventory.itemDatabase.textureDatabase;
                
            }
        }
    }
}
