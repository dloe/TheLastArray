using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class WorldItem : Activatable
{
    public ItemData worldItemData;

    private void Start()
    {
        
        gameObject.GetComponentInChildren<Text>().text = worldItemData.itemName;
        
    }

    public override void Activate()
    {
        if (!Player.Instance.inventory.IsFull())
        {
            Player.Instance.inventory.AddItem(new Item(worldItemData));
            Destroy(gameObject);
            Player.Instance.thingToActivate = null;
        }
        else
        {
            Debug.Log("Can't add " + worldItemData.itemName + " to inventory because its full");
        }
    }

}

