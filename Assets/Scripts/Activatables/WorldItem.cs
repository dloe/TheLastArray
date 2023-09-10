using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class WorldItem : Activatable
{
    /// <summary>
    /// Activatable
    /// Dylan Loe
    /// 
    /// Last Updated: 4/15/21
    /// 
    /// - activatable world item, pickup items the player can use and is stored in inventory
    /// </summary>
    
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
        if (!CheckIfExtraWeapon())
        {
            if (!Player.Instance.inventory.IsFull())
            {
                if (worldItemData.itemType == ItemType.Binoculars)
                {
                    if (!Player.Instance.inventory.Contains(worldItemData))
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

    bool CheckIfExtraWeapon()
    {
        //if its a weapon and we already have one, take the ammo from the new one and add it to the old one
        if(worldItemData.itemType == ItemType.RangedWeapon && Player.Instance.inventory.Contains(worldItemData))
        {
            Debug.Log("WorldItems: Already has this weapon (" + worldItemData.itemName + ")... Stripping ammo and destroying gun");

            Item ourWeapon = Player.Instance.inventory.Find(worldItemData);
            if (ourWeapon != null)
            {
                int newAmmot = Random.Range(ourWeapon.itemData.magSize, 2 * ourWeapon.itemData.magSize);
                if (ourWeapon.itemData.ammoType == AmmoType.HeavyAmmo)
                {
                    Player.Instance.currentHeavyAmmo += newAmmot;
                }
                else
                {
                    Player.Instance.currentLightAmmo += newAmmot;
                }

                //repaint UI
                InventoryUI.Instance.RefreshUI();

                
                Player.Instance.thingsToActivate.Remove(this);
                Destroy(gameObject);
                Player.Instance.thingToActivate = null;

                return true;
            } else {
                Debug.Log("WorldItem Warning: Could not find the right dublicate weapon.");
            }
        }

        return false;
    }
}

