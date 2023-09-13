using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;


public class Inventory
{
    /// <summary>
    /// Inventory 
    /// Jeremy Casada
    /// 
    /// Last Updated: 5/28/22
    /// 
    /// - adding items, talks with UI and player
    /// 
    /// </summary>
    
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

    //finds our corresponding item in our inventory and returns a reference
    //for picking up dublicate weapons
    public Item Find(ItemData itemData)
    {
        foreach (Item invItem in itemList)
        {
            if (invItem.itemData.itemName == itemData.itemName)
            {
                return invItem;
            }
        }

        return null;
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
        if (item.itemData.itemType == ItemType.Binoculars)
        {
            CameraController.Instance.ToggleBinocularMode(true);
        }

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
        if (itemData.itemType == ItemType.Binoculars)
        {
            CameraController.Instance.ToggleBinocularMode(true);
        }

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
        if (itemData.itemType == ItemType.Binoculars)
        {
            CameraController.Instance.ToggleBinocularMode(true);
        }
        itemList.Add(new Item(itemData));

    }

    public void RemoveItem(Item item)
    {
        if (selectedItem.itemData.itemType == ItemType.Binoculars)
        {
            CameraController.Instance.ToggleBinocularMode(false);
        }

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

    /// <summary>
    /// probably should not be used because this creates a new item in memory
    /// </summary>
    /// <param name="itemData"></param>
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

    /// <summary>
    /// Removes Item by Type, Recommended to only use for items that dont carry unique data
    /// </summary>
    /// <param name="itemType">type to remove</param>
    public void RemoveItemByType(ItemType itemType)
    {
        Item itemToRemove = null;
        foreach(Item item in itemList)
        {
            if(item.itemData.itemType == itemType)
            {
                itemToRemove = item;
                break;
            }
        }

        itemList.Remove(itemToRemove);

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
        Player.Instance.laserLine.gameObject.SetActive(false);
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
            jsonList.Add(JsonUtility.ToJson(itemSave));
        }

        return jsonList;
    }

    public void LoadFromJsonList(List<string> jsonList)
    {
        ItemDataSave itemSave;
        ItemData itemData;
        Clear();
        foreach (string json in jsonList)
        {
            itemSave = new ItemDataSave();
            JsonUtility.FromJsonOverwrite(json, itemSave);
            itemData = ScriptableObject.CreateInstance<ItemData>();
            itemSave.LoadToItemData(itemData);
           // Debug.Log(itemData.itemName + " ammo: " + itemData.loadedAmmo);
            AddItemNoUI(itemData);
        }
    }
}
