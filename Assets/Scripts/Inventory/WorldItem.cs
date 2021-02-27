using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class WorldItem : MonoBehaviour
{
    public Item.ItemType itemType;
    public ItemData worldItemData;

    private void Start()
    {
        
        gameObject.GetComponentInChildren<Text>().text = worldItemData.itemName;
        
    }

}

