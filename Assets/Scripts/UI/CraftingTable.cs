﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingTable : MonoBehaviour
{
    public GameObject Menu;
    public GameObject optionParent;
    public GameObject craftingOptionPrefab;
    
    public List<CraftingRecipe> recipes;

    private void Start()
    {
        Menu = UI.Instance.transform.Find("Crafting Menu").gameObject;
        Debug.Log(Menu.name);
        optionParent = Menu.transform.Find("Scroll View").Find("Viewport").Find("Content").gameObject;
        CraftingOption tempOption;
        foreach (CraftingRecipe recipe in recipes)
        {
            tempOption = Instantiate(craftingOptionPrefab, optionParent.transform).GetComponent<CraftingOption>();
            tempOption.recipe = recipe;
            tempOption.itemName.text = recipe.Result.ToString();
            foreach (ResourceRequirement requirement in recipe.Requirements)
            {
                switch (requirement.Resource)
                {
                    case Resource.ResourceType.scrap:
                        tempOption.scrapText.text = requirement.Amount.ToString();
                        break;
                    case Resource.ResourceType.meds:
                        tempOption.medsText.text = requirement.Amount.ToString();
                        break;
                    case Resource.ResourceType.cloth:
                        tempOption.clothText.text = requirement.Amount.ToString();
                        break;
                    default:
                        break;
                }
            }

           
            tempOption.craftButton.onClick.AddListener(() => recipe.Craft(Player.Instance));
            if (!recipe.IsCraftable(Player.Instance))
            {
                tempOption.craftButton.interactable = false;
            }
        }
    }

    public void ActivateMenu()
    {
        Menu.SetActive(true);
        UpdateCraftingTable();
    }

    public void DeactivateMenu()
    {
        Menu.SetActive(false);
    }

    public void UpdateCraftingTable()
    {
        foreach (Transform option in optionParent.transform)
        {
            option.GetComponent<CraftingOption>().craftButton.interactable = option.GetComponent<CraftingOption>().recipe.IsCraftable(Player.Instance);
        }
    }
}
