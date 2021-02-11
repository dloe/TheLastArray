using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    private List<Item> itemList;

    public Item selectedItem;
    public InventoryUI invUI;

    public Inventory()
    {
        itemList = new List<Item>();
        Debug.Log("Inventory Created");
        invUI = InventoryUI.Instance;
    }

   public void AddItem(Item item)
    {
        itemList.Add(item);
        Debug.Log("Item Added: " + item.itemType);
    }

    public void RemoveItem(Item item)
    {
        itemList.Remove(item);
    }

    public bool isFull()
    {
        return itemList.Count == 4;
    }
}
