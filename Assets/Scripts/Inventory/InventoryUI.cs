﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;
    public Transform slotParent;
    //public Transform selector;
    private Player player;
    public Inventory inventory;
    public int selectedItemIndex = 0;
    public List<Transform> slotList = new List<Transform>();

    public GameObject emptyWorldItem;

    private readonly string _zoomAxis = "Mouse ScrollWheel";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        player = Player.Instance;
        inventory = player.inventory;
        foreach(Transform slot in slotParent)
        {
            slotList.Add(slot);
        }
        //selector.parent = slotParent;
        RefreshUI();
    }

    // Update is called once per frame
    void Update()
    {
        if(!UI.Instance.PausedStatus)
        {
            if (Input.GetAxis(_zoomAxis) < 0)
            {
                SetIndex(selectedItemIndex + 1);
            }
            else if (Input.GetAxis(_zoomAxis) > 0)
            {
                SetIndex(selectedItemIndex - 1);
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SetIndex(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SetIndex(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                SetIndex(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                SetIndex(3);
            }
        }
       

        
    }

    void SetIndex(int index)
    {
        

        if(index > slotList.Count-1)
        {
            selectedItemIndex = 0;
        }
        else if(index < 0)
        {
            selectedItemIndex = slotList.Count - 1;
        }
        else
        {
            selectedItemIndex = index;
        }

        
        RefreshUI();

        
    }

    public void RefreshUI()
    {
        for (int index = 0; index < slotList.Count; index++)
        {
            if(inventory.ItemAtIndex(index) != null)
            {
                slotList[index].GetComponentInChildren<Text>().text = inventory.ItemAtIndex(index).itemData.itemName;
                
            }
            else
            {
                slotList[index].GetComponentInChildren<Text>().text = "none";
            }
        }
        SelectSlot(selectedItemIndex);
        inventory.Equip(selectedItemIndex);
    }

    public void SpawnItem(Item item)
    {
        Vector3 dropPos = new Vector3(player.transform.position.x, 1, player.transform.position.z);
        WorldItem worldItem = Instantiate(emptyWorldItem, dropPos, emptyWorldItem.transform.rotation).GetComponent<WorldItem>();

        worldItem.itemType = item.itemType;
        worldItem.worldItemData = item.itemData;
        
    }

    private void SelectSlot(int slotIndex)
    {
        for(int index = 0; index < slotList.Count; index ++)
        {
            if(slotIndex == index)
            {
                slotList[index].GetComponent<Image>().color = Color.blue;
            }
            else
            {
                slotList[index].GetComponent<Image>().color = Color.white;
            }
        }

    }
}
