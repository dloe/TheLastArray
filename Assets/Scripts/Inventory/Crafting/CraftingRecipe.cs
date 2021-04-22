using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public enum ResultType
{
    item,
    ammo,
    attachment,
    armor,
    fireBullets
}

[Serializable]
public struct ResourceRequirement
{
    public Resource.ResourceType Resource;
    [Range(1, 30)]
    public int Amount;
}

[Serializable]
public class Result
{
    public ResultType resultType;
    public ItemData itemResult;

    [Header("Sprite for if the result is not an item")]
    public Sprite displaySprite;

    [Header("Description for if the result is not an item")]
    public string nonItemDescription;

    [Header("ignore if result type is not ammo/fire bullets")]
    public AmmoType ammoType;

    [Header("ignore if result type is not attachment")]
    public AttachType attachType;
}

[CreateAssetMenu]
public class CraftingRecipe : ScriptableObject
{
    public List<ResourceRequirement> Requirements;

    public Result craftingResult;

    [HideInInspector]
    public int amountToCraft = 1;

    /// <summary>
    /// Returns True if This Item is Craftable
    /// </summary>
    /// <param name="player">The Player</param>
    /// <returns></returns>
    public bool IsCraftable(Player player)
    {
        bool result = false;
        foreach(ResourceRequirement requirement in Requirements)
        {
            result = false;
            switch (requirement.Resource)
            {
                case Resource.ResourceType.scrap:
                    if(requirement.Amount * amountToCraft <= player.ScrapCount)
                    {
                        result = true;
                    }
                    break;
                case Resource.ResourceType.meds:
                    if (requirement.Amount * amountToCraft <= player.MedsCount)
                    {
                        result = true;
                    }
                    break;
                case Resource.ResourceType.cloth:
                    if (requirement.Amount * amountToCraft <= player.ClothCount)
                    {
                        result = true;
                    }
                    break;
                default:
                    break;
            }

            if(craftingResult.resultType == ResultType.item && craftingResult.itemResult.itemType == ItemType.BackPack && player.hasBackPack)
            {
                result = false;
            }

            if(result == false)
            {
                break;
            }
        }

        if(craftingResult.resultType == ResultType.ammo && result)
        {
            switch (craftingResult.ammoType)
            {
                case AmmoType.LightAmmo:
                    result = Player.Instance.currentLightAmmo + (amountToCraft) <= Player.Instance.maxLightAmmo;
                    break;
                case AmmoType.HeavyAmmo:
                    result = Player.Instance.currentHeavyAmmo + (amountToCraft) <= Player.Instance.maxHeavyAmmo;
                    break;
                default:
                    break;
            }
        }
        else if(craftingResult.resultType == ResultType.attachment && result)
        {
            if(player.inventory.selectedItem != null && player.inventory.selectedItem.itemData.itemType == ItemType.RangedWeapon)
            {
                switch (craftingResult.attachType)
                {
                    case AttachType.laser:
                            result = !player.inventory.selectedItem.itemData.hasLaserSight;
                            break;
                    case AttachType.tunedBarrel:
                        break;
                    default:
                        break;
                }
            }
            else
            {
                result = false;
            }
           
        }
        else if(craftingResult.resultType == ResultType.armor && result)
        {
            result = !player.hasArmorPlate;
        }
        else if(craftingResult.resultType == ResultType.fireBullets && result)
        {
            if (player.inventory.selectedItem != null && player.inventory.selectedItem.itemData.itemType == ItemType.RangedWeapon)
            {
                if(player.inventory.selectedItem.itemData.usingFireBullets)
                {
                    result = false;
                }
                else
                {
                    result = player.inventory.selectedItem.itemData.ammoType == craftingResult.ammoType;
                }
               

            }
            else
            {
                result = false;
            }
        }

        if(amountToCraft == 0)
        {
            result = false;
        }
        return result;
    }

    /// <summary>
    /// Crafts Item and Removes Required Resources 
    /// </summary>
    /// <param name="player">The Player</param>
    public void Craft(Player player)
    {
        if(player.inventory.IsFull() && craftingResult.resultType == ResultType.item && craftingResult.itemResult.itemType != ItemType.BackPack)
        {
            Debug.Log("can't craft, inventory is full chief");
        }
        else
        {
            foreach (ResourceRequirement requirement in Requirements)
            {
                switch (requirement.Resource)
                {
                    case Resource.ResourceType.scrap:
                        player.ScrapCount -= requirement.Amount * amountToCraft;
                        break;
                    case Resource.ResourceType.meds:
                        player.MedsCount -= requirement.Amount * amountToCraft;
                        break;
                    case Resource.ResourceType.cloth:
                        player.ClothCount -= requirement.Amount * amountToCraft;
                        break;
                    default:
                        break;
                }
                
            }
            if(craftingResult.resultType == ResultType.ammo)
            {
                switch (craftingResult.ammoType)
                {
                    case AmmoType.LightAmmo:
                        Player.Instance.currentLightAmmo += amountToCraft;
                        break;
                    case AmmoType.HeavyAmmo:
                        Player.Instance.currentHeavyAmmo += amountToCraft;
                        break;
                    default:
                        break;
                }

                InventoryUI.Instance.RefreshUI();
                
            }
            else if(craftingResult.resultType == ResultType.attachment)
            {
                switch (craftingResult.attachType)
                {
                    case AttachType.laser:
                        if (!Player.Instance.inventory.selectedItem.itemData.hasLaserSight)
                        {
                            Player.Instance.inventory.selectedItem.itemData.hasLaserSight = true;
                            Player.Instance.laserLine.gameObject.SetActive(true);
                            
                        }
                        break;
                    case AttachType.tunedBarrel:
                        break;
                    default:
                        break;
                }
            }
            else if( craftingResult.resultType == ResultType.armor)
            {
                player.hasArmorPlate = true;
                player.ArmorPlateImage.gameObject.SetActive(true);
            }
            else if(craftingResult.resultType == ResultType.fireBullets)
            {
                player.inventory.selectedItem.itemData.LoadFireBullets();

            }
            else
            {
                if(craftingResult.itemResult.itemType == ItemType.BackPack)
                {
                    InventoryUI.Instance.AddSlot();
                    InventoryUI.Instance.AddSlot();
                    player.hasBackPack = true;
                }
                else
                {
                    player.inventory.AddItem(new Item(craftingResult.itemResult));
                }
                
                InventoryUI.Instance.RefreshUI();
            }
            
        }

        ((CraftingTable)player.thingToActivate).UpdateCraftingTable();
    }

    
}
