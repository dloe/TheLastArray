using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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
    public bool isAmmoResult = false;
    public ItemData itemResult;
    [Header("ignore if isAmmoResult is false")]
    public AmmoType ammoType;
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
            if(result == false)
            {
                break;
            }
        }

        if(craftingResult.isAmmoResult && result)
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
        if(player.inventory.IsFull() && !craftingResult.isAmmoResult)
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
            if(craftingResult.isAmmoResult)
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
            else
            {
                player.inventory.AddItem(new Item(craftingResult.itemResult));
                InventoryUI.Instance.RefreshUI();
            }
            
        }

        ((CraftingTable)player.thingToActivate).UpdateCraftingTable();
    }

    
}
