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
        if(worldItemData.itemSprite != null)
        {
            gameObject.GetComponentInChildren<Image>().sprite = worldItemData.itemSprite;
        }
        gameObject.name = worldItemData.itemName + "_Interatable";

        if(!worldItemData.name.Contains("Instance"))
        {
            worldItemData.loadedAmmo = worldItemData.magSize;
        }
    }

    public override void Activate()
    {
        if (!Player.Instance.inventory.IsFull())
        {
            if (worldItemData.itemType == ItemType.Binoculars)
            {
                if(!Player.Instance.inventory.Contains(worldItemData))
                {
                    Player.Instance.inventory.AddItem(new Item(worldItemData));

                    Destroy(gameObject);
                    Player.Instance.thingToActivate = null;
                }
                else
                {
                    Debug.Log("Can't pick up more than one binoculars");
                }
                
            }
            else
            {
                Player.Instance.inventory.AddItem(new Item(worldItemData));

                Destroy(gameObject);
                Player.Instance.thingToActivate = null;
            }
            
        }
        else
        {
            Debug.Log("Can't add " + worldItemData.itemName + " to inventory because its full");
        }
    }

}

