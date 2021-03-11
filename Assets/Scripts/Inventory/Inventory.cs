using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

[System.Serializable]
public class Inventory
{
    [SerializeField]
    private List<Item> itemList;

    [HideInInspector]
    public Item selectedItem;

    public int numInvSlots;

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
       // Debug.Log("Inventory Created");
        
    }

    public Inventory(Inventory invToCopy)
    {
        itemList = new List<Item>(invToCopy.itemList);
        //Debug.Log("Inventory Created");
        
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

    public bool Contains(Item item)
    {
        bool result = false;

        foreach (Item invItem in itemList)
        {
            if (invItem.itemData.itemName == item.itemData.itemName)
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

        if(InventoryUI.Instance)
        {
            InventoryUI.Instance.RefreshUI();
        }
        else
        {
            Debug.LogWarning("Warning, Inventory UI was not found");
        }
    }

    public void AddItem(ItemData itemData)
    {
        itemList.Add(new Item(itemData));
        if (InventoryUI.Instance)
        {
            InventoryUI.Instance.RefreshUI();
        }
        else
        {
            Debug.LogWarning("Warning, Inventory UI was not found");
        }
    }

    public void AddItemNoUI(ItemData itemData)
    {
        itemList.Add(new Item(itemData));

    }



    public void RemoveItem(Item item)
    {
        itemList.Remove(item);
        if (InventoryUI.Instance)
        {
            InventoryUI.Instance.RefreshUI();
        }
        else
        {
            Debug.LogWarning("Warning, Inventory UI was not found");
        }
    }

    public void RemoveItem(ItemData itemData)
    {
        itemList.Remove(new Item(itemData));
        if (InventoryUI.Instance)
        {
            InventoryUI.Instance.RefreshUI();
        }
        else
        {
            Debug.LogWarning("Warning, Inventory UI was not found");
        }
    }

    public void Clear()
    {
        foreach(Item item in itemList)
        {
            if(item.itemData.name.Contains("Instance"))
            {
                Object.Destroy(item.itemData);
            }
            
        }
        itemList.Clear();
        selectedItem = null;
        Debug.Log("Inventory Cleared Count is Now: " + Count);
    }

    public void DropItem()
    {
        InventoryUI.Instance.SpawnItem(selectedItem);
        RemoveItem(selectedItem);
    }

    public bool IsFull()
    {
        
        return itemList.Count == InventoryUI.Instance.slotList.Count;
    }

    public void Equip(int slotIndex)
    {
        if(slotIndex > itemList.Count-1)
        {
            selectedItem = null;
            //Debug.Log("Item Equipped: none");
        }
        else
        {
            selectedItem = itemList[slotIndex];
            //Debug.Log("Item Equipped: " + selectedItem.itemData.itemType);
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

    //public List<Item> GetItemList()
    //{
    //    return itemList;
    //}

    public List<string> SaveToJsonList()
    {
        List<string> jsonList = new List<string>();
        ItemDataSave itemSave;
        foreach (Item item in itemList)
        {
            itemSave = new ItemDataSave();
            itemSave.SaveFromItemData(item.itemData);
            jsonList.Add(JsonUtility.ToJson(item.itemData));
        }

        return jsonList;
    }

    public void LoadFromJsonList(List<string> jsonList)
    {
        ItemDataSave itemSave;
        ItemData itemData;
        foreach (string json in jsonList)
        {
            itemSave = new ItemDataSave();
            JsonUtility.FromJsonOverwrite(json, itemSave);
            itemData = ScriptableObject.CreateInstance<ItemData>();
            itemSave.LoadToItemData(itemData);

            AddItemNoUI(itemData);
        }
    }
}
