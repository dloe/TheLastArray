using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    private List<Item> itemList;

    public Item selectedItem;
    public InventoryUI invUI;
    public int Count
    {
        get
        {
            return itemList.Count;
        }
    }

    public Inventory()
    {
        itemList = new List<Item>();
        Debug.Log("Inventory Created");
        invUI = InventoryUI.Instance;
    }



    public bool Contains(ItemData itemData)
    {
        bool result = false;
        foreach(Item item in itemList)
        {
            if(item.itemData.itemName == itemData.itemName)
            {
                result = true;
                break;
            }
        }
        return result;
    }

   public void AddItem(Item item)
    {
        itemList.Add(item);
        Debug.Log("Item Added: " + item.itemData.itemType);
        invUI.RefreshUI();
    }

    public void RemoveItem(Item item)
    {
        itemList.Remove(item);
        Debug.Log("Item Removed: " + item.itemData.itemType);
        invUI.RefreshUI();
    }
    public void DropItem()
    {
        invUI.SpawnItem(selectedItem);
        RemoveItem(selectedItem);
    }

    public bool IsFull()
    {
        
        return itemList.Count == invUI.slotList.Count;
    }

    public void Equip(int slotIndex)
    {
        if(slotIndex > itemList.Count-1)
        {
            selectedItem = null;
            Debug.Log("Item Equipped: none");
        }
        else
        {
            selectedItem = itemList[slotIndex];
            Debug.Log("Item Equipped: " + selectedItem.itemData.itemType);
        }
        
    }

    public Item ItemAtIndex(int slotIndex)
    {
        Item result;
        if(slotIndex <= Count-1)
        {
            result = itemList[slotIndex];
        }
        else
        {
            result = null;
        }

        return result;
    }
}
