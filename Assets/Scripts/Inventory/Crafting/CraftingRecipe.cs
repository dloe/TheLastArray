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
public struct ItemResult
{
    public Item.ItemType itemType;
    public ItemData itemData;
}

[CreateAssetMenu]
public class CraftingRecipe : ScriptableObject
{
    public List<ResourceRequirement> Requirements;
    
    public ItemResult Result;

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
                    if(requirement.Amount <= player.ScrapCount)
                    {
                        result = true;
                    }
                    break;
                case Resource.ResourceType.meds:
                    if (requirement.Amount <= player.MedsCount)
                    {
                        result = true;
                    }
                    break;
                case Resource.ResourceType.cloth:
                    if (requirement.Amount <= player.ClothCount)
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
        return result;
    }

    /// <summary>
    /// Crafts Item and Removes Required Resources 
    /// </summary>
    /// <param name="player">The Player</param>
    public void Craft(Player player)
    {
        if(player.inventory.IsFull())
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
                        player.ScrapCount -= requirement.Amount;
                        break;
                    case Resource.ResourceType.meds:
                        player.MedsCount -= requirement.Amount;
                        break;
                    case Resource.ResourceType.cloth:
                        player.ClothCount -= requirement.Amount;
                        break;
                    default:
                        break;
                }
                
            }
            player.inventory.AddItem(new Item(Result.itemType, Result.itemData));
        }

        player.craftingTable.UpdateCraftingTable();
    }
}
